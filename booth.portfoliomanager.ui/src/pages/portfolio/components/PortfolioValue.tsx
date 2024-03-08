import { PortfolioSummary } from "@/model/Portfolio";
import { formatCurrency } from "@/lib/formatting";
interface PortfolioReturnProps {
	portfolio: PortfolioSummary
}

function PortfolioReturn({portfolio} : PortfolioReturnProps) {
	return(
    <div className="p-8 content-center justify-center text-center align-middle">
      <div className="text-5xl font-bold">{formatCurrency(portfolio.portfolioValue)}</div>
    </div>	
	);
}

export default PortfolioReturn;
