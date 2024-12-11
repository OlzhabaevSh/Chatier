
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Orleans;

namespace Chatier.Apps.SignalrService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IClusterClient clusterClient; 
    private readonly SignalrUserMessageNotificationObserver signalrObserver;
    private readonly IUserNotificationChannel userNotificationChannel;
    private readonly ILogger<NotificationBackgroundService> logger;


    public NotificationBackgroundService(
        IClusterClient clusterClient,
        SignalrUserMessageNotificationObserver signalrObserver,
        IUserNotificationChannel userNotificationChannel,
        ILogger<NotificationBackgroundService> logger)
    {
        this.clusterClient = clusterClient;
        this.signalrObserver = signalrObserver;
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
            var userNotificationGrain = clusterClient
                .GetGrain<IUserMessageNotificationGrain>(item.userName);
            await userNotificationGrain.SubscribeAsync(
                this.signalrObserver);

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
            var userNotificationGrain = clusterClient
                .GetGrain<IUserMessageNotificationGrain>(item.userName);
            await userNotificationGrain.UnsubscribeAsync(
                this.signalrObserver);

            await Task.Delay(200, cancellationToken);
        }
        this.logger.LogInformation("UnSubscription task is stopping.");
    }
}
