using Chatier.Apps.SignalrService.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Hubs;

public class UserHub : Hub
{
    private readonly IHubGroupStore groupStore;
    private readonly IUserSubscriptionQueue userSubscriptionQueue;
    private readonly IUserUnSubscriptionQueue userUnSubscriptionQueue;

    public UserHub(
        IHubGroupStore groupStore, 
        IUserSubscriptionQueue userSubscriptionQueue,
        IUserUnSubscriptionQueue userUnSubscriptionQueue)
    {
        this.groupStore = groupStore;
        this.userSubscriptionQueue = userSubscriptionQueue;
        this.userUnSubscriptionQueue = userUnSubscriptionQueue;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        var isGroupHasConnections = await this.groupStore
            .IsGroupHasConnections(userName);

        await this.groupStore.AddToGroupAsync(userName, connectionId);
        await this.Groups.AddToGroupAsync(connectionId, userName);

        if(!isGroupHasConnections)
        {
            this.userSubscriptionQueue.Enqueue(userName);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = this.Context.ConnectionId;
        var userName = this.GetUserName();

        await this.groupStore.RemoveFromGroupAsync(userName, connectionId);
        await this.Groups.RemoveFromGroupAsync(connectionId, userName);

        var isGroupHasConnections = await this.groupStore
            .IsGroupHasConnections(userName);

        if (!isGroupHasConnections)
        {
            this.userUnSubscriptionQueue.Enqueue(userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserName()
    {
        return Context.GetHttpContext().Request.Headers["Chatier-User-Name"];
    }
}
