import { Link } from "react-router-dom";
import { useState } from "react";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Checkbox } from "@/components/ui/checkbox"

import { formatNumber, formatCurrency } from "@/lib/formatting";
import {PortfolioProperties, PortfolioSummary} from "@/model/Portfolio";


interface HoldingsProps {
	portfolioProperties: PortfolioProperties,
	portfolioSummary: PortfolioSummary
}

interface HoldingItem {
	id: string,
	name: string,
	units: string,
	amount: string,
	inActive: boolean
}

function Holdings({ portfolioProperties, portfolioSummary }: HoldingsProps) {
	
	const [showSold, setShowSold] = useState(false);

	const holdings: HoldingItem[] = portfolioSummary.holdings.map<HoldingItem>((holding) => ({
		id: holding.stock.id,
		name: holding.stock.name,
		units: formatNumber(holding.units),
		amount: formatCurrency(holding.value),
		inActive: false
	}));

	if (showSold) {
		const soldHoldings = portfolioProperties.holdings.filter((holding) => holding.endDate)
			.map<HoldingItem>((holding) => ({
				id: holding.stock.id,
				name: holding.stock.name,
				units: "-",
				amount: "-",
				inActive: true
			}));
		holdings.push(...soldHoldings);
	}

	holdings.sort((a, b) => (a.name == b.name) ? 0 : ((a.name > b.name) ? 1: -1) );

	
	return(
		<div className="flex flex-col">
			<div className="pt-3 px-10 place-self-end">
				<Checkbox id="showSold" checked={showSold} onClick={() => setShowSold(!showSold)} className="border-foreground rounded-lg"/>
				<label htmlFor="showSold" className="px-2">Include Sold Holdings</label>
			</div>
			<Table className="w-full text-sm">
				<TableHeader>
					<TableRow>
						<TableHead>Holding</TableHead>
						<TableHead className="text-right">Units</TableHead>
						<TableHead className="text-right">Amount</TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{holdings.map((holding) => (
						<TableRow key={holding.id}>
							<TableCell className="hover:text-primary py-1 text-nowrap">
								<Link to={"holding/" + holding.id} className={holding.inActive ? 'line-through': ''}>{holding.name}</Link>
							</TableCell>
							<TableCell className="text-right py-1">{holding.units}</TableCell>
							<TableCell className="text-right py-1">{holding.amount}</TableCell>
						</TableRow>
					))}
					<TableRow key="cash">
						<TableCell className="hover:text-primary py-1">
							<Link to="cashaccount">Cash</Link>
						</TableCell>
						<TableCell className="text-right py-1"></TableCell>
						<TableCell className="text-right py-1">{formatCurrency(portfolioSummary.cashBalance)}</TableCell>
					</TableRow>
				</TableBody>
			</Table>
		</div>
	);
}

export default Holdings;
