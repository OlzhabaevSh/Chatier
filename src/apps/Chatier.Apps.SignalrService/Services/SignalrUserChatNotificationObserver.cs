using Chatier.Apps.SignalrService.Hubs;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.AspNetCore.SignalR;

namespace Chatier.Apps.SignalrService.Services;

public class SignalrUserChatNotificationObserver : IUserChatNotificationObserver
{
    private readonly IHubContext<UserHub> hubContext;

    public SignalrUserChatNotificationObserver(IHubContext<UserHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task ReceiveNotification(
        Guid notificationId, 
        string chatName, 
        string userName, 
        UserGroupNotificationType notificationType, 
        DateTimeOffset createdAt, 
        string receiverName)
    {
        await this.hubContext.Clients
            .Group(receiverName)
            .SendAsync(
                "ChatNotifications",
                notificationId,
                chatName,
                userName,
                notificationType,
                createdAt);
    }
}
