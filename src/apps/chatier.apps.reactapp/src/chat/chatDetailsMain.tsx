import { ActivityItem, DocumentCard, DocumentCardActivity, DocumentCardDetails, DocumentCardTitle, FocusZone, IconButton, IIconProps, IStackTokens, Label, Link, List, Stack, TextField } from "@fluentui/react";
import { IChatDetailsProps } from "./chatDetails";
import { useState } from "react";
import { IMessage } from "../hooks/useNotifications";

const stackTokens: IStackTokens = { childrenGap: 5 };

const imojiIcon: IIconProps = { iconName: 'Send' };

const formatTime = (date: Date) => {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return dateObj.toLocaleTimeString('en-US', { 
        hour12: false 
    }); 
};

const activityItemStyles = { 
    root: { 
        display: 'flex', 
        flexDirection: 'column', 
        alignItems: 'flex-start', 
        width: '100%', 
    }, 
    activityContent: { 
        display: 'flex', 
        flexDirection: 'column', 
        alignItems: 'flex-start', 
        width: '100%', 
    }, 
};

export const ChatDetailsMain = (props: IChatDetailsProps) => {

    const [message, setMessage] = useState<string | undefined>(undefined);

    const sendMessage = async () => {
        if(!message) {
            return;
        }

        await props.sendMessage(props.selectedChat!, message);
        setMessage('');
    };

    const onRenderCell = (
        item: any, 
        index: number | undefined) => {
            const data = item as IMessage;

            if(!data) {
                return (<div>error</div>);
            }

            return (
                <ActivityItem
                    style={{
                        width: '100%', 
                        marginBottom: '2vh',
                        textAlign: 'left'
                    }}
                    key={data.id}
                    timeStamp={formatTime(data.createdAt)}
                    activityDescription={[
                        <>{data.senderName}</>,
                        <Label>{data.message}</Label>
                    ]} />
            );
        };

    return (
        <Stack 
            style={{width: '100%', height: '100%'}}
            tokens={stackTokens}>
            <Stack.Item 
                align="auto">
                <h4>{props.selectedChat}</h4>
            </Stack.Item>
            <Stack.Item 
                align="stretch"
                grow>
                    <List 
                        items={props.messages} 
                        onRenderCell={onRenderCell}/>
            </Stack.Item>
            <Stack.Item 
                align="auto">
                <Stack horizontal>
                    <Stack.Item grow>
                        <TextField 
                            placeholder="Type a message" 
                            onChange={(e, newValue) => setMessage(newValue)} />
                    </Stack.Item>
                    <Stack.Item>
                        <IconButton 
                            iconProps={imojiIcon}
                            onClick={sendMessage} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
        </Stack>
    );
};