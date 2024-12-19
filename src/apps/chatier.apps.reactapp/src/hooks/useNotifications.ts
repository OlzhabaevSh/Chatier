import { useEffect, useState } from 'react';
import { 
  eventTarget, 
  IUserChat, 
  IUserChatNotification, 
  IUserMessageNotification, 
  startConnection,
  createChat,
  sendMessage,
  IChatMessage,
  selectChat,
  getChats
} from '../services/signalr';

export interface IChat {
  name: string;
  newMessages: boolean;
}

export interface IMessage {
  id: string;
  chatName: string;
  senderName: string;
  message: string;
  createdAt: Date;
  notificationId?: string | undefined;
}

export const useNotifications = () => {
  const [chatNotifications, setChatNotifications] = useState<IUserChatNotification[]>([]);
  const [messageNotifications, setMessageNotifications] = useState<IUserMessageNotification[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(false);

  const [chats, setChats] = useState<IChat[]>([]);
  const [messages, setMessages] = useState<IMessage[]>([]);

  const [latestMessage, setLatestMessage] = useState<IMessage | undefined>(undefined);

  const [selectedChat, setSelectedChat] = useState<string | undefined>(undefined);

  useEffect(() => {
    const handleChatNotificationReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserChatNotification>;
      setChatNotifications(prevChatNotifications => [...prevChatNotifications, customEvent.detail]);

      const chatName = customEvent.detail.chatName;
      setChats(prevChats => {
        if(prevChats.some(chat => chat.name === chatName)) {
          return prevChats;
        }
        return [
          ...prevChats, 
          { 
            name: chatName,
            newMessages: false
          }];
      });
    };
    eventTarget.addEventListener('chatNotificationReceived', handleChatNotificationReceived);

    const handleMessageNotificationReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserMessageNotification>;

      setMessageNotifications(prevMessageNotifications => [...prevMessageNotifications, customEvent.detail]);
      
      setLatestMessage(customEvent.detail);
    };
    eventTarget.addEventListener('messageNotificationReceived', handleMessageNotificationReceived);

    const handleChatsReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserChat[]>;
      customEvent.detail.forEach(chat => {
        setChats(prevChats => {
          if(prevChats.some(c => c.name === chat.name)) {
            return prevChats;
          }
          return [
            ...prevChats, 
            { 
              name: chat.name,
              newMessages: false 
            }];
        });
      });
    };
    eventTarget.addEventListener('chatsReceived', handleChatsReceived);

    const messagesReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IChatMessage[]>;
      setMessages(customEvent.detail);
    };
    eventTarget.addEventListener('messagesReceived', messagesReceived);

    startConnection().then(() => {
      setIsConnected(true);
      getChats();
    });

    return () => {
      eventTarget.removeEventListener('chatNotificationReceived', handleChatNotificationReceived);
      eventTarget.removeEventListener('messageNotificationReceived', handleMessageNotificationReceived);
      eventTarget.removeEventListener('chatsReceived', handleChatsReceived);
      eventTarget.removeEventListener('messagesReceived', messagesReceived);
    };
  }, []);

  useEffect(() => {
    if(!selectedChat) {
      return;
    }

    selectChat(selectedChat);
  }, [selectedChat]);

  useEffect(() => {
    if(!latestMessage) {
      return;
    }
    console.info('shyn 1.1');
    if(selectedChat !== latestMessage.chatName) {
      console.info('shyn 1.1.1');
      /*
      setChats(prevChats => prevChats.map(chat => {
        if(chat.name === latestMessage.chatName) {
          return {
            ...chat,
            newMessages: true
          };
        }
        return chat;
      }));
      */
    } else {
      console.info('shyn 1.1.2');
      /*
      const updatedChats = chats.map(chat => {
        if(chat.name === selectedChat) {
          return {
            ...chat,
            newMessages: false
          };
        }
        return chat;
      });
      setChats(updatedChats);
      */
      setMessages(prevMessages => [...prevMessages, {
        id: latestMessage.id,
        chatName: latestMessage.chatName,
        senderName: latestMessage.senderName,
        message: latestMessage.message,
        createdAt: latestMessage.createdAt
      }]);
    }
  }, [latestMessage, selectedChat]);

  return { 
    chatNotifications, 
    messageNotifications, 
    isConnected,
    chats,
    messages,
    createChat,
    sendMessage,
    selectedChat,
    setSelectedChat
  };
};
