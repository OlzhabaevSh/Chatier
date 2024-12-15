import * as signalR from '@microsoft/signalr';

interface IUserChatNotification {
    notificationId: string;
    chatName: string;
    userName: string;
    notificationType: 0 | 1, // 0 = Joined, 1 = Left
    createdAt: Date;
}

interface IUserMessageNotification {
    id: string;
    notificationId: string;
    chatName: string,
    senderName: string,
    message: string,
    createdAt: Date;
}

export interface IUserChat {
  name: string;
  FriendlyName: string;
  ChatType: 0 | 1 | 2; // ownm, chat, group
  owner: string;
}

export interface IChatMessage {
  id: string;
  chatName: string;
  sender: string;
  message: string;
  createdAt: Date;
}

const key = 'chatierUserName';
const userName = window.localStorage.getItem(key); 

console.info('User Name: ', userName);

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`https://localhost:7149/userHub?userName=${userName}`)
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

connection.on('ReceiveChats', (chats: IUserChat[]) => {
  const event = new CustomEvent('chatsReceived', { detail: chats });
  eventTarget.dispatchEvent(event);
});

connection.on('ReceiveMessages', (messages: IChatMessage[]) => {
  const event = new CustomEvent('messagesReceived', { detail: messages });
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

const createChat = async (userName: string) => {
  await connection.invoke('CreateChatAsync', userName);
};

const sendMessage = async (chatName: string, message: string) => {
  await connection.invoke('SendMessageAsync', chatName, message);
};

const selectChat = async (chatName: string) => {
  await connection.invoke('GetMessagesFromChatAsync', chatName);
};

export { eventTarget, startConnection, createChat, sendMessage, selectChat };  
export type { IUserChatNotification, IUserMessageNotification };

