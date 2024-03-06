import { useSession } from "@/lib/Session";
import { AuthenticationRequest, AuthenticationResponse } from "@/model/Authentication";
import { PortfolioSummary, PortfolioProperties } from "@/model/Portfolio";


export interface UseApiClientResult {
    signIn(username: string, password: string): Promise<AuthenticationResponse>;
    signOut(): Promise<void>;
    getPortfolioProperties(): Promise<PortfolioProperties>;
    getPortfolioSummary(): Promise<PortfolioSummary>;
}

export const useApiClient = (): UseApiClientResult => {

    const { session, setSession } = useSession();

    const signIn = async (username: string, password: string): Promise<AuthenticationResponse> => {

        const authRequest: AuthenticationRequest = {
            userName: username,
            password: password
        };

        const authResponse = fetch("/api/authenticate", 
        { 
            method: "POST",
            headers: {  "Content-Type": "application/json", "Accept" : "application/json" },
            body: JSON.stringify(authRequest)
        })
        .then(response => {
            if (!response.ok)
                throw new Error(response.statusText)
            return response.json() as Promise<AuthenticationResponse>
        })
        .then(authResponse => {
            setSession({
                username: username,
                token: authResponse.token,
                portfolioId: "5D5DE669-726C-4C5D-BB2E-6520C924DB90"   
                });
        
            return authResponse;
        });

        return new Promise<AuthenticationResponse>(() => authResponse);
    }

    const signOut = async() => {
        setSession(null);
    }

    const getPortfolioProperties = async (): Promise<PortfolioProperties> => {

        const properties = fetch("/api/portfolio/" + session?.portfolioId + "/properties",
            {
                method: "GET",
                headers: {
                    "Authorization": "Bearer " + session?.token,
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                }
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => JSON.parse(response, customReviver))

        return properties;
    }
 

    const getPortfolioSummary = async (): Promise<PortfolioSummary> => {

        const summary = fetch("/api/portfolio/" + session?.portfolioId + "/summary",
            {
                method: "GET",
                headers: {
                    "Authorization": "Bearer " + session?.token,
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                }
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => JSON.parse(response, customReviver))

        return summary;
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    function customReviver(key: string, value: any): any {

        if ((key == "startDate") || (key == "endDate")) {
            if ((value != undefined) && (value != null)) {

                if (value == "9999-12-31")
                    return undefined;
                else {
                    const date = new Date(value);
                    return date;
                }

            }
        }

        return value;
    }

    return { signIn, signOut, getPortfolioProperties, getPortfolioSummary };
}