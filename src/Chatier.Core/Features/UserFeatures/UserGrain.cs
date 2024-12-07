using Chatier.Core.Features.NotificationFeatures;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.UserFeatures;

public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserNotificationState> notificationState;
    
    private readonly ILogger<UserGrain> logger;

    public UserGrain(
        [PersistentState("notifications", "userStore")]
        IPersistentState<UserNotificationState> notificationState,
        ILogger<UserGrain> logger)
    {
        this.notificationState = notificationState;
        this.logger = logger;
    }

    public Task NotifyAboutAddingToChatAsync(
        string chat,
        string userName)
    {
        return this.HandleNotifyGroupActionAsync(
            chat, 
            userName, 
            UserGroupNotificationType.Joined);
    }

    public Task NotifyAboutLeavingAChatAsync(
        string chat,
        string userName)
    {
        return this.HandleNotifyGroupActionAsync(
            chat,
            userName,
            UserGroupNotificationType.Left);
    }

    private async Task HandleNotifyGroupActionAsync(
        string chat, 
        string userName, 
        UserGroupNotificationType actionType) 
    {
        var notificationId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        this.notificationState.State.Notifications.Add(
            key: notificationId,
            value: new UserGroupNotificationItem()
            {
                Id = notificationId,
                CreatedAt = createdAt,
                GroupName = chat,
                ActionType = UserGroupNotificationType.Joined,
                User = userName
            });

        await this.notificationState.WriteStateAsync();

        var content = string.Format(
            "User '{0}' has been {1} from the group '{2}'",
            userName,
            actionType == UserGroupNotificationType.Joined 
                ? "added" 
                : "removed",
            chat);

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(notificationId);
        await notificationGrain.ScheduleAsync(
            from: chat,
            to: userName,
            topic: "Group notification",
            content: content,
            createdAt: createdAt);
    }

    public async Task NotifyAboutNewMessageAsync(
        string groupName,
        string sender,
        Guid messageId,
        string message)
    {
        var myName = this.GetPrimaryKeyString();
        if (sender == myName)
        {
            return;
        }

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
                Message = message
            });

        await this.notificationState.WriteStateAsync();

        var user = GrainFactory.GetGrain<IUserNotificationGrain>(myName);
        await user.NotifyAsync(
            notificationId: notificationId,
            chatName: groupName,
            message: message,
            createdAt: createdAt);

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(
            notificationId);

        await notificationGrain.ScheduleAsync(
            from: $"{groupName} ({sender})",
            to: myName,
            topic: "New message",
            content: message,
            createdAt: createdAt,
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
        await notificationGrain.ReadAsync();
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