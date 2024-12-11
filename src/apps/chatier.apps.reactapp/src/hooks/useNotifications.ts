import { useEffect, useState } from 'react';
import { eventTarget, IUserChatNotification, IUserMessageNotification, startConnection } from '../services/signalr';

export const useNotifications = () => {
  const [chats, setChats] = useState<IUserChatNotification[]>([]);
  const [messages, setMessages] = useState<IUserMessageNotification[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(false);

  useEffect(() => {
    const handleChatNotificationReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserChatNotification>;
      setChats(prevChats => [...prevChats, customEvent.detail]);
    };

    const handleMessageNotificationReceived = (event: Event) => {
      const customEvent = event as CustomEvent<IUserMessageNotification>;
      setMessages(prevMessages => [...prevMessages, customEvent.detail]);
    };

    eventTarget.addEventListener('chatNotificationReceived', handleChatNotificationReceived);
    eventTarget.addEventListener('messageNotificationReceived', handleMessageNotificationReceived);

    startConnection().then(() => setIsConnected(true));

    return () => {
      eventTarget.removeEventListener('chatNotificationReceived', handleChatNotificationReceived);
      eventTarget.removeEventListener('messageNotificationReceived', handleMessageNotificationReceived);
    };
  }, []);

  return { chats, messages, isConnected };
};
