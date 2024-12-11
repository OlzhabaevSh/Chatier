using Chatier.Apps.SignalrService.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Hubs;

public class UserHub : Hub
{
    private readonly IUserNotificationChannel userNotificationChannel;

    public UserHub(
        IUserNotificationChannel userNotificationChannel)
    {
        this.userNotificationChannel = userNotificationChannel;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.Groups.AddToGroupAsync(connectionId, userName);

        await this.userNotificationChannel.SubscribeAsync(
            userName, 
            connectionId);

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

    private string GetUserName()
    {
        return Context.GetHttpContext().Request.Headers["Chatier-User-Name"];
    }
}
