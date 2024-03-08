import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom"; 

import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"

import { useApiClient } from "@/lib/ApiClient";

function Logon() {

    const [userName, setUserName] = useState("");
    const [password, setPassword] = useState("");

    const navigate = useNavigate();

    const location = useLocation();
    const params = new URLSearchParams(location.search);
    const from = params.get("from") || "/";

    const { signIn } = useApiClient();

    const logon = async(userName: string, password: string) => {
        await signIn(userName, password);   
        return navigate(from || "/");
    }

    return (
        <div id="logon-background" className="container min-h-screen min-w-full flex flex-row items-center justify-center">
			<div className="rounded-lg backdrop-blur-sm bg-white/30 p-8">
				<div className="mx-auto flex w-full flex-col justify-center space-y-6 sm:w-[350px]">
					<div className="text-2xl font-bold text-center">Sign in</div>
					<div className="grid gap-2">
						<Input name="username" placeholder="User name*" value={userName} onChange={e => setUserName(e.target.value)} />
						<Input name="password" type="password" placeholder="Password*" value={password} onChange={e => setPassword(e.target.value)} />
						<Button onClick={() => logon(userName, password)}>Sign in</Button>
					</div>
				</div>
			</div>
		</div>
    );
}

export default Logon;