export interface PortfolioProperties {
	id: string;
	name: string;
	stateDate: Date;
	endDate?: Date;
	holdings: HoldingProperties[];
}
export interface HoldingProperties {
	stock: Stock;
	stateDate: Date;
	endDate?: Date;
	participatingInDrp: boolean;
}

export interface PortfolioSummary {
	portfolioValue: number;
	portfolioCost: number;
	return1Year?: number;
	return3Year?: number;
	return5Year?: number;
	returnAll?: number;
	cashBalance: number;
	holdings: Holding[];
}

export interface Holding {
	stock: Stock;
	units: number;
	value: number;
	cost: number;
	costBase: number;
}

export interface Stock {
	id: string;
	asxCode: string;
	name: string;
	category: AssetCategory;
}

export interface CorporateAction {
	id: string,
    actionDate: Date,
    stock: Stock,
    description: string
}

export interface Transaction {
	id: string,
	stock: Stock,
	transactionDate: Date,
	description: string,
	comment: string
}

export interface CashAccount {
	openingBalance: number,
	closingBalance: number,
	transactions: CashTransaction[]
}

export interface CashTransaction {
	date: Date,
	type: CashTransactionType,
	description: string,
	amount: number,
	balance: number
}


export enum AssetCategory {
	AustralianStocks = "australianStocks",
	InternationalStocks = "internationalStocks",
	AustralianProperty = "australianProperty",
	InternationalProperty = "internationalProperty",
	AustralianFixedInterest = "australianFixedInterest",
	InternationalFixedInterest = "internationlFixedInterest",
	Cash = "cash"
}

export function isGrowthCategory(category: AssetCategory) {
	return ((category == AssetCategory.AustralianStocks) ||
		(category == AssetCategory.InternationalStocks) ||
		(category == AssetCategory.AustralianProperty) ||
		(category == AssetCategory.InternationalProperty));
}

export function isIncomeCategory(category: AssetCategory) {
	return ((category == AssetCategory.AustralianFixedInterest) ||
		(category == AssetCategory.InternationalFixedInterest));
}

export enum CashTransactionType {
	Deposit = "deposit",
	Withdrawl = "withdrawl",
	Transfer = "transfer",
	Fee = "fee",
	Interest = "interest"
}
