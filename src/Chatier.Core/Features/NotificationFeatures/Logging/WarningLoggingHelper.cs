using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures.Logging;

public static class WarningLoggingHelper
{
    private static EventId NotificationStateAlreadyInitializedEventId =>
        new EventId(003_001, "Notification state is already initialized");

    public static void LogStateAlreadyInitialized(
        this WarningLogger instance,
        Guid notificationId)
    {
        instance.Logger.LogWarning(
            NotificationStateAlreadyInitializedEventId,
            "Notification state is already initialized ({notificationId})",
            notificationId);
    }

    private static EventId NotificationStateAlreadyCanceledEventId =>
        new EventId(003_002, "Notification state is already canceled");

    public static void LogNotificationAlreadyCanceled(
        this WarningLogger instance,
        Guid notificationId)
    {
        instance.Logger.LogWarning(
            NotificationStateAlreadyCanceledEventId,
            "ReceiveReminder for '{notificationId}' was soft canceled",
            notificationId);
    }
}
