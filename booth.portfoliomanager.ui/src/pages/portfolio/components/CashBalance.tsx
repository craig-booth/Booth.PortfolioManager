import { CashAccount } from "@/model/Portfolio";
import { formatCurrency } from "@/lib/formatting";
interface CashBalanceProps {
    cashAccount: CashAccount
}

function CashBalance({ cashAccount }: CashBalanceProps) {
    return (
        <div className="p-8 content-center justify-center text-center align-middle">
            <div className="text-5xl font-bold">{formatCurrency(cashAccount.closingBalance)}</div>
        </div>
    );
}

export default CashBalance;