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
        string receiverName,
        Guid messageId)
    {
        await this.hubContext.Clients
            .Group(receiverName)
            .SendAsync(
                "MessageNotification",
                new EventModel() 
                {
                    NotificationId = notificationId,
                    ChatName = chatName,
                    SenderName = senderName,
                    Message = message,
                    CreatedAt = createdAt,
                    MessageId = messageId
                });
    }

    class EventModel
    {
        public Guid NotificationId { get; set; }
        public string ChatName { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid MessageId { get; set; }
    }
}
