import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table"

import { formatDate } from "@/lib/formatting";
import { usePortfolioCorporateActionsQuery } from "../hooks/PortfolioQueries";

import { CorporateAction } from "@/model/Portfolio";


function CorporateActions() {

	const { isPending: isPending, isError: isError, data: corporateActions } = usePortfolioCorporateActionsQuery();

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	return (
		<Table className="w-full text-sm">
			<TableHeader>
				<TableRow>
					<TableHead>Date</TableHead>
					<TableHead>Stock</TableHead>
					<TableHead>Description</TableHead>
				</TableRow>
			</TableHeader>
			<TableBody>
				{corporateActions.map((action: CorporateAction) => (
					<TableRow key={action.id}>
						<TableCell className="py-1 text-nowrap">{formatDate(action.actionDate)}</TableCell>
						<TableCell className="py-1 text-nowrap">{action.stock.asxCode}</TableCell>
						<TableCell className="py-1 text-nowrap">{action.description}</TableCell>
					</TableRow>
				))}
			</TableBody>
		</Table>
	);
}

export default CorporateActions;
