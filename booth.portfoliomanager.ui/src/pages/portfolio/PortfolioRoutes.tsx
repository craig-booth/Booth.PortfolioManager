import Portfolio from "./Portfolio.tsx";
import Holding from "./Holding.tsx";
import CashAccount from "./CashAccount.tsx";

const PortfolioRoutes = {
	path: "portfolio",
	children: [
		{
			index: true,
			element: <Portfolio />,
		},
		{
			path: "holding/:holdingId",
			element: <Holding />,
		},
		{
			path: "cashaccount",
			element: <CashAccount />,
		}
	]
};

export default PortfolioRoutes;
