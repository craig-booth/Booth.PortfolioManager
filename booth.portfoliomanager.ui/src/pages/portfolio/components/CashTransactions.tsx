import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table"

import { formatDate, formatCurrency } from "@/lib/formatting";
import { CashAccount, CashTransaction } from "@/model/Portfolio";

interface CashTransactionsProps {
	cashAccount: CashAccount
}

function CashTransactions({ cashAccount }: CashTransactionsProps) {

	return (
		<Table className="w-full text-sm">
			<TableHeader>
				<TableRow>
					<TableHead className="min-w-20">Date</TableHead>
					<TableHead>Description</TableHead>
					<TableHead className="min-w-20 text-right">Amount</TableHead>
					<TableHead className="min-w-20 text-right">Balance</TableHead>
				</TableRow>
			</TableHeader>
			<TableBody>
				{ cashAccount.transactions.map((transaction: CashTransaction) => (
					<TableRow>
						<TableCell className="py-1">{formatDate(transaction.date)}</TableCell>
						<TableCell className="py-1">{transaction.description}</TableCell>
						<TableCell className="py-1 text-right">{formatCurrency(transaction.amount)}</TableCell>
						<TableCell className="py-1 text-right">{formatCurrency(transaction.balance)}</TableCell>
					</TableRow>
				))} 
			</TableBody>
		</Table>
	);
}

export default CashTransactions;
