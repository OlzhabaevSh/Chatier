namespace Chatier.Core.Features.ChatFeatures;

[GenerateSerializer]
public class ChatGrainUsersState
{
    [Id(0)]
    public required HashSet<string> Users { get; set; } = new HashSet<string>();
}

[GenerateSerializer]
public class ChatGrainMessagesState
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