import {
    Outlet
} from "react-router-dom";

import Menu from "./Menu.tsx";

export default function Layout() {

    return (
        <>
            <div>
				<Menu/>
				<div className="p-6">
					<Outlet />
				</div>
            </div>
        </>
    );
}