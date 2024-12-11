import { IUserChatNotification } from "../services/signalr";

interface IChatMasterProps {
    chats: IUserChatNotification[];
    setSelectedChat: React.Dispatch<React.SetStateAction<string | undefined>>;
}

export const ChatMaster = (props: IChatMasterProps) => {
    return (<div>master page</div>)
};