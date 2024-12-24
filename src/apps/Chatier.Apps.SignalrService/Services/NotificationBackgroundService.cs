
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Orleans;
using System.Collections.Concurrent;

namespace Chatier.Apps.SignalrService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IClusterClient clusterClient;
    private readonly IUserMessageNotificationObserver userMessageNotificationObserver;
    private readonly IUserChatNotificationObserver userChatNotificationObserver;
    private readonly IUserNotificationChannel userNotificationChannel;
    private readonly ConcurrentDictionary<string, IUserChatNotificationObserver> userChatObserverDictionary;
    private readonly ConcurrentDictionary<string, IUserMessageNotificationObserver> userMessageObserverDictionary;
    private readonly ILogger<NotificationBackgroundService> logger;


    public NotificationBackgroundService(
        IClusterClient clusterClient,
        IUserMessageNotificationObserver userMessageNotificationObserver,
        IUserChatNotificationObserver userChatNotificationObserver,
        IUserNotificationChannel userNotificationChannel,
        ILogger<NotificationBackgroundService> logger)
    {
        this.clusterClient = clusterClient;
        this.userMessageNotificationObserver = userMessageNotificationObserver;
        this.userChatNotificationObserver = userChatNotificationObserver;
        this.userChatObserverDictionary = new ConcurrentDictionary<string, IUserChatNotificationObserver>();
        this.userMessageObserverDictionary = new ConcurrentDictionary<string, IUserMessageNotificationObserver>();
        this.userNotificationChannel = userNotificationChannel;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Notification background service is starting.");
        
        var subscriptionTask = this.CreateSubscriptionAsync(stoppingToken);
        var unSubscriptionTask = this.CreateUnSubscriptionAsync(stoppingToken);

        await Task.WhenAll(
            subscriptionTask, 
            unSubscriptionTask);
        
        this.logger.LogInformation("Notification background service is stopping.");
    }

    private async Task CreateSubscriptionAsync(
        CancellationToken cancellationToken) 
    {
        this.logger.LogInformation("Subscription task is starting.");

        await foreach (var item in this.userNotificationChannel
            .GetSubscriptionEnumerableAsync(cancellationToken)) 
        {
            this.logger.LogTrace("Subscribing to user {userName}.", item.userName);

            await this.SetUserStatusAsync(item.userName, true);
            await this.SubscribeForChatsAsync(item);
            await this.SubscribeForMessagesAsync(item);

            await Task.Delay(200, cancellationToken);
        }

        this.logger.LogInformation("Subscription task is stopping.");
    }

    private async Task SubscribeForChatsAsync(
        UserSubscriberModel item) 
    {
        var userChatNotificationGrain = this.clusterClient
                .GetGrain<IUserChatNotificationGrain>(item.userName);

        if(!this.userChatObserverDictionary.TryGetValue(
            item.userName, 
            out var observerReference))
        {
            observerReference = this.clusterClient
                .CreateObjectReference<IUserChatNotificationObserver>(
                    this.userChatNotificationObserver);

            this.userChatObserverDictionary.TryAdd(
                item.userName, 
                observerReference);
        }

        await userChatNotificationGrain.SubscribeAsync(
            observerReference);
    }

    private async Task SubscribeForMessagesAsync(
        UserSubscriberModel item)
    {
        var userMessageNotificationGrain = this.clusterClient
            .GetGrain<IUserMessageNotificationGrain>(item.userName);

        if (!this.userMessageObserverDictionary.TryGetValue(
            item.userName, 
            out var observerReference)) 
        {
            observerReference = this.clusterClient
                .CreateObjectReference<IUserMessageNotificationObserver>(
                    this.userMessageNotificationObserver);

            this.userMessageObserverDictionary.TryAdd(
                item.userName,
                observerReference);
        }

        await userMessageNotificationGrain.SubscribeAsync(
            observerReference);
    }

    private async Task CreateUnSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation("UnSubscription task is starting.");

        await foreach (var item in this.userNotificationChannel
            .GetUnSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Unsubscribing from user {userName}.", item.userName);

            await this.SetUserStatusAsync(item.userName, false);
            await this.UnSubscribeForChatsAsync(item);
            await this.UnSubscribeForMessagesAsync(item);

            await Task.Delay(200, cancellationToken);
        }

        this.logger.LogInformation("UnSubscription task is stopping.");
    }

    private async Task UnSubscribeForChatsAsync(
        UserUnSubscriberModel item)
    {
        var userChatNotificationGrain = this.clusterClient
                .GetGrain<IUserChatNotificationGrain>(item.userName);

        if(this.userChatObserverDictionary.TryGetValue(
            item.userName, 
            out var observerReference))
        {
            await userChatNotificationGrain.UnsubscribeAsync(
                observerReference);

            this.userChatObserverDictionary.TryRemove(
                item.userName, 
                out _);
        }
    }

    private async Task UnSubscribeForMessagesAsync(
        UserUnSubscriberModel item)
    {
        var userMessageNotificationGrain = this.clusterClient
                .GetGrain<IUserMessageNotificationGrain>(item.userName);

        if(this.userMessageObserverDictionary.TryGetValue(
            item.userName, 
            out var observerReference))
        {
            await userMessageNotificationGrain.UnsubscribeAsync(
                observerReference);

            this.userMessageObserverDictionary.TryRemove(
                item.userName, 
                out _);
        }
    }

    private async Task SetUserStatusAsync(string userName,
        bool isOnline)
    {
        var userStatusGrain = this.clusterClient
            .GetGrain<IUserStatusGrain>(userName);
        await userStatusGrain.SetStatusAsync(isOnline);
    }
}
