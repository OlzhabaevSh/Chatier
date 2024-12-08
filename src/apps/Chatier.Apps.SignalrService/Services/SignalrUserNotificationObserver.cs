using Chatier.Apps.SignalrService.Hubs;
using Chatier.Core.Features.UserFeatures;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Services;

public class SignalrUserNotificationObserver : IUserNotificationObserver
{
    private readonly IHubContext<UserHub> hubContext;

    public SignalrUserNotificationObserver(IHubContext<UserHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ReceiveNotification(
        Guid notificationId, 
        string chatName, 
        string message, 
        DateTimeOffset createdAt, 
        string recieverName)
    {
        await this.hubContext.Clients
            .Group(recieverName)
            .SendAsync(
                "ReceiveNotification", 
                notificationId, 
                chatName, 
                message, 
                createdAt);
    }
}
