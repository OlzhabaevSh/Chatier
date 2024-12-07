namespace Chatier.Core.Features.UserFeatures;

public interface IUserGrain : IGrainWithStringKey
{
    Task NotifyAboutAddingToChatAsync(
        string chat,
        string userName);

    Task NotifyAboutLeavingAChatAsync(
        string chat,
        string userName);

    Task NotifyAboutNewMessageAsync(
        string chat,
        string sender,
        Guid messageId,
        string message);

    Task ConfirmNotificationAsync(
        Guid notificationId);

    Task<Guid> GetLatestMessageNotificationIdAsync();
}