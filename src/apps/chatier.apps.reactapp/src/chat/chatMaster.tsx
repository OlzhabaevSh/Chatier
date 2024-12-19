import { 
    CheckboxVisibility, 
    DetailsList, 
    DetailsListLayoutMode, 
    FocusZone, 
    IColumn, 
    Icon, 
    IconButton, 
    IIconProps, 
    IStackTokens, 
    Selection, 
    SelectionMode, 
    SharedColors, 
    Stack, 
    TextField 
} from "@fluentui/react";
import { IChat } from "../hooks/useNotifications";
import React, { ReactNode, useState } from "react";

export interface IChatMasterProps {
    chats: IChat[];
    setSelectedChat: React.Dispatch<React.SetStateAction<string | undefined>>;
    selectedChat: string | undefined;
    createChat: (chatName: string) => Promise<void>;
}

const stackTokens: IStackTokens = { childrenGap: 5 };
const columns : IColumn[] = [
    {
        key: 'newMessages',
        name: 'New Messages',
        fieldName: 'newMessages',
        minWidth: 15,
    },
    { 
        key: 'name', 
        name: 'Chat Name',
        fieldName: 'name',
        minWidth: 100
    }
];

const imojiIcon: IIconProps = { iconName: 'Add' };

export const ChatMaster = (props: IChatMasterProps) => {
    
    const [chatName, setChatName] = useState<string|undefined>('');

    const createChat = async () => {
        if(chatName) {
            await props.createChat(chatName);
            setChatName('');
        }
    }

    const selection = new Selection({
        onSelectionChanged: () => { 
            const selected = selection.getSelection()[0] as IChat;
            props.setSelectedChat(selected?.name);
        }, 
    });

    const renderColumnt = (
        item?: IChat, 
        index?: number | undefined, 
        column?: IColumn | undefined) : ReactNode => {

            if(!item || !column) {
                return <></>;
            }

            const fieldContent = item[column.fieldName as keyof IChat] as string;

            switch(column.key) {
                case 'newMessages':
                    const iconName = item.newMessages ? 'AlertSolid' : 'Contact';
                    const iconColor = item.newMessages ? SharedColors.red20 : SharedColors.gray20;
                    return <Icon iconName={iconName} style={{fontSize: '15px', color: iconColor}} />
                default:
                    return <span key={`${item.name}-${column.key}`}>{fieldContent}</span>;;
            }
    };

    return (
        <FocusZone>
            <Stack
                tokens={stackTokens}
                style={{width: '100%', height: '100%'}}>
                <Stack.Item 
                    align="auto"
                    grow={1}>
                        <Stack 
                            horizontal
                            tokens={stackTokens}>
                                <Stack.Item
                                    align="stretch"
                                    grow>
                                    <TextField 
                                        placeholder="Enter chat name"
                                        onChange={(e, newValue) => setChatName(newValue)}/>
                                </Stack.Item>
                                <Stack.Item
                                    align="auto">
                                    <IconButton
                                        iconProps={imojiIcon}
                                        onClick={createChat} />
                                </Stack.Item>
                        </Stack>
                </Stack.Item>
                <Stack.Item
                    align="stretch" 
                    grow={8}>
                    <DetailsList 
                        items={props.chats} 
                        columns={columns} 
                        layoutMode={DetailsListLayoutMode.fixedColumns}
                        checkboxVisibility={CheckboxVisibility.hidden}
                        isHeaderVisible={false}
                        selectionMode={SelectionMode.single}
                        selectionPreservedOnEmptyClick={true}
                        selection={selection} 
                        onRenderItemColumn={renderColumnt}/>
                </Stack.Item>
            </Stack>
        </FocusZone>
    )
};