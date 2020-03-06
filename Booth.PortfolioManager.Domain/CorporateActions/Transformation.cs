using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.CorporateActions
{
    public class Transformation : ICorporateAction
    {
        public Guid Id { get; private set; }
        public Stock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }
        public Date ImplementationDate { get; private set; }
        public decimal CashComponent { get; private set; }
        public bool RolloverRefliefApplies { get; private set; }

        private List<ResultingStock> _ResultingStocks = new List<ResultingStock>();
        public IEnumerable<ResultingStock> ResultingStocks
        {
            get { return _ResultingStocks; }
        }

        internal Transformation(Guid id, Stock stock, Date actionDate, string description, Date implementationDate, decimal cashComponent, bool rolloverReliefApplies, IEnumerable<ResultingStock> resultingStocks)
        {
            Id = id;
            Stock = stock;
            Date = actionDate;
            Type = CorporateActionType.Transformation;
            Description = description;
            ImplementationDate = implementationDate;
            CashComponent = cashComponent;
            RolloverRefliefApplies = rolloverReliefApplies;
            _ResultingStocks.AddRange(resultingStocks);
        }

        public class ResultingStock
        {
            public Guid Stock { get; private set; }
            public int OriginalUnits { get; set; }
            public int NewUnits { get; set; }
            public decimal CostBasePercentage { get; set; }
            public Date AquisitionDate { get; set; }

            public ResultingStock(Guid stock, int originalUnits, int newUnits, decimal costBasePercentage, Date aquisitionDate)
            {
                Stock = stock;
                OriginalUnits = originalUnits;
                NewUnits = newUnits;
                CostBasePercentage = costBasePercentage;
                AquisitionDate = aquisitionDate;
            }
        }

        public IEnumerable<IPortfolioTransaction> GetTransactionList(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var transactions = new List<IPortfolioTransaction>();

            var holdingProperties = holding.Properties[Date];
            if (holdingProperties.Units == 0)
                return transactions;

            if (ResultingStocks.Any())
            {
                if (RolloverRefliefApplies)
                {
                    var resultStockTransactions = ResultStockTransactionsWithRollover(holding, stockResolver);
                    transactions.AddRange(resultStockTransactions);

                    // Reduce the costbase of the original parcels 
                    decimal originalCostBasePercentage = 1 - ResultingStocks.Sum(x => x.CostBasePercentage);
                    transactions.Add(new CostBaseAdjustment()
                    {
                        Id = Guid.NewGuid(),
                        Date = ImplementationDate,
                        Stock = Stock,
                        Comment = Description,
                        Percentage = originalCostBasePercentage
                    });
                }
                else
                {
                    var resultStockTransactions = ResultStockTransactions(holding, stockResolver);

                    transactions.AddRange(resultStockTransactions);

                    // Reduce the costbase of the original parcels 
                    var amount = resultStockTransactions.OfType<OpeningBalance>().Sum(x => x.CostBase);
                    transactions.Add(new ReturnOfCapital()
                    {
                        Id = Guid.NewGuid(),
                        Date = ImplementationDate,
                        Stock = Stock,
                        Comment = Description,
                        RecordDate = Date,
                        Amount = amount,
                        CreateCashTransaction = false
                    });
                }
            }

            // Handle disposal of original parcels 
            if (CashComponent > 0.00m)
            {
                transactions.Add(new Disposal()
                {
                    Id = Guid.NewGuid(),
                    Date = ImplementationDate,
                    Stock = Stock,
                    Units = holdingProperties.Units,
                    AveragePrice = CashComponent,
                    TransactionCosts = 0.00m,
                    CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                    CreateCashTransaction = true,
                    Comment = Description
                });
            }

            return transactions;
        }

        private IEnumerable<IPortfolioTransaction> ResultStockTransactions(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var holdingProperties = holding.Properties[Date];

            // create parcels for resulting stock 
            foreach (var resultingStock in ResultingStocks)
            {
                int units = (int)Math.Ceiling(holdingProperties.Units * ((decimal)resultingStock.NewUnits / (decimal)resultingStock.OriginalUnits));
                decimal costBase = holdingProperties.CostBase * resultingStock.CostBasePercentage;

                var stock = new Stock(Guid.Empty);

                var transaction = new OpeningBalance()
                {
                    Id = Guid.NewGuid(),
                    Date = ImplementationDate,
                    Stock = stockResolver.GetStock(resultingStock.Stock),
                    Units = units,
                    CostBase = costBase,
                    AquisitionDate = ImplementationDate,
                    Comment = Description
                };

                yield return transaction;
            }
        }

        private IEnumerable<IPortfolioTransaction> ResultStockTransactionsWithRollover(IReadOnlyHolding holding, IStockResolver stockResolver)
        {
            var parcels = holding.Parcels(Date);
            foreach (var parcel in parcels)
            {
                var parcelProperties = parcel.Properties[Date];

                // create parcels for resulting stock 
                foreach (var resultingStock in ResultingStocks)
                {
                    int units = (int)Math.Ceiling(parcelProperties.Units * ((decimal)resultingStock.NewUnits / (decimal)resultingStock.OriginalUnits));
                    decimal costBase = parcelProperties.CostBase * resultingStock.CostBasePercentage;

                    var stock = new Stock(Guid.Empty);

                    var transaction = new OpeningBalance()
                    {
                        Id = Guid.NewGuid(),
                        Date = ImplementationDate,
                        Stock = stockResolver.GetStock(resultingStock.Stock),
                        Units = units,
                        CostBase = costBase,
                        AquisitionDate = parcel.AquisitionDate,
                        Comment = Description
                    };

                    yield return transaction;
                }
            }
        }

        public bool HasBeenApplied(IPortfolioTransactionList transactions)
        {
            if (ResultingStocks.Any())
                return transactions.ForHolding(ResultingStocks.First().Stock, ImplementationDate).OfType<OpeningBalance>().Any();
            else
                return transactions.ForHolding(Stock.Id, ImplementationDate).OfType<Disposal>().Any(); 
        }
    }
}
