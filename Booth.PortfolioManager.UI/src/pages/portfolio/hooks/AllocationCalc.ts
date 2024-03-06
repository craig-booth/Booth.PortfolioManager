import {PortfolioSummary, Holding} from "@/model/Portfolio";

export interface AlloctionCalcParam {
	name: string;
	target: number;
	value?: number;
	selector?: ((holding: Holding) => boolean);
}

export interface Allocation {
	name: string;
	value: number;
	percentage: number;
	targetPercentage: number;
	differenceAmount: number;
	differencePercentage: number;	
}

export const allocationCalc = (portfolio: PortfolioSummary, params: AlloctionCalcParam[]): Allocation[] => {
	
	
	const allocation: Allocation[] = [];
	
	params.forEach((param) => {
		
		let value = 0;
		
		if (param.selector !== undefined)
			value = portfolio.holdings.reduce((accumulator: number, holding: Holding) => accumulator + (param.selector!(holding) ? holding.value : 0), 0);
		else
			value = param.value ? param.value: 0;
			
		allocation.push({
			name: param.name,
			value: value,
			percentage: (value / portfolio.portfolioValue),
			targetPercentage: param.target,
			differenceAmount: (value - (portfolio.portfolioValue * param.target)),
			differencePercentage: ((value / portfolio.portfolioValue) - param.target)	
		})
	});

	return allocation;
};