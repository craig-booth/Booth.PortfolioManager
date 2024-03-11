import { useQuery, UseQueryResult } from "@tanstack/react-query";

import { PortfolioSummary, PortfolioProperties, CorporateAction } from "@/model/Portfolio";
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