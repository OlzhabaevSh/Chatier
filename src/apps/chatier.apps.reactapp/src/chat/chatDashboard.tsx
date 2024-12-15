import { IStackStyles, Stack } from "@fluentui/react";
import { ChatMaster } from "./chatMaster";
import { ChatDetails } from "./chatDetails";
import { useEffect } from "react";
import { useNotifications } from "../hooks/useNotifications";
import { Container } from "../layout/container";

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
    const { 
        chats, 
        messages, 
        createChat, 
        sendMessage, 
        selectedChat,
        setSelectedChat,
    } = useNotifications();

    useEffect(() => {
        if(!selectedChat) {
            return;
        };

        setSelectedChat(selectedChat);
    }, [setSelectedChat, selectedChat]);

    return (
        <Stack
            horizontalAlign="baseline" 
            verticalAlign="baseline" 
            verticalFill
            horizontal
            styles={stackStyles}>
                <Stack.Item align="auto" grow={3}>
                    <Container>
                        <ChatMaster 
                            chats={chats}
                            setSelectedChat={setSelectedChat}
                            createChat={createChat}
                            selectedChat={selectedChat} />
                    </Container>
                </Stack.Item>
                <Stack.Item align="stretch" grow={7}>
                    <Container>
                        <ChatDetails 
                            selectedChat={selectedChat}
                            messages={messages}
                            sendMessage={sendMessage} />
                    </Container>
                </Stack.Item>
        </Stack>
    );
};