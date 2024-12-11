import { IStackStyles, PrimaryButton, Stack, TextField } from "@fluentui/react";
import { useUser } from "../hooks/useUser";
import { useEffect, useState } from "react";
import { useHistory } from "react-router-dom";

const stackTokens = { childrenGap: 15 };

const stackStyles: Partial<IStackStyles> = {
    root: {
      margin: '15vh',
      textAlign: 'center',
      color: '#605e5c',
    },
  };

export const Login = () => {

    const [userName, setUserName] = useUser();
    const [name, setName] = useState<string | undefined>(undefined);
    const history = useHistory();

    const saveName = () => {
        console.log(name);
        if(!name) {
            return;
        }
        setUserName(name);

        setTimeout(() => {
            document.location.reload();
        }, 100);
    };

    useEffect(() => {
        console.log('login useEffect', userName);
        if(!userName) {
            return;
        }
        history.push('/');
    }, [userName, history]);

    return (
        <Stack
            styles={stackStyles}
            tokens={stackTokens}>
                <TextField 
                    label="Username" 
                    onChange={(e, newValue) => setName(newValue)} />
                <PrimaryButton 
                    text="Login"
                    onClick={saveName} />
        </Stack>
    )
};