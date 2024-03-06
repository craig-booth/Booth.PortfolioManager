import { PortfolioSummary } from "@/model/Portfolio";

interface PortfolioReturnProps {
	portfolio: PortfolioSummary
}

function PortfolioReturn({portfolio} : PortfolioReturnProps) {
	return(
    <div className="p-8 content-center justify-center text-center align-middle">
      <div className="text-5xl font-bold">{portfolio.portfolioValue.toLocaleString(undefined, {style: "currency", currency: "AUD"})}</div>
    </div>	
	);
}

export default PortfolioReturn;
