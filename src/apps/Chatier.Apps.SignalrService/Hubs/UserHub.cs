using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.UserFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Hubs;

[AllowAnonymous]
public class UserHub : Hub
{
    private readonly IUserNotificationChannel userNotificationChannel;
    private readonly IClusterClient clusterClient;

    public UserHub(
        IUserNotificationChannel userNotificationChannel,
        IClusterClient clusterClient)
    {
        this.userNotificationChannel = userNotificationChannel;
        this.clusterClient = clusterClient;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.Groups.AddToGroupAsync(connectionId, userName);

        await this.userNotificationChannel.SubscribeAsync(
            userName, 
            connectionId);

        var userChatGrain = this.clusterClient.GetGrain<IUserChatGrain>(userName);
        var chats = await userChatGrain.GetChatsAsync();

        await this.Clients.Caller.SendAsync(
            "ReceiveChats", 
            chats);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.userNotificationChannel.UnSubscribeAsync(
            userName,
            connectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateChatAsync(string userName) 
    {
        var myName = this.GetUserName();
        var userChatGrain = this.clusterClient.GetGrain<IUserChatGrain>(myName);
        var chatId = await userChatGrain.CreateChatAsync(userName);
    }

    public async Task SendMessageAsync(string chatName, string message)
    {
        var myName = this.GetUserName();

        var chatGrain = this.clusterClient.GetGrain<IChatGrain>(chatName);
        await chatGrain.SendMessageAsync(myName, message);
    }

    public async Task GetMessagesFromChatAsync(string chatName) 
    {
        var myName = this.GetUserName();

        var chatGrain = this.clusterClient.GetGrain<IChatGrain>(chatName);

        var messages = await chatGrain.GetMessagesAsync();

        var result = messages.Select(x => new 
        {
            Id = x.Key,
            x.Value.Sender,
            x.Value.Message,
            x.Value.CreatedAt,
            ChatName = chatName
        }).ToList();

        await this.Clients.Caller.SendAsync(
            "ReceiveMessages",
            result);
    }

    private string GetUserName()
    {
        var httpContext = this.Context.GetHttpContext();
        var userName = httpContext.Request.Query["userName"];
        return userName;
    }
}
