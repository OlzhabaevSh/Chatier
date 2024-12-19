import { Meta, StoryFn } from '@storybook/react';
import { ChatMaster, IChatMasterProps } from '../chat/chatMaster';
import { Container } from '../layout/container';
import { initializeIcons } from '@fluentui/font-icons-mdl2';

export default {
    title: 'Chats/ChatMaster',
    component: ChatMaster
} as Meta;

initializeIcons();

const Template: StoryFn<typeof ChatMaster> = (
    args: IChatMasterProps) => 
        <div style={{width: '30%', height: '350px'}}>
            <Container>
                <ChatMaster {...args} />
            </Container>;
        </div>

const args : IChatMasterProps = {
    chats: [{
        name: 'Chat 1',
        newMessages: false
    }, {
        name: 'Chat 2',
        newMessages: true
    }],
    setSelectedChat: (chatName) => {
        console.log(`Selected chat: ${chatName}`);
    },
    createChat: async (chatName) => {
        console.log(`Creating chat: ${chatName}`);
    },
    selectedChat: undefined
};

export const Defaults = Template.bind({});
Defaults.args = args;