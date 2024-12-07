namespace Chatier.Core.Features.NotificationFeatures;

public interface INotificationGrain : IGrainWithGuidKey
{
    Task ScheduleAsync(
        string from,
        string to,
        string topic,
        string content,
        DateTimeOffset createdAt,
        bool scheduleSending = false);

    Task ReadAsync();

    Task<bool> SentExternally();
}