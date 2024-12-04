namespace Chatier.Core.Features.UserFeatures;

public interface IUserGrain : IGrainWithStringKey
{
    Task<string> GetNameAsync();

    Task NotifyAboutAddingToGroupAsync(
        string groupName,
        string userName);

    Task NotifyAboutLeavingAGroupAsync(
        string groupName,
        string userName);

    Task NotifyAboutMessageAsync(
        string groupName,
        string sender,
        Guid messageId,
        string message);

    Task ConfirmNotificationAsync(
        Guid notificationId);

    Task SetNotificationAsync(
        Guid notificationId);

    Task<Guid[]> GetReceivedNotificationsAsync();

    Task<Dictionary<Guid, BaseUserNotificationItems>> GetAllNotifications();

    Task<Dictionary<Guid, UserGotMessageNotificationItem>> GetAllMessageNotifications();

    Task<(Guid? id, UserGotMessageNotificationItem?)> GetLatestMessageNotification();

    Task<Dictionary<Guid, UserGroupNotificationItem>> GetAllGroupNotifications();

    Task<(Guid? id, UserGroupNotificationItem?)> GetLatestGroupNotification();
}

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