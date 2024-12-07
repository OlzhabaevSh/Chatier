namespace Chatier.Core.Features.ChatFeatures;

public interface IChatGrain : IGrainWithStringKey
{
    Task AddUserAsync(
        string userName);
    Task RemoveUserAsync(
        string userName);

    Task<Guid> SendMessageAsync(
        string userName,
        string message);

    Task<bool> RemoveMessageAsync(
        Guid messageId,
        string caller);

    Task<string[]> GetUsersAsync();

    Task<Dictionary<Guid, ChatMessageItem>> GetMessagesAsync();
}