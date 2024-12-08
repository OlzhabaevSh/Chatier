using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.Extensions.Logging;
using Orleans.Utilities;

namespace Chatier.Core.Features.UserFeatures;

public class UserNotificationGrain : Grain, IUserNotificationGrain
{
    private readonly IPersistentState<UserNotificationGrainState> userNotificationStates;
    private readonly ObserverManager<IUserNotificationObserver> observerManager;
    private readonly ILogger<UserNotificationGrain> logger;

    public UserNotificationGrain(
        [PersistentState("userNotifications", "userStore")]
        IPersistentState<UserNotificationGrainState> userNotificationStates,
        ILogger<UserNotificationGrain> logger)
    {
        this.userNotificationStates = userNotificationStates;
        this.logger = logger;
        this.observerManager = new ObserverManager<IUserNotificationObserver>(
            TimeSpan.FromMinutes(5), 
            logger);
    }

    public async Task NotifyAsync(
        Guid notificationId, 
        string chatName, 
        string message, 
        DateTimeOffset createdAt)
    {
        var model = new UserMessageNotificationModel()
        {
            ChatName = chatName,
            Message = message,
            CreatedAt = createdAt,
            NotificationId = notificationId,
            RecieverName = this.GetPrimaryKeyString()
        };

        this.userNotificationStates.State.Notifications.Add(model);
        await this.userNotificationStates.WriteStateAsync();

        this.observerManager.Notify(observer => 
        {
            observer.ReceiveNotification(
                notificationId: notificationId,
                chatName: chatName,
                message: message,
                createdAt: createdAt,
                recieverName: this.GetPrimaryKeyString());
        });
    }

    public Task<UserMessageNotificationModel[]> GetHistoryAsync()
    {
        return Task.FromResult(this.userNotificationStates.State.Notifications.ToArray());
    }

    public Task SubscribeAsync(IUserNotificationObserver observer) 
    {
        this.observerManager.Subscribe(observer, observer); 
        return Task.CompletedTask; 
    }

    public Task UnsubscribeAsync(IUserNotificationObserver observer) 
    {
        this.observerManager.Unsubscribe(observer); 
        return Task.CompletedTask; 
    }
}