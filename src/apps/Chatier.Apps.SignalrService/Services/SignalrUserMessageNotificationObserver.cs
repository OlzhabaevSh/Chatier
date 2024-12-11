using Chatier.Apps.SignalrService.Hubs;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Services;

public class SignalrUserMessageNotificationObserver : IUserMessageNotificationObserver
{
    private readonly IHubContext<UserHub> hubContext;

    public SignalrUserMessageNotificationObserver(IHubContext<UserHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ReceiveNotification(
        Guid notificationId, 
        string chatName,
        string senderName,
        string message, 
        DateTimeOffset createdAt, 
        string receiverName)
    {
        await this.hubContext.Clients
            .Group(receiverName)
            .SendAsync(
                "MessageNotification", 
                notificationId, 
                chatName,
                senderName,
                message, 
                createdAt);
    }
}
