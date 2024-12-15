import { IMessage } from "../hooks/useNotifications";
import { ChatDetailsEmpty } from "./ChatDetailsEmpty";
import { ChatDetailsMain } from "./chatDetailsMain";

export interface IChatDetailsProps {
    messages: IMessage[];
    selectedChat: string | undefined
    sendMessage: (chatName: string, message: string) => Promise<void>;
}

export const ChatDetails = (props: IChatDetailsProps) => {
    return (
        <>
            {props.selectedChat ? <ChatDetailsMain {...props} /> : <ChatDetailsEmpty />}
        </>
    )
};