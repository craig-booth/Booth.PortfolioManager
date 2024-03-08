
export function formatNumber(value: number): string {
	return value.toLocaleString(undefined);
}

export function formatCurrency(value: number, digits?: number): string {
	if (digits)
		return value.toLocaleString(undefined, { style: "currency", currency: "AUD", currencyDisplay: "narrowSymbol", minimumFractionDigits: digits });
	else
		return value.toLocaleString(undefined, { style: "currency", currency: "AUD", currencyDisplay: "narrowSymbol" });
}

export function formatPercentage(value: number, digits: number = 2): string {
	return value.toLocaleString(undefined, { style: "percent", minimumFractionDigits: digits });
}
