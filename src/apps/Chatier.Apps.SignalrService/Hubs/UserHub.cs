using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.UserFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Hubs;

[AllowAnonymous]
public class UserHub : Hub
{
    private readonly IUserService userService;

    public UserHub(
        IUserService userService)
    {
        this.userService = userService;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.Groups.AddToGroupAsync(
            connectionId, 
            userName);

        await this.userService.SubscribeToEvents(
            connectionId,
            userName);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.Groups.RemoveFromGroupAsync(
            connectionId,
            userName);

        await this.userService.UnSubscribeFromEvents(
            connectionId,
            userName);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetChatsAsync() 
    {
        var userName = this.GetUserName();

        var chats = await this.userService.GetAllChatsAsync(userName);

        await this.Clients.Caller.SendAsync(
            "ReceiveChats",
            chats);
    }

    public async Task CreateChatAsync(string userName) 
    {
        var myName = this.GetUserName();
        _ = await this.userService.CreateChatAsync(myName, userName);
    }

    public async Task SendMessageAsync(string chatName, string message)
    {
        var myName = this.GetUserName();

        _ = await this.userService.SendMessageAsync(
            chatName,
            myName,
            message);
    }

    public async Task GetMessagesFromChatAsync(string chatName) 
    {
        var myName = this.GetUserName();

        var result = await this.userService.GetMessagesFromChatAsync(chatName);

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
