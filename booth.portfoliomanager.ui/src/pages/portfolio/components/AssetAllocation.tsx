import { useState } from "react";

import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
  } from "@/components/ui/table"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label"
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, TooltipProps } from 'recharts';
import { ValueType, NameType } from 'recharts/types/component/DefaultTooltipContent';

import { PortfolioSummary, AssetCategory, isGrowthCategory, isIncomeCategory, Holding, Stock } from "@/model/Portfolio";
import { AlloctionCalcParam, allocationCalc, Allocation } from "../hooks/AllocationCalc.ts";
import { generateColors, ColorRangeInfo } from "@/lib/GenerateColors.ts";
import { formatPercentage, formatCurrency } from "@/lib/formatting.ts";

import * as d3 from 'd3';

interface AssetAllocationProps {
	portfolio: PortfolioSummary
}
 
function stockTarget(stock: Stock): number {

	if (stock.asxCode == "ARG") return 0.25;
	else if (stock.asxCode == "COH") return 0.025;
	else if (stock.asxCode == "VAP") return 0.05;
	else if (stock.asxCode == "VGE") return 0.05;
	else if (stock.asxCode == "VGS") return 0.46;
	else if (stock.asxCode == "VISM") return 0.065;
	else if (stock.asxCode == "VCF") return 0.05;
	else if (stock.asxCode == "VAF") return 0.05;
	else return 0;

}

const CustomTooltip = ({active, payload }: TooltipProps<ValueType, NameType>)  => {
	if (active && payload?.length) {

		const allocation: Allocation = payload[0].payload;

		return (
			<div className="bg-muted z-50 rounded-lg p-2">
				<span className="text-sm">{allocation.name + "  " + formatPercentage(allocation.percentage, 2)}</span>
			</div>
		);
	}
	else
		return null;
}

function AssetAllocation({portfolio} : AssetAllocationProps) {
	const [chartType, setChartType] = useState("holding");

	let calcParams: AlloctionCalcParam[] = [];
	switch (chartType) {
		case "type": 
			calcParams = [
				{ name: "Growth", target: 0.90, selector: (holding: Holding) => isGrowthCategory(holding.stock.category) },
				{ name: "Income", target: 0.10, selector: (holding: Holding) => isIncomeCategory(holding.stock.category) },
				{ name: "Cash", target: 0.00, value: portfolio.cashBalance }
			];	
			break;
		case "category": 			
			calcParams = [
				{ name: "Australian Stocks", target: 0.30, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianStocks },
				{ name: "International Stocks", target: 0.55, selector: (holding: Holding) => holding.stock.category == AssetCategory.InternationalStocks },
				{ name: "Australian Property", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianProperty },
				{ name: "Australian Fixed Interest", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianFixedInterest },
				{ name: "International Fixed Interest", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.InternationalFixedInterest },
				{ name: "Cash", target: 0.00, value: portfolio.cashBalance }
			];
			break;
		case "holding": 	
			calcParams = portfolio.holdings.map((holding) =>({
				name: holding.stock.name,
				target: stockTarget(holding.stock),
				value: holding.value
			}));
			calcParams.push({ name: "Cash", target: 0.00, value: portfolio.cashBalance });
			break;		
	}
	const allocation = allocationCalc(portfolio, calcParams);
	allocation.sort((a, b) => b.percentage - a.percentage);

	const colorScale = d3.interpolateBlues;
	const colorRangeInfo: ColorRangeInfo = {
		colorStart: 0.4,
		colorEnd: 0.95,
		useEndAsStart: false,
	};
	const colors = generateColors(allocation.length, colorScale, colorRangeInfo).reverse();
	
	return(
		<div className="flex flex-col">
			<div className="px-10 pb-4">
				<RadioGroup value={chartType} onValueChange={(value) => setChartType(value)} className="flex flex-row-reverse">
					<div>
						<RadioGroupItem value="type" id="type" className="peer sr-only" />
						<Label htmlFor="type" className="rounded-lg p-2 peer-data-[state=checked]:bg-primary peer-data-[state=unchecked]:cursor-pointer peer-data-[state=unchecked]:hover:text-primary">Type</Label>
					</div>	
					<div>
						<RadioGroupItem value="category" id="category" className="peer sr-only" />
						<Label htmlFor="category" className="rounded-lg p-2 peer-data-[state=checked]:bg-primary peer-data-[state=unchecked]:cursor-pointer peer-data-[state=unchecked]:hover:text-primary">Category</Label>
					</div>
					<div>
						<RadioGroupItem value="holding" id="holding" className="peer sr-only" />
						<Label htmlFor="holding" className="rounded-lg p-2 peer-data-[state=checked]:bg-primary peer-data-[state=unchecked]:cursor-pointer peer-data-[state=unchecked]:hover:text-primary">Holding</Label>
					</div>
				</RadioGroup>
			</div>			
			<div className="grid grid-cols-1 xl:grid-cols-3">
				<div className="col-span-1 content-center min-h-72" >
					<ResponsiveContainer >
						<PieChart>
							<Pie data={allocation} stroke="none" dataKey="percentage" nameKey="name" startAngle={90} endAngle={450} outerRadius="100%">
								{
									colors.map((color) => (
										<Cell fill={color} />
									))
								}		
							</Pie>
							<Tooltip content={<CustomTooltip active={false} payload={[]} /> }></Tooltip>
						</PieChart> 
					</ResponsiveContainer>
				</div>
				<div className="col-span-2 p-2">
					<Table className="w-full text-sm">
						<TableHeader>
							<TableRow>
								<TableHead className="">Asset</TableHead>
								<TableHead className="min-w-20 text-right">%</TableHead>
								<TableHead className="min-w-20 text-right">Target</TableHead>
								<TableHead className="min-w-20 text-right">Diff $</TableHead>
								<TableHead className="min-w-20 text-right">Diff %</TableHead>
							</TableRow>
						</TableHeader>
						<TableBody>
							{allocation.map((asset, index) => (
								<TableRow key={asset.name} className="h-2">
									<TableCell className="py-1">
										<div className="inline-flex">
											<div className="w-8 h-6 rounded-xl mr-3" style={{backgroundColor: colors[index]}}>&nbsp;</div>
											<div className="text-nowrap">{asset.name}</div>
										</div>
									</TableCell>
									<TableCell className="py-1 text-right">{formatPercentage(asset.percentage)}</TableCell>
									<TableCell className="py-1 text-right">{formatPercentage(asset.targetPercentage)}</TableCell>
									<TableCell className="py-1 text-right">{formatCurrency(asset.differenceAmount)}</TableCell>
									<TableCell className="py-1 text-right">{formatPercentage(asset.differencePercentage)}</TableCell>
								</TableRow>
							))}
						</TableBody>
					</Table>
				</div>
			</div>
						
		</div>	
	);
}

export default AssetAllocation;
