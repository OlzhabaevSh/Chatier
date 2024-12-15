import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, IColumn, IconButton, IIconProps, IStackTokens, Selection, SelectionMode, Stack, TextField } from "@fluentui/react";
import { IChat } from "../hooks/useNotifications";
import React, { useState } from "react";

export interface IChatMasterProps {
    chats: IChat[];
    setSelectedChat: React.Dispatch<React.SetStateAction<string | undefined>>;
    selectedChat: string | undefined;
    createChat: (chatName: string) => Promise<void>;
}

const stackTokens: IStackTokens = { childrenGap: 5 };
const columns : IColumn[] = [
    { 
        key: 'name', 
        name: 'Chat Name',
        fieldName: 'name',
        minWidth: 100
    },
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

    const [selectedItem, setSelectedItem] = useState<IChat | undefined>(undefined);

    const selection = new Selection({
        onSelectionChanged: () => { 
            const selected = selection.getSelection()[0] as IChat;
            setSelectedItem(selected);
            props.setSelectedChat(selected?.name);
        }, 
    });

    return (
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
                    selection={selection} />
            </Stack.Item>
        </Stack>
    )
};