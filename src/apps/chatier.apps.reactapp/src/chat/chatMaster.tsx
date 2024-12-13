import { DetailsList, DetailsListLayoutMode, IColumn, IStackTokens, PrimaryButton, Stack } from "@fluentui/react";
import { IUserChatNotification } from "../services/signalr";

interface IChatMasterProps {
    chats: IUserChatNotification[];
    setSelectedChat: React.Dispatch<React.SetStateAction<string | undefined>>;
}

const stackTokens: IStackTokens = { childrenGap: 2 };

export const ChatMaster = (props: IChatMasterProps) => {
    const columns : IColumn[] = [
        { 
            key: 'chatName', 
            name: 'chatName',
            fieldName: 'chatName', 
            minWidth: 100, 
            maxWidth: 200, 
            isResizable: true 
        },
    ];
    return (
        <Stack
            tokens={stackTokens}>
            <Stack.Item 
                align="stretch">
                <PrimaryButton 
                    text="Create chat" />
            </Stack.Item>
            <Stack.Item
                align="stretch" 
                grow>
                <DetailsList 
                    items={props.chats} 
                    columns={columns} 
                    layoutMode={DetailsListLayoutMode.fixedColumns}/>
            </Stack.Item>
        </Stack>
    )
};