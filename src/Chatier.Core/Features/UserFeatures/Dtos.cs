namespace Chatier.Core.Features.UserFeatures;

[GenerateSerializer]
public class UserNotificationState
{
    [Id(0)]
    public required Dictionary<Guid, BaseUserNotificationItems> Notifications { get; set; } = new Dictionary<Guid, BaseUserNotificationItems>();
}

[GenerateSerializer]
public abstract class BaseUserNotificationItems
{
    [Id(0)]
    public required Guid Id { get; set; }

    [Id(1)]
    public DateTimeOffset CreatedAt { get; set; }
}

[GenerateSerializer]
public sealed class UserGotMessageNotificationItem : BaseUserNotificationItems
{
    [Id(0)]
    public required string GroupName { get; set; }

    [Id(1)]
    public required string Sender { get; set; }

    [Id(2)]
    public required string Message { get; set; }
}

[GenerateSerializer]
public sealed class UserGroupNotificationItem : BaseUserNotificationItems
{
    [Id(0)]
    public required string GroupName { get; set; }

    [Id(1)]
    public required string User { get; set; }

    [Id(2)]
    public UserGroupNotificationType ActionType { get; set; }
}

public enum UserGroupNotificationType
{
    Joined,
    Left
}

[GenerateSerializer]
public sealed class UserSentNotificationState
{
    public HashSet<Guid> NotificationIds { get; set; } = new HashSet<Guid>();
}

[GenerateSerializer]
public class UserNotificationGrainState
{
    public List<UserMessageNotificationModel> Notifications { get; set; } = new List<UserMessageNotificationModel>();
}

[GenerateSerializer]
public class UserMessageNotificationModel
{
    [Id(0)]
    public Guid NotificationId { get; init; }

    [Id(1)]
    public required string ChatName { get; init; }

    [Id(2)]
    public required string Message { get; init; }

    [Id(3)]
    public DateTimeOffset CreatedAt { get; set; }

    [Id(4)]
    public required string RecieverName { get; init; }
}

[GenerateSerializer]
public class UserStatusGrainState
{
    public bool Online { get; set; }
    public DateTimeOffset LastVisitedAt { get; set; }
}