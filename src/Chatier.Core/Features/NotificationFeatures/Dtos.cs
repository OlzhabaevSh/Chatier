namespace Chatier.Core.Features.NotificationFeatures;

[GenerateSerializer]
public class NotificationGrainState
{
    [Id(0)]
    public required Guid Id { get; set; }

    [Id(1)]
    public required string From { get; set; }

    [Id(2)]
    public required string To { get; set; }

    [Id(3)]
    public required string Topic { get; set; }

    [Id(4)]
    public required string Content { get; set; }

    [Id(5)]
    public required DateTimeOffset CreatedAt { get; set; }

    [Id(6)]
    public NotificationStatus Status { get; set; }
}

public enum NotificationStatus
{
    Canceled = -1,
    Scheduled = 0,
    Sent = 1
}