using Chatier.Apps.SignalrService.Hubs;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

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
                new EventModel() 
                {
                    NotificationId = notificationId,
                    ChatName = chatName,
                    UserName = userName,
                    NotificationType = notificationType,
                    CreatedAt = createdAt
                });
    }

    class EventModel 
    {
        public Guid NotificationId { get; set; }
        public string ChatName { get; set; }
        public string UserName { get; set; }
        public UserGroupNotificationType NotificationType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
