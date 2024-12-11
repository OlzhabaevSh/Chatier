import { IUserMessageNotification } from "../services/signalr";
import { ChatDetailsEmpty } from "./ChatDetailsEmpty";

interface IChatDetailsProps {
    messages: IUserMessageNotification[];
    selectedChat: string | undefined;
}

export const ChatDetails = (props: IChatDetailsProps) => {
    return (<>
        {props.selectedChat ? <div>List</div> : <ChatDetailsEmpty />}
    </>)
};