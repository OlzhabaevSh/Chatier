using Chatier.Core.Features.NotificationFeatures;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.UserFeatures;

public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserNotificationState> notificationState;
    private readonly IPersistentState<UserSentNotificationState> sentNotificationsState;

    private readonly ILogger<UserGrain> logger;

    public UserGrain(
        [PersistentState("notifications", "userStore")]
        IPersistentState<UserNotificationState> notificationState,
        [PersistentState("sentNotifications", "userStore")]
        IPersistentState<UserSentNotificationState> sentNotificationsState,
        ILogger<UserGrain> logger)
    {
        this.notificationState = notificationState;
        this.sentNotificationsState = sentNotificationsState;
        this.logger = logger;
    }

    public Task<string> GetNameAsync()
    {
        var name = this.GetPrimaryKeyString();
        return Task.FromResult(name);
    }

    public Task NotifyAboutAddingToGroupAsync(
        string groupName,
        string userName)
    {
        return this.HandleNotifyGroupActionAsync(
            groupName, 
            userName, 
            UserGroupNotificationType.Joined);
    }

    public Task NotifyAboutLeavingAGroupAsync(
        string groupName,
        string userName)
    {
        return this.HandleNotifyGroupActionAsync(
            groupName,
            userName,
            UserGroupNotificationType.Left);
    }

    private async Task HandleNotifyGroupActionAsync(
        string groupName, 
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
                GroupName = groupName,
                ActionType = UserGroupNotificationType.Joined,
                User = userName
            });

        await this.notificationState.WriteStateAsync();

        var content = string.Format(
            "You have been {0} from the group '{1}'",
            actionType == UserGroupNotificationType.Joined 
                ? "added" 
                : "removed",
            groupName);

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(notificationId);
        await notificationGrain.ScheduleAsync(
            from: groupName,
            to: userName,
            topic: "Group notification",
            content: content,
            createdAt: createdAt);
    }

    public async Task NotifyAboutMessageAsync(
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

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(notificationId);
        await notificationGrain.ScheduleAsync(
            from: $"{groupName} ({sender})",
            to: myName,
            topic: "New message",
            content: message,
            createdAt: createdAt);
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
        await notificationGrain.ConfirmAsync();
    }

    public Task<Dictionary<Guid, BaseUserNotificationItems>> GetAllNotifications()
    {
        var notifications = this.notificationState.State.Notifications;

        return Task.FromResult(notifications);
    }

    public Task<Dictionary<Guid, UserGotMessageNotificationItem>> GetAllMessageNotifications()
    {
        var notifications = this.notificationState.State.Notifications
            .Where(x => x.Value is UserGotMessageNotificationItem)
            .ToDictionary(x => x.Key, x => (UserGotMessageNotificationItem)x.Value);

        return Task.FromResult(notifications);
    }

    public Task<(Guid? id, UserGotMessageNotificationItem?)> GetLatestMessageNotification()
    {
        var notification = this.notificationState.State.Notifications
            .Where(x => x.Value is UserGotMessageNotificationItem)
            .LastOrDefault();

        (Guid? id, UserGotMessageNotificationItem?) result = notification.Key != Guid.Empty
            ? (notification.Key, (UserGotMessageNotificationItem)notification.Value)
            : (null, null);

        return Task.FromResult(result);
    }

    public Task<Dictionary<Guid, UserGroupNotificationItem>> GetAllGroupNotifications()
    {
        var notifications = this.notificationState.State.Notifications
            .Where(x => x.Value is UserGroupNotificationItem)
            .ToDictionary(x => x.Key, x => (UserGroupNotificationItem)x.Value);

        return Task.FromResult(notifications);
    }

    public Task<(Guid? id, UserGroupNotificationItem?)> GetLatestGroupNotification()
    {
        var notification = this.notificationState.State.Notifications
            .Where(x => x.Value is UserGroupNotificationItem)
            .LastOrDefault();

        (Guid? id, UserGroupNotificationItem?) result = notification.Key != Guid.Empty
            ? (notification.Key, (UserGroupNotificationItem)notification.Value)
            : (null, null);

        return Task.FromResult(result);
    }

    public Task<Guid[]> GetReceivedNotificationsAsync() 
    {
        var notificationIds = this.notificationState.State.Notifications.Keys.ToArray();
        return Task.FromResult(notificationIds);
    }

    public async Task SetNotificationAsync(
        Guid notificationId)
    {
        this.sentNotificationsState.State.NotificationIds.Add(notificationId);
        await this.sentNotificationsState.WriteStateAsync();
    }
}