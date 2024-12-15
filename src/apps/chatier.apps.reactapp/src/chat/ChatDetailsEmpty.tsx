import { DocumentCard, DocumentCardTitle, IStackStyles, Stack } from "@fluentui/react";

const centerStyles: IStackStyles = {
    root: {
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center', 
        height: '100%'
    }
};

export const ChatDetailsEmpty = () => {
    return(
        <Stack styles={centerStyles}>
            <DocumentCard>
                <DocumentCardTitle title="Empty" />
            </DocumentCard>
        </Stack>
    );
};