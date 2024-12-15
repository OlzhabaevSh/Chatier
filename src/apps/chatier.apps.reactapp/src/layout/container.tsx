import { IStackStyles, Stack } from "@fluentui/react";

const containerStyles: IStackStyles = { 
    root: { 
        width: '100%',
        height: '100%', 
        margin: '0 auto', 
        padding: '5px', 
        border: '1px solid #ccc', 
        borderRadius: '4px', 
        background: '#f9f9f9', 
    }, 
};

export const Container : React.FC = ({children}) => {
    return(
        <Stack styles={containerStyles}>
            {children}
        </Stack>
    )
};