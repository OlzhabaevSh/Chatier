import * as signalR from '@microsoft/signalr';

interface IUserChatNotification {
    notificationId: string;
    chatName: string;
    userName: string;
    notificationType: 0 | 1, // 0 = Joined, 1 = Left
    createdAt: Date;
}

interface IUserMessageNotification {
    notificationId: string;
    chatName: string,
    senderName: string,
    message: string,
    createdAt: Date;
}

const key = 'chatierUserName';
const userName = window.localStorage.getItem(key); 

console.info('User Name: ', userName);

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7149/userHub', {
    headers: {
      'Chatier-User-Name': userName!,
    },
  })
  .withAutomaticReconnect()
  .build();

const eventTarget = new EventTarget();

connection.on('ChatNotifications', (notification: IUserChatNotification) => {
  const event = new CustomEvent('chatNotificationReceived', { detail: notification });
  eventTarget.dispatchEvent(event);
});

connection.on('MessageNotification', (notification: IUserMessageNotification) => {
  const event = new CustomEvent('messageNotificationReceived', { detail: notification });
  eventTarget.dispatchEvent(event);
});

const startConnection = async () => {
  try {
    await connection.start();
    console.log('SignalR Connected');
  } catch (err) {
    console.error('SignalR Connection Error: ', err);
  }
};

export { eventTarget, startConnection };  
export type { IUserChatNotification, IUserMessageNotification };

