using Chatier.Core.Features.UserFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures;

public class NotificationGrain :
    Grain,
    INotificationGrain,
    IRemindable
{
    private readonly IPersistentState<NotificationState> notificationState;

    private readonly ILogger<NotificationGrain> logger;

    private readonly TimeSpan reminderInterval;

    private IGrainReminder? reminder;

    public NotificationGrain(
        [PersistentState("notifications", "notificationStore")]
        IPersistentState<NotificationState> notificationState,
        ILogger<NotificationGrain> logger,
        IConfiguration configuration)
    {
        this.notificationState = notificationState;
        this.logger = logger;

        var reminderIntervalInMilliseconds = configuration.GetValue<int>("ReminderIntervalInMilliseconds");
        this.reminderInterval = reminderIntervalInMilliseconds > 0
            ? TimeSpan.FromMilliseconds(reminderIntervalInMilliseconds)
            : TimeSpan.FromMilliseconds(500);
    }

    public Task<Guid> GetNotificationIdAsync()
    {
        var id = this.GetPrimaryKey();
        return Task.FromResult(id);
    }

    public async Task ScheduleAsync(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt)
    {
        var notificationId = this.GetPrimaryKey();

        if (this.notificationState.State.Id != default)
        {
            this.logger.LogWarningNotificationStateAlreadyInitialized(
                notificationId);

            return;
        }

        this.notificationState.State = new NotificationState
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

        // register reminder
        this.reminder = await this.RegisterOrUpdateReminder(
            reminderName: $"SendEmailNotificationReminder-{notificationId}",
            dueTime: reminderInterval,
            period: TimeSpan.FromMinutes(1));
    }

    public async Task ReadAsync()
    {
        var notificationId = this.GetPrimaryKey();

        if (this.notificationState.State.Id == default)
        {
            this.logger.LogErrorNotificationStateNotInitialized(notificationId);
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
            this.logger.LogWarningNotificationStateAlreadyCanceled(reminderId);

            return;
        }

        SendEmail();

        this.notificationState.State.Status = NotificationStatus.Sent;
        await this.notificationState.WriteStateAsync();

        var user = GrainFactory.GetGrain<IUserGrain>(this.notificationState.State.To);
        await user.SetNotificationAsync(this.notificationState.State.Id);

        // unregister reminder
        await this.DoUnregisteringAsync();
    }

    private async Task DoUnregisteringAsync(bool isForced = false) 
    {
        var notificationId = this.GetPrimaryKey();

        if (this.reminder != null)
        {
            this.logger.LogInformationUnregisterReminder(
                notificationId,
                isForced);

            await this.UnregisterReminder(reminder);
            this.reminder = null;
        }
    } 

    private void SendEmail() 
    {
        var reminderId = this.GetPrimaryKey();

        this.logger.LogInformationSendEmail(
            this.notificationState.State.Topic,
            this.notificationState.State.From,
            this.notificationState.State.To,
            this.notificationState.State.CreatedAt,
            reminderId,
            this.notificationState.State.Content);
    }
}