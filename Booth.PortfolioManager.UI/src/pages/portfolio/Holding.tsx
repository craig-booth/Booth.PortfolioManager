import {
    useParams
} from "react-router-dom";

import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

function Holding() {

	const params = useParams();

    return (
		<>	
			<h1>{params.holdingId}</h1>
			
			<div className="grid grid-flow-row-dense grid-cols-3 grid-rows-3 gap-8">
				<div className="col-span-3">
					<Card>
						<CardHeader>
							<CardTitle>Holding Return</CardTitle>
							<CardDescription>Shows Holding over various periods</CardDescription>
						</CardHeader>
					</Card>
				</div>
				<div className="col-span-3">
					<Card>
						<CardHeader>
							<CardTitle>Transactions</CardTitle>
							<CardDescription>Shows transactions for holding</CardDescription>
						</CardHeader>
					</Card>
				</div>
			</div>
		</>	
    );
}

export default Holding;