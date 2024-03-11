import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table"

import { formatDate } from "@/lib/formatting";
import { useHoldingTransactionQuery } from "../hooks/PortfolioQueries";

import { Transaction } from "@/model/Portfolio";

interface TransactionsProps {
	stockId: string
}
function Transactions({ stockId }: TransactionsProps) {

	const { isPending: isPending, isError: isError, data: transactions } = useHoldingTransactionQuery(stockId);

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	transactions.sort((a, b) => b.transactionDate.getTime() - a.transactionDate.getTime());

	return (
		<Table className="w-full text-sm">
			<TableHeader>
				<TableRow>
					<TableHead>Date</TableHead>
					<TableHead>Description</TableHead>
					<TableHead>Comment</TableHead>
				</TableRow>
			</TableHeader>
			<TableBody>
				{transactions.map((transaction: Transaction) => (
					<TableRow key={transaction.id}>
						<TableCell className="py-1 text-nowrap">{formatDate(transaction.transactionDate)}</TableCell>
						<TableCell className="py-1 text-nowrap">{transaction.description}</TableCell>
						<TableCell className="py-1 text-nowrap">{transaction.comment}</TableCell>
					</TableRow>
				))} 
			</TableBody>
		</Table>
	);
}

export default Transactions;
