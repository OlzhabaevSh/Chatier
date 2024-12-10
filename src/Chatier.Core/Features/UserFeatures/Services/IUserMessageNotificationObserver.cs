namespace Chatier.Core.Features.UserFeatures.Services;

public interface IUserMessageNotificationObserver : IGrainObserver
{
    Task ReceiveNotification(
        Guid notificationId,
        string chatName,
        string senderName,
        string message,
        DateTimeOffset createdAt,
        string receiverName);
}