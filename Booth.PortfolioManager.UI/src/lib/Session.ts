import { useContext } from "react";
import { SessionContext } from "@/lib/SessionProvider";

export interface Session {
    username?: string;
    token?: string;
    portfolioId?: string;
}

export interface UseSessionResult {
    session: Session | null;
    setSession(session: Session | null): void;
    isAuthenticated: boolean;
}

export const useSession = (): UseSessionResult => {

    const { session, setSession } = useContext(SessionContext);

    const isAuthenticated  = session ? (session.token != null): false;

    const updateSession = (session: Session | null) => {
        setSession(session!);
    }

    return { session, setSession: updateSession, isAuthenticated };

}