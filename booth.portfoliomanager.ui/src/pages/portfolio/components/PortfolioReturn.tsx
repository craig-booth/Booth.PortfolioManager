import { PortfolioSummary } from "@/model/Portfolio";

import { formatPercentage } from "@/lib/formatting";

interface PortfolioReturnProps {
	portfolio: PortfolioSummary
}

function PortfolioReturn({portfolio} : PortfolioReturnProps) {
	return(
		<div>
			<div className="grid grid-cols-3 grid-rows-2 place-items-center">
				<div className="col-span-3 py-3">
					<div className="text-3xl font-bold">{portfolio.returnAll ? formatPercentage(portfolio.returnAll): "-"}</div>
				</div>
				<div>
					<div className="text-muted-foreground text-sm">1 year</div>
					<div>{portfolio.return1Year ? formatPercentage(portfolio.return1Year): "-"}</div>			
				</div>
				<div>
					<div className="text-muted-foreground text-sm">3 years</div>
					<div>{portfolio.return3Year ? formatPercentage(portfolio.return3Year): "-"}</div>
				</div>
				<div>
					<div className="text-muted-foreground text-sm">5 years</div>
					<div>{portfolio.return5Year ? formatPercentage(portfolio.return5Year): "-"}</div>
				</div>
			</div>	
		</div>
	);
}

export default PortfolioReturn;
