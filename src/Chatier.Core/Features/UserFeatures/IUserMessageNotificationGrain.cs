using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.Extensions.Logging;
using Orleans.Utilities;

namespace Chatier.Core.Features.UserFeatures;

#region Interface
public interface IUserMessageNotificationGrain : IGrainWithStringKey
{
    Task<UserMessageNotificationModel[]> GetHistoryAsync();

    Task NotifyAsync(
        Guid notificationId,
        string chatName,
        string message,
        DateTimeOffset createdAt);

    Task SubscribeAsync(IUserNotificationObserver observer);

    Task UnsubscribeAsync(IUserNotificationObserver observer);
}
# endregion

# region Implementation
public class UserMessageNotificationGrain : Grain, IUserMessageNotificationGrain
{
    private readonly IPersistentState<UserMessageNotificationGrainState> grainState;
    private readonly ObserverManager<IUserNotificationObserver> observerManager;
    private readonly ILogger<UserMessageNotificationGrain> logger;

    public UserMessageNotificationGrain(
        [PersistentState("userNotifications", "userStore")]
        IPersistentState<UserMessageNotificationGrainState> grainState,
        ILogger<UserMessageNotificationGrain> logger)
    {
        this.grainState = grainState;
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

        this.grainState.State.Notifications.Add(model);
        await this.grainState.WriteStateAsync();

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
        return Task.FromResult(this.grainState.State.Notifications.ToArray());
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
#endregion

#region State model
[GenerateSerializer]
public class UserMessageNotificationGrainState
{
    public List<UserMessageNotificationModel> Notifications { get; set; } = new List<UserMessageNotificationModel>();
}

[GenerateSerializer]
public class UserMessageNotificationModel
{
    [Id(0)]
    public Guid NotificationId { get; init; }

    [Id(1)]
    public required string ChatName { get; init; }

    [Id(2)]
    public required string Message { get; init; }

    [Id(3)]
    public DateTimeOffset CreatedAt { get; set; }

    [Id(4)]
    public required string RecieverName { get; init; }
}
#endregion