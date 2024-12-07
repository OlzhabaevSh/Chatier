using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.NotificationFeatures.Services;

public interface IEmailService
{
    Task SendAsync(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt,
        Guid notificationId);
}

public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        this.logger = logger;
    }

    public Task SendAsync(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt,
        Guid notificationId)
    {
        FakeSendingEmail(
            from,
            to,
            topic,
            content,
            createdAt,
            notificationId);

        return Task.CompletedTask;
    }

    private static EventId NotificationSendEmailEventId =>
        new EventId(1, "Notification email sent");

    private void FakeSendingEmail(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt,
        Guid notificationId)
    {
        logger.LogInformation(
            NotificationSendEmailEventId,
            """
            _______________________________________________________
            | New Email!
            |------------------------------------------------------
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
