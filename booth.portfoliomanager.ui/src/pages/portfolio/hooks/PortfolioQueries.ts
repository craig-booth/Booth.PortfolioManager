import { useQuery, UseQueryResult } from "@tanstack/react-query";

import { PortfolioSummary, PortfolioProperties, CorporateAction, Holding, Transaction } from "@/model/Portfolio";
import { useApiClient } from "@/lib/ApiClient";

export const usePortfolioSummaryQuery = (): UseQueryResult<PortfolioSummary> => {
	
	const { getPortfolioSummary } = useApiClient();

	return useQuery({
		queryKey: ["portfolioSummary"],
		queryFn: () => getPortfolioSummary()
	});
};

export const usePortfolioPropertiesQuery = (): UseQueryResult<PortfolioProperties> => {

	const { getPortfolioProperties } = useApiClient();

	return useQuery({
		queryKey: ["portfolioProperties"],
		queryFn: () => getPortfolioProperties()
	});
};

export const usePortfolioCorporateActionsQuery = (): UseQueryResult<CorporateAction[]> => {

	const { getCorporateActions } = useApiClient();

	return useQuery({
		queryKey: ["portfolioCorporateActions"],
		queryFn: () => getCorporateActions()
	});
};

export const useHoldingQuery = (stockId: string): UseQueryResult<Holding> => {

	const { getHolding } = useApiClient();

	return useQuery({
		queryKey: ["holding", stockId],
		queryFn: () => getHolding(stockId)
	});
};

export const useHoldingTransactionQuery = (stockId: string): UseQueryResult<Transaction[]> => {

	const { getTransactions } = useApiClient();

	return useQuery({
		queryKey: ["holdingTransactions", stockId],
		queryFn: () => getTransactions(stockId)
	});
};