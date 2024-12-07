using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures.Logging;

public static class ErrorLoggingHelper
{
    private static EventId NotificationStateNotInitializedEventId =>
        new EventId(004_001, "Notification state is not initialized");

    public static void LogStateNotInitialized(
        this ErrorLogger instance,
        Guid notificationId)
    {
        instance.Logger.LogError(
            NotificationStateNotInitializedEventId,
            "Notification state is not initialized ({notificationId})",
            notificationId);
    }
}
