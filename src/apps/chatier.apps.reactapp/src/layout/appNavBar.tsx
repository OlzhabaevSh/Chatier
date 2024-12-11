import { CommandBar, ICommandBarItemProps, ICommandBarStyles, IStackStyles, IStackTokens, Stack, Text } from '@fluentui/react';
import { useEffect, useState } from 'react';
import { useUser } from '../hooks/useUser';

interface IAppNavBarProps {
    title: string;
}

const stackTokens: IStackTokens = { childrenGap: 15 };
const stackStyles : Partial<IStackStyles> = { 
    root: { 
        // padding: '22px',
        padding: '1vh 5vh', 
        backgroundColor: '#f3f2f1', 
        width: '100%' 
    } 
};
const commandBarStyles : Partial<ICommandBarStyles> = { 
    root: { 
        margin: 0, 
        padding: 0, 
        backgroundColor: 'transparent', 
    }, 
};

export const AppNavBar = (props: IAppNavBarProps) => { 

    const [userName] = useUser();
    const [commands, setCommands] = useState<ICommandBarItemProps[]>([]);
    
    useEffect(() => {
        if (userName){
            setCommands([{
                key: 'logout',
                text: `Logout (${userName})`,
                iconProps: { iconName: 'logout' },
                onClick: () => console.log('Logout'),
            }]);
        } else {
            setCommands([{
                key: 'login',
                text: `Login`,
                iconProps: { iconName: 'login' },
                onClick: () => console.log('Login'),
            }]);
        }
    }, [userName]);

    return (
        <Stack 
            horizontal
            horizontalAlign="space-between" 
            verticalAlign="center" 
            styles={stackStyles}
            tokens={stackTokens}>
                <Text 
                    variant="mediumPlus">
                    {props.title}
                </Text>
                <CommandBar 
                    items={commands}
                    styles={commandBarStyles} />
        </Stack>);
};