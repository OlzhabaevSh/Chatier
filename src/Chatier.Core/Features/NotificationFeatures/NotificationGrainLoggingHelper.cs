using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures;

public static class NotificationGrainLoggingHelper
{
    private static EventId NotificationStateAlreadyInitializedEventId => 
        new EventId(1, "Notification state is already initialized");

    public static void LogWarningNotificationStateAlreadyInitialized(
        this ILogger<NotificationGrain> logger,
        Guid notificationId)
    {
        logger.LogWarning(
            NotificationStateAlreadyInitializedEventId,
            "Notification state is already initialized ({notificationId})",
            notificationId);
    }

    private static EventId NotificationStateNotInitializedEventId =>
        new EventId(2, "Notification state is not initialized");

    public static void LogErrorNotificationStateNotInitialized(
        this ILogger<NotificationGrain> logger,
        Guid notificationId)
    {
        logger.LogError(
            NotificationStateNotInitializedEventId,
            "Notification state is not initialized ({notificationId})",
            notificationId);
    }

    private static EventId NotificationStateAlreadyCanceledEventId =>
        new EventId(3, "Notification state is already canceled");

    public static void LogWarningNotificationStateAlreadyCanceled(
        this ILogger<NotificationGrain> logger,
        Guid notificationId)
    {
        logger.LogWarning(
            NotificationStateAlreadyCanceledEventId,
            "ReceiveReminder for '{notificationId}' was soft canceled",
            notificationId);
    }

    private static EventId NotificationReminderUnregisteredEventId =>
        new EventId(4, "Notification reminder unregistered");

    public static void LogInformationUnregisterReminder(
        this ILogger<NotificationGrain> logger,
        Guid notificationId,
        bool isForced)
    {
        logger.LogInformation(
            NotificationReminderUnregisteredEventId,
            "Unregistering reminder for '{notificationId}'. Is forced: {isForced}",
            notificationId,
            isForced);
    }

    private static EventId NotificationSendEmailEventId =>
        new EventId(5, "Notification email sent");

    public static void LogInformationSendEmail(
        this ILogger<NotificationGrain> logger,
        string topic,
        string from,
        string to,
        DateTimeOffset createdAt,
        Guid notificationId,
        string content)
    {
        logger.LogInformation(
            NotificationSendEmailEventId,
            """
            _______________________________________________________
            NOTIFICATION!
            | {topic}
            | From: '{from}' -> To: '{to}' | ({createdAt})
            | ID: {notificationId}
            |
            | Content: 
            | {content}
            _______________________________________________________
            """,
            topic,
            from,
            to,
            createdAt,
            notificationId,
            content);
    }
}
