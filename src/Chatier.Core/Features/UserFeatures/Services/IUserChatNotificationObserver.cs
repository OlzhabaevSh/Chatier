namespace Chatier.Core.Features.UserFeatures.Services;

public interface IUserChatNotificationObserver : IGrainObserver
{
    Task ReceiveNotification(
        Guid notificationId,
        string chatName,
        string userName,
        UserGroupNotificationType notificationType,
        DateTimeOffset createdAt,
        string receiverName);
}
