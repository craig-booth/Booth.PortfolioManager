import {
    useParams
} from "react-router-dom";

import {
  Card,
  CardHeader
} from "@/components/ui/card"

import { useHoldingQuery } from "./hooks/PortfolioQueries";

import Transactions from "./components/Transactions";
import HoldingValue from "./components/HoldingValue";

function Holding() {

	const { holdingId = "" } = useParams();

	const { isPending: isPending, isError: isError, data: holding } = useHoldingQuery(holdingId);

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

    return (
		<div className="grid grid-flow-row-dense grid-cols-3 gap-8">
			<Card className="bg-muted rounded-lg col-span-2 p-2">
				<CardHeader className="text-sm">Holding</CardHeader>
				<HoldingValue holding={holding} />
			</Card>
			<Card className="bg-muted rounded-lg col-span-1 p-2">
				<CardHeader className="text-sm">Return</CardHeader>
			</Card>
			<Card className="bg-muted rounded-lg col-span-3 p-2">
				<CardHeader className="text-sm">Transactions</CardHeader>
				<Transactions stockId={ holding.stock.id } />
			</Card>		
		</div>
    );
}

export default Holding;