
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Orleans;

namespace Chatier.Apps.SignalrService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IClusterClient clusterClient;
    private readonly IUserMessageNotificationObserver userMessageNotificationObserver;
    private readonly IUserChatNotificationObserver userChatNotificationObserver;
    private readonly IUserNotificationChannel userNotificationChannel;
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

        this.userNotificationChannel = userNotificationChannel;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Notification background service is starting.");
        var subscriptionTask = Task.Run(() => 
            this.CreateSubscriptionAsync(stoppingToken));

        var unSubscriptionTask = Task.Run(() =>
            this.CreateUnSubscriptionAsync(stoppingToken));

        await Task.WhenAll(subscriptionTask, unSubscriptionTask);
        this.logger.LogInformation("Notification background service is stopping.");
    }

    private async Task CreateSubscriptionAsync(CancellationToken cancellationToken) 
    {
        this.logger.LogInformation("Subscription task is starting.");

        var tasks = new[]
        {
            this.SubscribeForChatsAsync(cancellationToken),
            this.SubscribeForMessagesAsync(cancellationToken)
        };

        await Task.WhenAll(tasks);

        this.logger.LogInformation("Subscription task is stopping.");
    }

    private async Task SubscribeForChatsAsync(CancellationToken cancellationToken) 
    {
        await foreach (var item in this.userNotificationChannel
            .GetSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Subscribing to user {userName}.", item.userName);
            var userChatNotificationGrain = this.clusterClient
                .GetGrain<IUserChatNotificationGrain>(item.userName);

            var observerReference = this.clusterClient
                .CreateObjectReference<IUserChatNotificationObserver>(
                    this.userChatNotificationObserver);

            await userChatNotificationGrain.SubscribeAsync(
                observerReference);

            await Task.Delay(200, cancellationToken);
        }
    }

    private async Task SubscribeForMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in this.userNotificationChannel
            .GetSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Subscribing to user {userName}.", item.userName);
            var userMessageNotificationGrain = this.clusterClient
                .GetGrain<IUserMessageNotificationGrain>(item.userName);

            var observerReference = this.clusterClient
                .CreateObjectReference<IUserMessageNotificationObserver>(
                    this.userMessageNotificationObserver);

            await userMessageNotificationGrain.SubscribeAsync(
                observerReference);

            await Task.Delay(200, cancellationToken);
        }
    }

    private async Task CreateUnSubscriptionAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("UnSubscription task is starting.");
        
        var tasks = new[]
        {
            this.UnSubscribeForChatsAsync(cancellationToken),
            this.UnSubscribeForMessagesAsync(cancellationToken)
        };

        await Task.WhenAll(tasks);

        this.logger.LogInformation("UnSubscription task is stopping.");
    }

    private async Task UnSubscribeForChatsAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in this.userNotificationChannel
            .GetUnSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Unsubscribing from user {userName}.", item.userName);
            var userChatNotificationGrain = this.clusterClient
                .GetGrain<IUserChatNotificationGrain>(item.userName);

            var observerReference = this.clusterClient
                .CreateObjectReference<IUserChatNotificationObserver>(
                    userChatNotificationObserver);

            await userChatNotificationGrain.SubscribeAsync(
                observerReference);

            await Task.Delay(200, cancellationToken);
        }
    }

    private async Task UnSubscribeForMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var item in this.userNotificationChannel
            .GetUnSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Unsubscribing from user {userName}.", item.userName);
            var userMessageNotificationGrain = this.clusterClient
                .GetGrain<IUserMessageNotificationGrain>(item.userName);

            var observerReference = this.clusterClient
                .CreateObjectReference<IUserMessageNotificationObserver>(
                    this.userMessageNotificationObserver);

            await userMessageNotificationGrain.SubscribeAsync(
                observerReference);

            await Task.Delay(200, cancellationToken);
        }
    }
}
