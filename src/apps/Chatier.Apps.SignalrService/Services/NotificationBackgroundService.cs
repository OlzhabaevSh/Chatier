
using Chatier.Core.Features.UserFeatures;
using Orleans;

namespace Chatier.Apps.SignalrService.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IClusterClient clusterClient; 
    private readonly SignalrUserNotificationObserver signalrObserver;
    private readonly IUserSubscriptionQueue userSubscriptionQueue;
    private readonly IUserUnSubscriptionQueue userUnSubscriptionQueue;
    private readonly ILogger<NotificationBackgroundService> logger;


    public NotificationBackgroundService(
        IClusterClient clusterClient,
        SignalrUserNotificationObserver signalrObserver,
        IUserSubscriptionQueue userSubscriptionQueue,
        IUserUnSubscriptionQueue userUnSubscriptionQueue,
        ILogger<NotificationBackgroundService> logger)
    {
        this.clusterClient = clusterClient;
        this.signalrObserver = signalrObserver;
        this.userSubscriptionQueue = userSubscriptionQueue;
        this.userUnSubscriptionQueue = userUnSubscriptionQueue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var objectReference = this.clusterClient
            .CreateObjectReference<IUserNotificationObserver>(
                this.signalrObserver);

        var subscriptionTask = Task.Run(() => 
            this.CreateSubsciptionTask(stoppingToken));

        var unsubscriptionTask = Task.Run(() =>
            this.CreateUnsubscriptionTask(stoppingToken));

        await Task.WhenAll(subscriptionTask, unsubscriptionTask);
    }

    private async Task CreateSubsciptionTask(CancellationToken cancellationToken) 
    {
        while (!cancellationToken.IsCancellationRequested) 
        {
            // Process the queue for new user subscriptions
            while (userSubscriptionQueue.TryDequeue(out var userName))
            {
                var userNotificationGrain = clusterClient
                    .GetGrain<IUserNotificationGrain>(userName);

                await userNotificationGrain.SubscribeAsync(
                    this.signalrObserver);
            }
            // Add a delay to avoid tight loop
            await Task.Delay(1000, cancellationToken);
        }
    }

    private async Task CreateUnsubscriptionTask(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Process the queue for new user subscriptions
            while (userUnSubscriptionQueue.TryDequeue(out var userName))
            {
                var userNotificationGrain = clusterClient
                    .GetGrain<IUserNotificationGrain>(userName);

                await userNotificationGrain.UnsubscribeAsync(
                    this.signalrObserver);
            }

            // Add a delay to avoid tight loop
            await Task.Delay(1000, cancellationToken);
        }
    }
}
