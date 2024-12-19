import { 
    ActivityItem, 
    FontSizes, 
    FontWeights, 
    IconButton, 
    IIconProps, 
    IStackTokens, 
    Label, 
    List, 
    SharedColors, 
    Stack, 
    Text, 
    TextField 
} from "@fluentui/react";
import { IChatDetailsProps } from "./chatDetails";
import { useState } from "react";
import { IMessage } from "../hooks/useNotifications";

const stackTokens: IStackTokens = { childrenGap: 2 };

const imojiIcon: IIconProps = { iconName: 'Send' };

const formatTime = (date: Date) => {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return dateObj.toLocaleTimeString('en-US', { 
        hour12: false 
    }); 
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

            const i = index! ? 0 : index;
            const key = `${data.id}-${i}`;
            const fontColor = data.senderName != props.ownName 
                ? SharedColors.red20 
                : SharedColors.gray20;

            return (
                <ActivityItem
                    style={{
                        width: '100%', 
                        marginBottom: '1vh',
                        textAlign: 'left'
                    }}
                    key={`${key}`}
                    timeStamp={formatTime(data.createdAt)}
                    activityDescription={[
                        <Text style={{ fontWeight: FontWeights.semibold, fontSize: FontSizes.size14, color: fontColor }} key={1}>{data.senderName}</Text>,
                        <br key={2}/>,
                        <Text style={{ fontWeight: FontWeights.regular, fontSize: FontSizes.size16 }} key={3}>{data.message}</Text>
                    ]} />
            );
        };

    return (
        <Stack 
            verticalFill
            styles={{ root: { width: '100%', height: '100%', display: 'flex', flexDirection: 'column' } }}
            tokens={stackTokens}>
            <Stack.Item 
                align="auto">
                <Label style={{ fontWeight: FontWeights.light, fontSize: FontSizes.size20 }}>{props.selectedChat}</Label>
            </Stack.Item>
            <Stack.Item 
                grow 
                styles={{ root: { overflowY: 'auto' } }}>
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