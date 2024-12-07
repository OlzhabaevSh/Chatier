namespace Chatier.Core.Features.UserFeatures;

public interface IUserNotificationObserver : IGrainObserver
{
    Task ReceiveNotification(
        Guid notificationId,
        string chatName,
        string message,
        DateTimeOffset createdAt,
        string recieverName);
}