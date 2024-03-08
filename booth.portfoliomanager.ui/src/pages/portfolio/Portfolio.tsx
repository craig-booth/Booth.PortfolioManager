import {
  Card,
  CardHeader,
} from "@/components/ui/card"

import { usePortfolioPropertiesQuery, usePortfolioSummaryQuery } from "./hooks/PortfolioQueries.ts";

import PortfolioValue from "./components/PortfolioValue.tsx";
import PortfolioReturn from "./components/PortfolioReturn.tsx";
import Holdings from "./components/Holdings.tsx";
import AssetAllocation from "./components/AssetAllocation.tsx";

function Portfolio() {

	const { isPending: isPropertiesPending, isError: isPropertiesError, data: portfolioProperties } = usePortfolioPropertiesQuery();
	const { isPending: isSummaryPending, isError: isSummaryError, data: portfolioSummary } = usePortfolioSummaryQuery();

	if (isPropertiesPending || isSummaryPending) {
		return <div>Loading...</div>;
	}

	if (isPropertiesError || isSummaryError) {
		return <div>Error</div>;
	}

    return (
	<div className="grid grid-flow-row-dense grid-cols-3 gap-8">
		<Card className="bg-muted rounded-lg col-span-2 p-2">
			<CardHeader className="text-sm">Value</CardHeader>
			<PortfolioValue portfolio={portfolioSummary}/>
		</Card>
		<Card className="bg-muted rounded-lg col-span-1 p-2">
			<CardHeader className="text-sm">Return</CardHeader>
			<PortfolioReturn portfolio={portfolioSummary}/>
		</Card>		
		<Card className="bg-muted rounded-lg col-span-2">
			<CardHeader className="text-sm">Holdings</CardHeader>
			<Holdings portfolioProperties={portfolioProperties} portfolioSummary={portfolioSummary} />
		</Card>
		<Card className="bg-muted rounded-lg col-span-1 p-2">
			<CardHeader className="text-sm">Corporate Actions</CardHeader>
		</Card>			
		<Card className="bg-muted rounded-lg col-span-3 p-2">
			<CardHeader className="text-sm">Asset Allocation</CardHeader>
			<AssetAllocation portfolio={portfolioSummary}/>
		</Card>
		<Card className="bg-muted rounded-lg col-span-1 p-2">
			<CardHeader className="text-sm">Suggested Trades</CardHeader>
		</Card>		
	</div>
    );
}


export default Portfolio;