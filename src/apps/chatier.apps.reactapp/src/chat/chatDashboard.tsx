import { IStackStyles, Stack } from "@fluentui/react";
import { ChatMaster } from "./chatMaster";
import { ChatDetails } from "./chatDetails";
import { useState } from "react";
import { useNotifications } from "../hooks/useNotifications";

const stackStyles: Partial<IStackStyles> = {
    root: {
        margin: '0 auto',
        textAlign: 'center',
        color: '#605e5c',
        width: '100%',
        background: 'lightgray',
    }
};

export const ChatDashboard = () => {

    const [selectedChat, setSelectedChat] = useState<string | undefined>(undefined);
    const { chats, messages } = useNotifications();
    
    return (
        <Stack
            horizontalAlign="baseline" 
            verticalAlign="baseline" 
            verticalFill
            horizontal
            styles={stackStyles}>
                <Stack.Item align="auto" grow={3}>
                    <ChatMaster 
                        chats={chats}
                        setSelectedChat={setSelectedChat} />
                </Stack.Item>
                <Stack.Item align="stretch" grow={7}>
                    <ChatDetails 
                        messages={messages}
                        selectedChat={selectedChat} />
                </Stack.Item>
        </Stack>
    );
};