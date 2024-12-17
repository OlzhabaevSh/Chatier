using Chatier.Core.Features.NotificationFeatures;
using Chatier.Core.Features.UserFeatures.Logging;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.UserFeatures;

#region Interfaces
public interface IUserGrain : IGrainWithStringKey
{
    Task NotifyAboutNewMessageAsync(
        string chat,
        string sender,
        Guid messageId,
        string message);

    Task ConfirmNotificationAsync(
        Guid notificationId);

    Task<Guid> GetLatestMessageNotificationIdAsync();
}
#endregion

#region Implementation
public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserNotificationState> notificationState;
    private readonly ILogger<UserGrain> logger;

    public UserGrain(
        [PersistentState("userNotifications", "userStore")]
        IPersistentState<UserNotificationState> notificationState,
        ILogger<UserGrain> logger)
    {
        this.notificationState = notificationState;
        this.logger = logger;
    }

    public async Task NotifyAboutNewMessageAsync(
        string groupName,
        string sender,
        Guid messageId,
        string message)
    {
        var myName = this.GetPrimaryKeyString();
        
        var notificationId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        this.notificationState.State.Notifications.Add(
            key: notificationId,
            value: new UserGotMessageNotificationItem()
            {
                Id = notificationId,
                CreatedAt = createdAt,
                GroupName = groupName,
                Sender = sender,
                Message = message,
                MessageId = messageId
            });

        await this.notificationState.WriteStateAsync();

        var user = GrainFactory.GetGrain<IUserMessageNotificationGrain>(myName);
        await user.NotifyAsync(
            notificationId: notificationId,
            senderName: sender,
            chatName: groupName,
            message: message,
            createdAt: createdAt,
            messageId: messageId);

        if (sender == myName)
        {
            return;
        }

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(
            notificationId);

        await notificationGrain.ScheduleAsync(
            from: $"{groupName} ({sender})",
            to: myName,
            topic: "New message",
            content: message,
            createdAt: createdAt,
            messageId: messageId,
            scheduleSending: true);
    }

    public async Task ConfirmNotificationAsync(
        Guid notificationId)
    {
        if (!this.notificationState.State.Notifications.ContainsKey(notificationId))
        {
            var userName = this.GetPrimaryKeyString();

            this.logger.LogWarningNotificationIsNotExist(
                notificationId,
                userName);

            return;
        }

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(notificationId);
        await notificationGrain.MarkAsReadedAsync();
    }

    public Task<Guid> GetLatestMessageNotificationIdAsync()
    {
        var id = this.notificationState.State.Notifications
            .Where(x => x.Value is UserGotMessageNotificationItem)
            .OrderBy(x => x.Value.CreatedAt)
            .Select(x => x.Key)
            .LastOrDefault();

        return Task.FromResult(id);
    }
}
#endregion

#region State model
[GenerateSerializer]
public class UserNotificationState
{
    [Id(0)]
    public required Dictionary<Guid, BaseUserNotificationItems> Notifications { get; set; } = new Dictionary<Guid, BaseUserNotificationItems>();
}

[GenerateSerializer]
public abstract class BaseUserNotificationItems
{
    [Id(0)]
    public required Guid Id { get; set; }

    [Id(1)]
    public DateTimeOffset CreatedAt { get; set; }
}

[GenerateSerializer]
public sealed class UserGotMessageNotificationItem : BaseUserNotificationItems
{
    [Id(0)]
    public required string GroupName { get; set; }

    [Id(1)]
    public required string Sender { get; set; }

    [Id(2)]
    public required string Message { get; set; }

    [Id(3)]
    public Guid MessageId { get; set; }
}
#endregion