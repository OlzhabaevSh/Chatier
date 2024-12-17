using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.UserFeatures;
using System.Xml.Linq;

namespace Chatier.Apps.SignalrService.Services;

public interface IUserService
{
    Task SubscribeToEvents(
        string connectionId, 
        string userName);

    Task UnSubscribeFromEvents(
        string connectionId,
        string userName);

    Task<UserChatItem[]> GetAllChatsAsync(
        string userName);

    Task<string> CreateChatAsync(
        string initiatorUserName,
        string targetUserName);

    Task<Guid> SendMessageAsync(
        string chatName,
        string senderName,
        string message);

    Task<ChatDto[]> GetMessagesFromChatAsync(
        string chatName);
}

public class UserService : IUserService
{
    private readonly IUserNotificationChannel userNotificationChannel;
    private readonly IClusterClient clusterClient;

    public UserService(
        IUserNotificationChannel userNotificationChannel,
        IClusterClient clusterClient)
    {
        this.userNotificationChannel = userNotificationChannel;
        this.clusterClient = clusterClient;
    }

    public async Task SubscribeToEvents(
        string connectionId, 
        string userName) 
    {
        await this.userNotificationChannel.SubscribeAsync(
            userName,
            connectionId);
    }

    public async Task UnSubscribeFromEvents(
        string connectionId,
        string userName)
    {
        await this.userNotificationChannel.UnSubscribeAsync(
            userName,
            connectionId);
    }


    public async Task<UserChatItem[]> GetAllChatsAsync(
        string userName) 
    {
        var userChatGrain = this.clusterClient.GetGrain<IUserChatGrain>(userName);
        var chats = await userChatGrain.GetChatsAsync();
        return chats;
    }

    public async Task<string> CreateChatAsync(
        string initiatorUserName, 
        string targetUserName) 
    {
        var userChatGrain = this.clusterClient.GetGrain<IUserChatGrain>(initiatorUserName);
        var chatId = await userChatGrain.CreateChatAsync(targetUserName);

        return chatId;
    }

    public async Task<Guid> SendMessageAsync(
        string chatName,
        string senderName,
        string message)
    {
        var chatGrain = this.clusterClient.GetGrain<IChatGrain>(chatName);
        var messageId = await chatGrain.SendMessageAsync(senderName, message);
        return messageId;
    }

    public async Task<ChatDto[]> GetMessagesFromChatAsync(
        string chatName) 
    {
        var chatGrain = this.clusterClient.GetGrain<IChatGrain>(chatName);

        var messages = await chatGrain.GetMessagesAsync();

        return messages.Select(x => new ChatDto
        {
            Id = x.Key,
            SenderName = x.Value.Sender,
            Message = x.Value.Message,
            CreatedAt = x.Value.CreatedAt,
            ChatName = chatName
        }).ToArray();
    }
}

public class ChatDto
{
    public Guid Id { get; init; }

    public required string SenderName { get; init; }

    public required string Message { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public required string ChatName { get; init; }
}