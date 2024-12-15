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
  selectChat
} from '../services/signalr';

export interface IChat {
  name: string;
  newMessages: boolean;
}

export interface IMessage {
  id: string;
  chatName: string;
  sender: string;
  message: string;
  createdAt: Date;
}

export const useNotifications = () => {
  const [chatNotifications, setChatNotifications] = useState<IUserChatNotification[]>([]);
  const [messageNotifications, setMessageNotifications] = useState<IUserMessageNotification[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(false);

  const [chats, setChats] = useState<IChat[]>([]);
  const [messages, setMessages] = useState<IMessage[]>([]);

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

    const handleMessageNotificationReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserMessageNotification>;

      setMessageNotifications(prevMessageNotifications => [...prevMessageNotifications, customEvent.detail]);

      const chatName = customEvent.detail.chatName;
      if(chatName !== selectedChat) {
        return;
      }

      setChats(prevChats => [...prevChats, { name: chatName, newMessages: true }]);
    };

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

    const messagesReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IChatMessage[]>;
      console.info('Shyn 1', customEvent.detail);
      setMessages(customEvent.detail);
    };

    eventTarget.addEventListener('chatNotificationReceived', handleChatNotificationReceived);
    eventTarget.addEventListener('messageNotificationReceived', handleMessageNotificationReceived);
    eventTarget.addEventListener('chatsReceived', handleChatsReceived);
    eventTarget.addEventListener('messagesReceived', messagesReceived);

    startConnection().then(() => setIsConnected(true));

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
