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
import { Chart as ChartJS, ArcElement, PieController ,Tooltip, Legend } from 'chart.js';
import { Pie } from 'react-chartjs-2';

import { PortfolioSummary, AssetCategory, isGrowthCategory, isIncomeCategory, Holding } from "@/model/Portfolio";
import { AlloctionCalcParam, allocationCalc } from "../hooks/AllocationCalc.ts";
import { generateColors, ColorRangeInfo} from "@/lib/GenerateColors.ts";
import * as d3 from 'd3';

ChartJS.register(ArcElement, PieController, Tooltip, Legend);


interface AssetAllocationProps {
	portfolio: PortfolioSummary
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
				{ name: "Australian Stocks", target: 0.35, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianStocks },
				{ name: "International Stocks", target: 0.50, selector: (holding: Holding) => holding.stock.category == AssetCategory.InternationalStocks },
				{ name: "Australian Property", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianProperty },
				{ name: "Australian Fixed Interest", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.AustralianFixedInterest },
				{ name: "Internationl Fixed Interest", target: 0.05, selector: (holding: Holding) => holding.stock.category == AssetCategory.InternationlFixedInterest },
				{ name: "Cash", target: 0.00, value: portfolio.cashBalance }
			];
			break;
		case "holding": 	
			calcParams = portfolio.holdings.map((holding) =>({
				name: holding.stock.name,
				target: 0.10,
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
		colorEnd: 1,
		useEndAsStart: false,
	};
	const colors = generateColors(allocation.length, colorScale, colorRangeInfo);

	const chartData = {
		labels: allocation.map((allocation) => allocation.name),
		datasets: [ {
			data: allocation.map((allocation) => allocation.percentage),
			backgroundColor: colors,
			borderWidth: 0
		}],
	};

	const chartOptions = {
		plugins: {
			legend: {
				display: false,
			},
		}
	}; 

	let colorIndex = 0;
	
	return(
		<div className="flex flex-col">
			<div className="px-10">
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
			<div className="flex flex-row">
				<div>
					<Pie data={chartData} options={chartOptions} />
				</div>
				<div className="grow place-self-center">
					<Table className="w-full text-sm">
						<TableHeader>
							<TableRow>
								<TableHead >Asset</TableHead>
								<TableHead className="text-right">%</TableHead>
								<TableHead className="text-right">Target</TableHead>
								<TableHead className="text-right">Diff $</TableHead>
								<TableHead className="text-right">Diff %</TableHead>
							</TableRow>
						</TableHeader>
						<TableBody>
							{allocation.map((asset) => (
								<TableRow key={asset.name} className="h-2">
									<TableCell className="py-1">
										<div className="inline-flex">
											<div className="w-8 h-6 rounded-xl mr-3" style={{backgroundColor: colors[colorIndex++]}}>&nbsp;</div>
											<div className="text-nowrap">{asset.name}</div>
										</div>
									</TableCell>
									<TableCell className="py-1 text-right">{asset.percentage.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2})}</TableCell>
									<TableCell className="py-1 text-right">{asset.targetPercentage.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2})}</TableCell>
									<TableCell className="py-1 text-right">{asset.differenceAmount.toLocaleString(undefined, {style: "currency", currency: "AUD"})}</TableCell>
									<TableCell className="py-1 text-right">{asset.differencePercentage.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2})}</TableCell>
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
