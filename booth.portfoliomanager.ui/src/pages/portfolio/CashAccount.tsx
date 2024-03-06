import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

function CashAccount() {

    return (
	<div className="grid grid-flow-row-dense grid-cols-3 grid-rows-3 gap-8">
        <div className="col-span-3">
			<Card>
				<CardHeader>
					<CardTitle>Balance</CardTitle>
					<CardDescription>Shows Cash Account Balance</CardDescription>
				</CardHeader>
			</Card>
		</div>
		<div className="col-span-3">
			<Card>
				<CardHeader>
					<CardTitle>Transactions</CardTitle>
					<CardDescription>Shows transactions</CardDescription>
				</CardHeader>
			</Card>
		</div>
	</div>
    );
}

export default CashAccount;