using Chatier.Core.Features.NotificationFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.Extensions.Logging;
using Orleans.Utilities;
using System;

namespace Chatier.Core.Features.UserFeatures;

public interface IUserChatNotificationGrain : IGrainWithStringKey
{
    Task NotifyAddedToChatAsync(
        string chatName);

    Task NotifyLeftChatAsync(
        string chatName);

    Task NotifyUserAddedToChatAsync(
        string chatName,
        string userName);

    Task NotifyUserLeftChatAsync(
        string chatName,
        string userName);

    Task SubscribeAsync(IUserChatNotificationObserver observer);

    Task UnsubscribeAsync(IUserChatNotificationObserver observer);
}

public class UserChatNotificationGrain : Grain, IUserChatNotificationGrain
{
    private readonly IPersistentState<UserChatNotificationGrainState> grainState;
    private readonly ObserverManager<IUserChatNotificationObserver> observerManager;
    private readonly ILogger<UserChatNotificationGrain> logger;

    public UserChatNotificationGrain(
        [PersistentState("userChatNotifications", "userStore")]
        IPersistentState<UserChatNotificationGrainState> grainState,
        ILogger<UserChatNotificationGrain> logger)
    {
        this.grainState = grainState;
        this.logger = logger;

        this.observerManager = new ObserverManager<IUserChatNotificationObserver>(
            TimeSpan.FromMinutes(5),
            logger);
    }

    public Task NotifyAddedToChatAsync(
        string chatName)
    {
        return this.HandleNotify(
            UserGroupNotificationType.Joined,
            chatName,
            this.GetPrimaryKeyString(),
            true);
    }

    public Task NotifyLeftChatAsync(
        string chatName)
    {
        return this.HandleNotify(
            UserGroupNotificationType.Left,
            chatName,
            this.GetPrimaryKeyString(),
            true);
    }

    public Task NotifyUserAddedToChatAsync(
        string chatName, 
        string userName)
    {
        return this.HandleNotify(
            UserGroupNotificationType.Joined,
            chatName,
            userName,
            false);
    }

    public Task NotifyUserLeftChatAsync(
        string chatName, 
        string userName)
    {
        return this.HandleNotify(
            UserGroupNotificationType.Left,
            chatName,
            userName, 
            false);
    }

    private async Task HandleNotify(
        UserGroupNotificationType actionType, 
        string chatName, 
        string userName, 
        bool isThis)
    {
        var notificationId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var myName = this.GetPrimaryKeyString();

        var content = string.Format(
            "User '{0}' has been {1} from the group '{2}'",
            userName,
            actionType == UserGroupNotificationType.Joined
                ? "added"
                : "removed",
            chatName);

        this.grainState.State.Notifications.Add(new UserChatNotificationItem() 
        {
            NotificationId = notificationId,
            ActionType = actionType,
            CreatedAt = createdAt,
            GroupName = chatName,
            User = userName
        });

        await this.grainState.WriteStateAsync();

        this.observerManager.Notify(observer => 
        {
            observer.ReceiveNotification(
                notificationId: notificationId,
                chatName: chatName,
                userName: userName,
                notificationType: actionType,
                createdAt: createdAt,
                receiverName: myName);
        });

        var notificationGrain = this.GrainFactory.GetGrain<INotificationGrain>(notificationId);
        await notificationGrain.ScheduleAsync(
            from: chatName,
            to: myName,
            topic: "Group notification",
            content: content,
            createdAt: createdAt);
    }

    public Task SubscribeAsync(IUserChatNotificationObserver observer) 
    {
        this.observerManager.Subscribe(observer, observer);
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(IUserChatNotificationObserver observer) 
    {
        return Task.CompletedTask;
    }
}

#region State model
[GenerateSerializer]
public sealed class UserChatNotificationGrainState 
{
    public List<UserChatNotificationItem> Notifications { get; set; } = new List<UserChatNotificationItem>();
}

[GenerateSerializer]
public sealed class UserChatNotificationItem
{
    [Id(0)]
    public required Guid NotificationId { get; set; }

    [Id(1)]
    public DateTimeOffset CreatedAt { get; set; }

    [Id(2)]
    public required string GroupName { get; set; }

    [Id(3)]
    public required string User { get; set; }

    [Id(4)]
    public UserGroupNotificationType ActionType { get; set; }
}

public enum UserGroupNotificationType
{
    Joined,
    Left
}
#endregion
