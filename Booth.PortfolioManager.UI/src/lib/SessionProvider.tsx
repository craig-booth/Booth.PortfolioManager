import { ReactNode, createContext, useState } from "react";
import { Session } from "@/lib/Session";

interface SessionContextValue {
    session: Session | null;
    setSession(session: Session | null): void; 
}

const defaultSessionValue: SessionContextValue = {
    session: null, 
    setSession: () => {}
}

export const SessionContext = createContext<SessionContextValue>(defaultSessionValue);

const useSessionProvider = () => {

    const [ session, setSession ] = useState<Session>({});

    const updateSession = (session: Session | null) => {
        setSession(session!);
    }

    return { session, setSession: updateSession };

}
 
export function SessionProvider({ children }: { children: ReactNode; }): JSX.Element {

    const { session, setSession } = useSessionProvider();


    return (
        <SessionContext.Provider value={{session, setSession}}>
          {children}
        </SessionContext.Provider>
    );
}
