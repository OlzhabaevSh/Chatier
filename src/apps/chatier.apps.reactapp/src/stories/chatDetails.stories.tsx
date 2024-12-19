import { Meta, StoryFn } from '@storybook/react';
import { ChatDetails, IChatDetailsProps } from '../chat/chatDetails';
import { Container } from '../layout/container';
import { initializeIcons } from '@fluentui/font-icons-mdl2';

export default {
    title: 'Chats/ChatDetails',
    component: ChatDetails
} as Meta;

initializeIcons();

const Template: StoryFn<typeof ChatDetails> = (
    args: IChatDetailsProps) => 
        <div style={{width: '60%', height: '450px'}}>
            <Container>
                <ChatDetails {...args} />
            </Container>;
        </div>

const defaultArgs : IChatDetailsProps = {
    selectedChat: 'alpha-beta',
    ownName: 'alpha',
    messages: [
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'Hello, beta!', createdAt: new Date(), id: '1' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'Hello, alpha!', createdAt: new Date(), id: '2' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'How are you?', createdAt: new Date(), id: '3' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'Im fine, thank you!', createdAt: new Date(), id: '4' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'Good to hear that!', createdAt: new Date(), id: '4' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'How about you?', createdAt: new Date(), id: '5' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'Im fine too!', createdAt: new Date(), id: '6' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'Good to hear that!', createdAt: new Date(), id: '7' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'How about you?', createdAt: new Date(), id: '8' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'Im fine too!', createdAt: new Date(), id: '9' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'Good to hear that!', createdAt: new Date(), id: '10' },
        { chatName: 'alpha-beta', senderName: 'beta', message: 'How about you?', createdAt: new Date(), id: '11' },
        { chatName: 'alpha-beta', senderName: 'alpha', message: 'Im fine too!', createdAt: new Date(), id: '12' }
    ],
    sendMessage: async (chatName: string, message: string) => {
        console.log(`Sending message to ${chatName}: ${message}`);
    }
};

export const Defaults = Template.bind({});
Defaults.args = defaultArgs;

const emptyArgs : IChatDetailsProps = {
    selectedChat: undefined,
    ownName: 'alpha',
    messages: [],
    sendMessage: async (chatName: string, message: string) => {
        console.log(`Sending message to ${chatName}: ${message}`);
    }
};

export const EmtpyChat = Template.bind({});
EmtpyChat.args = emptyArgs;