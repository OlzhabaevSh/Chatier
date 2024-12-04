namespace Chatier.Core.Features.ChatFeatures;

public interface IChatGrain : IGrainWithStringKey
{
    Task<string> GetNameAsync();
    Task AddUserAsync(
        string userName);
    Task RemoveUserAsync(
        string userName);

    Task SendMessageAsync(
        string userName,
        string message);

    Task<string[]> GetUsersAsync();

    Task<Dictionary<Guid, ChatMessageItem>> GetMessagesAsync();
}

[GenerateSerializer]
public class ChatUsersState
{
    [Id(0)]
    public required HashSet<string> Users { get; set; } = new HashSet<string>();
}

[GenerateSerializer]
public class ChatMessagesState
{
    [Id(0)]
    public required Dictionary<Guid, ChatMessageItem> Messages { get; set; } = new Dictionary<Guid, ChatMessageItem>();
}

[GenerateSerializer]
public class ChatMessageItem
{
    [Id(0)]
    public required string Sender { get; set; }

    [Id(1)]
    public required string Message { get; set; }

    [Id(2)]
    public DateTimeOffset CreatedAt { get; set; }
}