
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Orleans;

namespace Chatier.Apps.SignalrService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IClusterClient clusterClient;
    //private readonly IUserMessageNotificationObserver signalrObserver;
    private readonly IUserChatNotificationObserver userChatNotificationObserver;
    private readonly IUserNotificationChannel userNotificationChannel;
    private readonly ILogger<NotificationBackgroundService> logger;


    public NotificationBackgroundService(
        IClusterClient clusterClient,
        //IUserMessageNotificationObserver signalrObserver,
        IUserChatNotificationObserver userChatNotificationObserver,
        IUserNotificationChannel userNotificationChannel,
        ILogger<NotificationBackgroundService> logger)
    {
        this.clusterClient = clusterClient;
        this.userChatNotificationObserver = userChatNotificationObserver;

        this.userNotificationChannel = userNotificationChannel;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Notification background service is starting.");
        var subscriptionTask = Task.Run(() => 
            this.CreateSubsciptionTask(stoppingToken));

        var unSubscriptionTask = Task.Run(() =>
            this.CreateUnsubscriptionTask(stoppingToken));

        await Task.WhenAll(subscriptionTask, unSubscriptionTask);
        this.logger.LogInformation("Notification background service is stopping.");
    }

    private async Task CreateSubsciptionTask(CancellationToken cancellationToken) 
    {
        this.logger.LogInformation("Subscription task is starting.");
        await foreach(var item in this.userNotificationChannel
            .GetSubscriptionEnumerableAsync(cancellationToken))
        {
            this.logger.LogTrace("Subscribing to user {userName}.", item.userName);
            var userChatNotificationGrain = this.clusterClient
                .GetGrain<IUserChatNotificationGrain>(item.userName);

            var observerReference = this.clusterClient
                .CreateObjectReference<IUserChatNotificationObserver>(
                    userChatNotificationObserver);

            await userChatNotificationGrain.SubscribeAsync(
                observerReference);

            await Task.Delay(200, cancellationToken);
        }
        this.logger.LogInformation("Subscription task is stopping.");
    }

    private async Task CreateUnsubscriptionTask(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("UnSubscription task is starting.");
        await foreach(var item in this.userNotificationChannel
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
        this.logger.LogInformation("UnSubscription task is stopping.");
    }
}
