import { Holding } from "@/model/Portfolio";
import { formatCurrency, formatNumber } from "@/lib/formatting";
interface HoldingValueProps {
    holding: Holding
}

function HoldingValue({ holding }: HoldingValueProps) {
    return (
        <div className="p-8 content-center justify-center text-center align-middle">
            <div className="text-l">{holding.stock.name}</div>
            <div className="text-5xl font-bold">{formatCurrency(holding.value)}</div>
            <div className="text-m">{formatNumber(holding.units) + " units"}</div>
        </div>
    );
}

export default HoldingValue;