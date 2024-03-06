import {
    createBrowserRouter,
    LoaderFunctionArgs,
    RouterProvider,
    Navigate,
	redirect
} from "react-router-dom";
import {
	QueryClient,
	QueryClientProvider
} from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";

import './App.css'
import { ThemeProvider } from "@/components/theme-provider";
import { useSession } from "@/lib/Session";
import { useApiClient } from "./lib/ApiClient";

import Layout from "@/components/Layout.tsx";
import Logon from "@/pages/logon/Logon.tsx";
import PortfolioRoutes from "@/pages/portfolio/PortfolioRoutes.tsx";
import ReportRoutes from "@/pages/reports/ReportRoutes.tsx";


function App() {

    const queryClient = new QueryClient();

    const { isAuthenticated } = useSession();
    const { signOut } = useApiClient();

    const router = createBrowserRouter([
        {
            path: "/",
            element: <Layout />,
            loader: protectedLoader,
            children: [
                {
                    path: "/",
                    element: <Navigate to="portfolio" replace={true} />
                },
                PortfolioRoutes,
                ReportRoutes,
            ]
        },
        {
            path: "/logon",
            loader: logonLoader,
            element: <Logon />
        },
        {
            path: "/logout",
            loader: logoutLoader,
            element: <Navigate to="/logon" replace={true} />
        }
    
    ]);
    
    function logonLoader() {
        if (isAuthenticated) {
            return redirect("/");
        }
        else
            return null;
    }
    
    function logoutLoader() {
        if (isAuthenticated) {
            signOut(); 
        }
        
        return null;
    }
    
    function protectedLoader({ request }: LoaderFunctionArgs) {
        if (!isAuthenticated) {
            const params = new URLSearchParams();
            params.set("from", new URL(request.url).pathname);
            return redirect("/logon?" + params.toString());
        }
        else 
            return null;
    }
    


    return (
        <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
                <ReactQueryDevtools initialIsOpen />
            </QueryClientProvider>
        </ThemeProvider>	
    )
}

export default App;
