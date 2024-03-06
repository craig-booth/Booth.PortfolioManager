import { PortfolioSummary } from "@/model/Portfolio";

interface PortfolioReturnProps {
	portfolio: PortfolioSummary
}

function PortfolioReturn({portfolio} : PortfolioReturnProps) {
	return(
		<div>
			<div className="grid grid-cols-3 grid-rows-2 place-items-center">
				<div className="col-span-3 py-3">
					<div className="text-3xl font-bold">{portfolio.returnAll? portfolio.returnAll.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2}): "-"}</div>
				</div>
				<div>
					<div className="text-muted-foreground text-sm">1 year</div>
					<div>{portfolio.return1Year? portfolio.return1Year.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2}): "-"}</div>			
				</div>
				<div>
					<div className="text-muted-foreground text-sm">3 years</div>
					<div>{portfolio.return3Year? portfolio.return3Year.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2}): "-"}</div>
				</div>
				<div>
					<div className="text-muted-foreground text-sm">5 years</div>
					<div>{portfolio.return5Year? portfolio.return5Year.toLocaleString(undefined, {style: "percent", minimumFractionDigits: 2}): "-"}</div>
				</div>
			</div>	
		</div>
	);
}

export default PortfolioReturn;
