import { useState, useEffect } from 'react';

const key = 'chatierUserName';

export const useUser = () => {
    const [userName, setUserName] = useState<string | undefined>();

    useEffect(() => {
        try { 
            const item = window.localStorage.getItem(key); 

            if (!item) {
                return;
            }

            setUserName(item);
        } 
        catch (error) { 
            console.error(error);
        }
    }, []);

    useEffect(() => {
        try{
            if(!userName) {
                return;
            }
            window.localStorage.setItem(key, userName);
        }
        catch(error) {
            console.error(error);
        }
    }, [userName]);

    return [userName, setUserName] as const;
};