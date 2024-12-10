using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.UserFeatures.Logging;

public static class LoggingHelper
{
    private static EventId NotificationIdNotFoundId =>
        new EventId(1, "Notification with ID does not exist.");

    public static void LogWarningNotificationIsNotExist(
        this ILogger<UserGrain> logger,
        Guid notificationId,
        string userName)
    {
        logger.LogWarning(
            NotificationIdNotFoundId,
            "A user {userName} doesn't have a notification with id '{notificationId}'",
            userName,
            notificationId);
    }
}
