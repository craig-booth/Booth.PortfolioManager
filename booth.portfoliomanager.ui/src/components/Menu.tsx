import { Link } from "react-router-dom";


export default function Menu() {

    return (
		<header className="w-full border-b ">
			<div className="container flex h-14 ">
				<nav className="flex items-center gap-6 text-sm">
					<Link to={`portfolio`} className="transition-colors hover:text-primary text-foreground/60">Portfolio</Link>
					<Link to={`reports`} className="transition-colors hover:text-primary text-foreground/60">Reports</Link>
					<Link to={`logout`} className="transition-colors hover:text-primary text-foreground/60">Logout</Link>			
				</nav>
			</div>
		</header>
    );
}