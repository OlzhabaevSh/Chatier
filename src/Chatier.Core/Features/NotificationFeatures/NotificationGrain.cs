using Chatier.Core.Features.NotificationFeatures.Logging;
using Chatier.Core.Features.NotificationFeatures.Services;
using Chatier.Core.Features.UserFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures;

public class NotificationGrain :
    Grain,
    INotificationGrain,
    IRemindable
{
    private readonly IPersistentState<NotificationGrainState> notificationState;

    private readonly IEmailService emailService;

    private readonly ILogger<NotificationGrain> logger;

    private readonly TimeSpan reminderInterval;

    private IGrainReminder? reminder;

    public NotificationGrain(
        [PersistentState("notifications", "notificationStore")]
        IPersistentState<NotificationGrainState> notificationState,
        ILogger<NotificationGrain> logger,
        IEmailService emailService,
        IConfiguration configuration)
    {
        this.notificationState = notificationState;
        this.emailService = emailService;
        this.logger = logger;

        var reminderIntervalInMilliseconds = configuration.GetValue<int>("ReminderIntervalInMilliseconds");
        this.reminderInterval = reminderIntervalInMilliseconds > 0
            ? TimeSpan.FromMilliseconds(reminderIntervalInMilliseconds)
            : TimeSpan.FromMilliseconds(500);
    }

    public async Task ScheduleAsync(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt,
        bool scheduleSending = false)
    {
        var notificationId = this.GetPrimaryKey();

        if (this.notificationState.State.Id != default)
        {
            this.logger.Warnings()
                .LogStateAlreadyInitialized(
                    notificationId);

            return;
        }

        this.notificationState.State = new NotificationGrainState
        {
            Id = notificationId,
            From = from,
            To = to,
            Topic = topic,
            Content = content,
            CreatedAt = createdAt,
            Status = NotificationStatus.Scheduled
        };
        await notificationState.WriteStateAsync();

        if(!scheduleSending)
        {
            return;
        }

        // register reminder
        this.reminder = await this.RegisterOrUpdateReminder(
            reminderName: $"SendEmailNotificationReminder-{notificationId}",
            dueTime: reminderInterval,
            period: TimeSpan.FromMinutes(1));
    }

    public Task<bool> SentExternally() 
    {
        var isSent = this.notificationState.State.Status == NotificationStatus.Sent;
        return Task.FromResult(isSent);
    }

    public async Task MarkAsReadedAsync()
    {
        var notificationId = this.GetPrimaryKey();

        if (this.notificationState.State.Id == default)
        {
            this.logger.Errors()
                .LogStateNotInitialized(
                    notificationId);
            return;
        }

        this.notificationState.State.Status = NotificationStatus.Canceled;
        await this.notificationState.WriteStateAsync();

        // unregister reminder
        await this.DoUnregisteringAsync(true);
    }

    public async Task ReceiveReminder(
        string reminderName,
        TickStatus status)
    {
        var reminderId = this.GetPrimaryKey();

        if (this.notificationState.State.Status != NotificationStatus.Scheduled)
        {
            this.logger.Warnings()
                .LogNotificationAlreadyCanceled(
                    reminderId);

            // unregister reminder
            await this.DoUnregisteringAsync();

            return;
        }

        var userStatusGrain = this.GrainFactory.GetGrain<IUserStatusGrain>(
            this.notificationState.State.To);

        var isOnline = await userStatusGrain.GetStatusAsync();

        if (isOnline)
        {
            this.logger.Information()
                .LogUserIsNotOffline(
                    reminderId, 
                    this.notificationState.State.To);

            // unregister reminder
            await this.DoUnregisteringAsync();

            return;
        }

        await this.emailService.SendAsync(
            from: this.notificationState.State.From,
            to: this.notificationState.State.To,
            topic: this.notificationState.State.Topic,
            content: this.notificationState.State.Content,
            createdAt: this.notificationState.State.CreatedAt,
            notificationId: reminderId);

        this.notificationState.State.Status = NotificationStatus.Sent;
        await this.notificationState.WriteStateAsync();

        // unregister reminder
        await this.DoUnregisteringAsync();
    }

    private async Task DoUnregisteringAsync(bool isForced = false) 
    {
        var notificationId = this.GetPrimaryKey();

        if (this.reminder != null)
        {
            this.logger.Information()
                .LogUnregisterReminder(
                    notificationId,
                    isForced);

            await this.UnregisterReminder(reminder);
            this.reminder = null;
        }
    } 
}