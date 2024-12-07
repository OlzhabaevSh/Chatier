using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures.Logging;

public static class InformationLoggingHelper
{
    private static EventId NotificationReminderUnregisteredEventId =>
        new EventId(002_001, "Notification reminder unregistered");

    public static void LogUnregisterReminder(
        this InformationLogger instance,
        Guid notificationId,
        bool isForced)
    {
        instance.Logger.LogInformation(
            NotificationReminderUnregisteredEventId,
            "Unregistering reminder for '{notificationId}'. Is forced: {isForced}",
            notificationId,
            isForced);
    }

    private static EventId NotificationUserIsNotOffline =>
        new EventId(002_002, "Notification canceled becauseUser is not offline");

    public static void LogUserIsNotOffline(
        this InformationLogger instance,
        Guid notificationId,
        string userName)
    {
        instance.Logger.LogInformation(
            NotificationUserIsNotOffline,
            "Notification '{notificationId}' canceled because user '{userName}' is not offline",
            notificationId,
            userName);
    }
}