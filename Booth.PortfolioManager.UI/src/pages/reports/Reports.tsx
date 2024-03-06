import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

function Reports() {

    return (
		<div className="grid grid-flow-row-dense grid-cols-3 grid-rows-3 gap-8">
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>Performance</CardTitle>
						<CardDescription>Portfolio Performance over selected period</CardDescription>
					</CardHeader>
				</Card>
			</div>
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>Unrealised Capital Gains</CardTitle>
						<CardDescription>Current unrealised capital gains</CardDescription>
					</CardHeader>
				</Card>
			</div>
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>Portfolio Return</CardTitle>
						<CardDescription>Return over selected periods (monthly, quarterly, annually)</CardDescription>
					</CardHeader>
				</Card>
			</div>	
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>Taxable Income</CardTitle>
						<CardDescription>Taxable Income for financial year</CardDescription>
					</CardHeader>
				</Card>
			</div>
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>CGT</CardTitle>
						<CardDescription>CGT for financial year</CardDescription>
					</CardHeader>
				</Card>
			</div>	
			<div className="col-span-1">
				<Card>
					<CardHeader>
						<CardTitle>Detailed CGT</CardTitle>
						<CardDescription>Detailed CGT for financial year</CardDescription>
					</CardHeader>
				</Card>
			</div>			
		</div>
    );
}

export default Reports;