using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class PortfolioSerializer : SerializerBase<Portfolio>
    {
        private IPortfolioFactory _Factory;
        public PortfolioSerializer(IPortfolioFactory factory)
        {
            _Factory = factory;
        }

        public override Portfolio Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            Guid id;
            string portfolioName = "";
            Guid owner;
            List<PortfolioTransaction> transactions = null;
            List<Guid> participateInDrp = null;

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                switch (name)
                {
                    case "_id":
                        id = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "name":
                        portfolioName = bsonReader.ReadString();
                        break;
                    case "owner":
                        owner = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "transactions":
                        transactions = BsonSerializer.Deserialize<List<PortfolioTransaction>>(bsonReader);
                        break;

                    case "participateInDrp":
                        participateInDrp = BsonSerializer.Deserialize<List<Guid>>(bsonReader);
                        break;
                }
            }

            var portfolio = _Factory.CreatePortfolio(id);
            portfolio.Create(portfolioName, owner);

            if (transactions != null)
            {
                foreach (var transaction in transactions)
                {
                    if (transaction is Aquisition aquisition)
                        portfolio.AquireShares(aquisition.Stock.Id, aquisition.Date, aquisition.Units, aquisition.AveragePrice, aquisition.TransactionCosts, aquisition.CreateCashTransaction, aquisition.Comment, aquisition.Id);
                    else if (transaction is CashTransaction cashTransaction)
                        portfolio.MakeCashTransaction(cashTransaction.Date, cashTransaction.CashTransactionType, cashTransaction.Amount, cashTransaction.Comment, cashTransaction.Id);
                    else if (transaction is CostBaseAdjustment costBaseAdjustment)
                        portfolio.AdjustCostBase(costBaseAdjustment.Stock.Id, costBaseAdjustment.Date, costBaseAdjustment.Percentage, costBaseAdjustment.Comment, costBaseAdjustment.Id);
                    else if (transaction is Disposal disposal)
                        portfolio.DisposeOfShares(disposal.Stock.Id, disposal.Date, disposal.Units, disposal.AveragePrice, disposal.TransactionCosts, disposal.CgtMethod, disposal.CreateCashTransaction, disposal.Comment, disposal.Id);
                    else if (transaction is IncomeReceived income)
                        portfolio.IncomeReceived(income.Stock.Id, income.RecordDate, income.Date, income.FrankedAmount, income.UnfrankedAmount, income.FrankingCredits, income.Interest, income.TaxDeferred, income.DrpCashBalance, income.CreateCashTransaction, income.Comment, income.Id);
                    else if (transaction is OpeningBalance openingBalance)
                        portfolio.AddOpeningBalance(openingBalance.Stock.Id, openingBalance.Date, openingBalance.AquisitionDate, openingBalance.Units, openingBalance.CostBase, openingBalance.Comment, openingBalance.Id);
                    else if (transaction is ReturnOfCapital returnOfCapital)
                        portfolio.ReturnOfCapitalReceived(returnOfCapital.Stock.Id, returnOfCapital.Date, returnOfCapital.RecordDate, returnOfCapital.Amount, returnOfCapital.CreateCashTransaction, returnOfCapital.Comment, returnOfCapital.Id);
                    else if (transaction is UnitCountAdjustment unitCountAdjustment)
                        portfolio.AdjustUnitCount(unitCountAdjustment.Stock.Id, unitCountAdjustment.Date, unitCountAdjustment.OriginalUnits, unitCountAdjustment.NewUnits, unitCountAdjustment.Comment, unitCountAdjustment.Id);
                }
            }


            foreach (var stockId in participateInDrp)
            {
                portfolio.ChangeDrpParticipation(stockId, true);
            }

            return portfolio; 
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Portfolio value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("_id");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);

            bsonWriter.WriteName("name");
            bsonWriter.WriteString(value.Name);

            bsonWriter.WriteName("owner");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Owner);

            bsonWriter.WriteName("participateInDrp");
            bsonWriter.WriteStartArray();
            foreach (var holding in value.Holdings.All())
            {
                if (holding.Settings.ParticipateInDrp)
                    BsonSerializer.Serialize<Guid>(bsonWriter, holding.Id);
            }
            bsonWriter.WriteEndArray();

            bsonWriter.WriteName("transactions");
            BsonSerializer.Serialize<ITransactionList<IPortfolioTransaction>>(bsonWriter, value.Transactions);

            bsonWriter.WriteEndDocument(); 
        }
    }
}
