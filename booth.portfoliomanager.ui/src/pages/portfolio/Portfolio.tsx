import {
  Card,
  CardContent,
  CardHeader,
} from "@/components/ui/card"

import { usePortfolioPropertiesQuery, usePortfolioSummaryQuery } from "./hooks/PortfolioQueries.ts";

import PortfolioValue from "./components/PortfolioValue.tsx";
import PortfolioReturn from "./components/PortfolioReturn.tsx";
import Holdings from "./components/Holdings.tsx";
import AssetAllocation from "./components/AssetAllocation.tsx";
import CorporateActions from "./components/CorporateActions.tsx";

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
		<div className="space-y-8">
			<div className="grid grid-cols-1 md:grid-cols-2 gap-8">
				<Card className="bg-muted rounded-lg p-2">
					<CardHeader className="text-sm">Value</CardHeader>
					<CardContent>
						<PortfolioValue portfolio={portfolioSummary} />
					</CardContent>
				</Card>
				<Card className="bg-muted rounded-lg p-2">
					<CardHeader className="text-sm">Return</CardHeader>
					<CardContent>
						<PortfolioReturn portfolio={portfolioSummary} />
					</CardContent>
				</Card>	
			</div>
			<div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-8">
				<Card className="bg-muted rounded-lg col-span-1 md:col-span-2">
					<CardHeader className="text-sm">Holdings</CardHeader>
					<CardContent>
						<Holdings portfolioProperties={portfolioProperties} portfolioSummary={portfolioSummary} />
					</CardContent>	
				</Card>
				<Card className="bg-muted rounded-lg col-span-1 p-2">
					<CardHeader className="text-sm">Corporate Actions</CardHeader>
					<CardContent>
						<CorporateActions />
					</CardContent>	
				</Card>	
				<Card className="bg-muted rounded-lg col-span-1 p-2">
					<CardHeader className="text-sm">Suggested Trades</CardHeader>
					<CardContent>
					</CardContent>
				</Card>
			</div>
			<div className="grid grid-cols-4 gap-8">
				<Card className="bg-muted rounded-lg col-span-4 p-2">
					<CardHeader className="text-sm">Asset Allocation</CardHeader>
					<CardContent>
						<AssetAllocation portfolio={portfolioSummary} />
					</CardContent>	
				</Card>
			</div>
		</div>
    );
}


export default Portfolio;