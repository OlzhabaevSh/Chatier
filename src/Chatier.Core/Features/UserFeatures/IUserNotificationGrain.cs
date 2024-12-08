using Chatier.Core.Features.UserFeatures.Services;

namespace Chatier.Core.Features.UserFeatures;

public interface IUserNotificationGrain : IGrainWithStringKey
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