import {
  Card,
  CardHeader,
} from "@/components/ui/card"

import { useCashAccountQuery } from "./hooks/PortfolioQueries";

import CashTransactions from "./components/CashTransactions";
import CashBalance from "./components/CashBalance";
function CashAccount() {

	const { isPending: isPending, isError: isError, data: cashAccount } = useCashAccountQuery();

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	} 

	return (

		<div className="grid grid-flow-row-dense grid-cols-3 gap-8">
			<Card className="bg-muted rounded-lg col-span-3 p-2">
				<CardHeader className="text-sm">Balance</CardHeader>
				<CashBalance cashAccount={cashAccount} />
			</Card>
			<Card className="bg-muted rounded-lg col-span-3 p-2">
				<CardHeader className="text-sm">Transactions</CardHeader>
				<CashTransactions cashAccount={cashAccount} />
			</Card>		
		</div>
    );
}

export default CashAccount;