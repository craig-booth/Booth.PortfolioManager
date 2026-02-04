using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Models.Transaction;

namespace Booth.PortfolioManager.Web.Mappers
{
    public interface ITransactionMapper
    {
        Models.Transaction.Transaction ToApi(Domain.Transactions.IPortfolioTransaction transaction);
        Domain.Transactions.PortfolioTransaction FromApi(Models.Transaction.Transaction transaction);
        TransactionsResponse.TransactionItem ToTransactionItem(Domain.Transactions.IPortfolioTransaction transaction, Date date);


        Models.Transaction.Aquisition ToApi(Domain.Transactions.Aquisition transaction);
        Domain.Transactions.Aquisition FromApi(Models.Transaction.Aquisition transaction);
        Models.Transaction.CashTransaction ToApi(Domain.Transactions.CashTransaction transaction);
        Domain.Transactions.CashTransaction FromApi(Models.Transaction.CashTransaction transaction);
        Models.Transaction.CostBaseAdjustment ToApi(Domain.Transactions.CostBaseAdjustment transaction);
        Domain.Transactions.CostBaseAdjustment FromApi(Models.Transaction.CostBaseAdjustment transaction);
        Models.Transaction.Disposal ToApi(Domain.Transactions.Disposal transaction);
        Domain.Transactions.Disposal FromApi(Models.Transaction.Disposal transaction);
        Models.Transaction.IncomeReceived ToApi(Domain.Transactions.IncomeReceived transaction);
        Domain.Transactions.IncomeReceived FromApi(Models.Transaction.IncomeReceived transaction);
        Models.Transaction.OpeningBalance ToApi(Domain.Transactions.OpeningBalance transaction);
        Domain.Transactions.OpeningBalance FromApi(Models.Transaction.OpeningBalance transaction);
        Models.Transaction.ReturnOfCapital ToApi(Domain.Transactions.ReturnOfCapital transaction);
        Domain.Transactions.ReturnOfCapital FromApi(Models.Transaction.ReturnOfCapital transaction);
        Models.Transaction.UnitCountAdjustment ToApi(Domain.Transactions.UnitCountAdjustment transaction);
        Domain.Transactions.UnitCountAdjustment FromApi(Models.Transaction.UnitCountAdjustment transaction);
    }

    public class TransactionMapper : ITransactionMapper
    {
        private readonly IStockResolver _StockResolver;

        public TransactionMapper(IStockResolver stockResover)
        {
            _StockResolver = stockResover;
        }

        public PortfolioTransaction FromApi(Transaction transaction)
        {
            if (transaction is Models.Transaction.Aquisition aquisition)
                return FromApi(aquisition);
            else if (transaction is Models.Transaction.CashTransaction cashTransaction)
                return FromApi(cashTransaction);
            else if (transaction is Models.Transaction.CostBaseAdjustment costBaseAdjustment)
                return FromApi(costBaseAdjustment);
            else if (transaction is Models.Transaction.Disposal disposal)
                return FromApi(disposal);
            else if (transaction is Models.Transaction.IncomeReceived incomeReceived)
                return FromApi(incomeReceived);
            else if (transaction is Models.Transaction.OpeningBalance openingBalance)
                return FromApi(openingBalance);
            else if (transaction is Models.Transaction.ReturnOfCapital returnOfCapital)
                return FromApi(returnOfCapital);
            else if (transaction is Models.Transaction.UnitCountAdjustment unitCountAdjustment)
                return FromApi(unitCountAdjustment);
            else
                throw new NotSupportedException();
        }

        public Transaction ToApi(IPortfolioTransaction transaction)
        {
            if (transaction is Domain.Transactions.Aquisition aquisition)
                return ToApi(aquisition);
            else if (transaction is Domain.Transactions.CashTransaction cashTransaction)
                return ToApi(cashTransaction);
            else if (transaction is Domain.Transactions.CostBaseAdjustment costBaseAdjustment)
                return ToApi(costBaseAdjustment);
            else if (transaction is Domain.Transactions.Disposal disposal)
                return ToApi(disposal);
            else if (transaction is Domain.Transactions.IncomeReceived incomeReceived)
                return ToApi(incomeReceived);
            else if (transaction is Domain.Transactions.OpeningBalance openingBalance)
                return ToApi(openingBalance);
            else if (transaction is Domain.Transactions.ReturnOfCapital returnOfCapital)
                return ToApi(returnOfCapital);
            else if (transaction is Domain.Transactions.UnitCountAdjustment unitCountAdjustment)
                return ToApi(unitCountAdjustment);
            else
                throw new NotSupportedException();
        }

        public Domain.Transactions.Aquisition FromApi(Models.Transaction.Aquisition transaction)
        {
            var response = new Domain.Transactions.Aquisition()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                AveragePrice = transaction.AveragePrice,
                CreateCashTransaction = transaction.CreateCashTransaction,
                TransactionCosts = transaction.TransactionCosts,
                Units = transaction.Units
            };

            return response;
        }

        public Models.Transaction.Aquisition ToApi(Domain.Transactions.Aquisition transaction)
        {
            var response = new Models.Transaction.Aquisition()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                AveragePrice = transaction.AveragePrice,
                CreateCashTransaction = transaction.CreateCashTransaction,
                TransactionCosts = transaction.TransactionCosts,
                Units = transaction.Units
            };

            return response;

        }
        public Models.Transaction.CashTransaction ToApi(Domain.Transactions.CashTransaction transaction)
        {
            var response = new Models.Transaction.CashTransaction()
            {
                Id = transaction.Id,
                Stock = Guid.Empty,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                CashTransactionType = transaction.CashTransactionType.ToResponse(),
                Amount = transaction.Amount
            };

            return response;
        }

        public Domain.Transactions.CashTransaction FromApi(Models.Transaction.CashTransaction transaction)
        {
            var response = new Domain.Transactions.CashTransaction()
            {
                Id = transaction.Id,
                Stock = null,
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                CashTransactionType = transaction.CashTransactionType.ToDomain(),
                Amount = transaction.Amount
            };

            return response;
        }        

        public Models.Transaction.CostBaseAdjustment ToApi(Domain.Transactions.CostBaseAdjustment transaction)
        {
            var response = new Models.Transaction.CostBaseAdjustment()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                Percentage = transaction.Percentage,
            };

            return response;
        }

        public Domain.Transactions.CostBaseAdjustment FromApi(Models.Transaction.CostBaseAdjustment transaction)
        {
            var response = new Domain.Transactions.CostBaseAdjustment()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                Percentage = transaction.Percentage
            };

            return response;
        }

        public Domain.Transactions.Disposal FromApi(Models.Transaction.Disposal transaction)
        {
            var response = new Domain.Transactions.Disposal()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                Units = transaction.Units,
                AveragePrice = transaction.AveragePrice,
                TransactionCosts = transaction.TransactionCosts,
                CgtMethod = transaction.CgtMethod.ToDomain(),
                CreateCashTransaction = transaction.CreateCashTransaction
            };

            return response;
        }

        public Models.Transaction.Disposal ToApi(Domain.Transactions.Disposal transaction)
        {
            var response = new Models.Transaction.Disposal()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                Units = transaction.Units,
                AveragePrice = transaction.AveragePrice,
                TransactionCosts = transaction.TransactionCosts,
                CgtMethod = transaction.CgtMethod.ToResponse(),
                CreateCashTransaction = transaction.CreateCashTransaction
            };

            return response;
        }

        public Domain.Transactions.IncomeReceived FromApi(Models.Transaction.IncomeReceived transaction)
        {
            var response = new Domain.Transactions.IncomeReceived()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                RecordDate = transaction.RecordDate,
                FrankedAmount = transaction.FrankedAmount,
                UnfrankedAmount = transaction.UnfrankedAmount,
                FrankingCredits = transaction.FrankingCredits,
                Interest = transaction.Interest,
                TaxDeferred = transaction.TaxDeferred,
                CreateCashTransaction = transaction.CreateCashTransaction,
                DrpCashBalance = transaction.DrpCashBalance
            };

            return response;
        }

        public Models.Transaction.IncomeReceived ToApi(Domain.Transactions.IncomeReceived transaction)
        {
            var response = new Models.Transaction.IncomeReceived()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                RecordDate = transaction.RecordDate,
                FrankedAmount = transaction.FrankedAmount,
                UnfrankedAmount = transaction.UnfrankedAmount,
                FrankingCredits = transaction.FrankingCredits,
                Interest = transaction.Interest,
                TaxDeferred = transaction.TaxDeferred,
                CreateCashTransaction = transaction.CreateCashTransaction,
                DrpCashBalance = transaction.DrpCashBalance
            };

            return response;
        }

        public Domain.Transactions.OpeningBalance FromApi(Models.Transaction.OpeningBalance transaction)
        {
            var response = new Domain.Transactions.OpeningBalance()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                Units = transaction.Units,
                CostBase = transaction.CostBase,
                AquisitionDate = transaction.AquisitionDate
            };

            return response;
        }

        public Models.Transaction.OpeningBalance ToApi(Domain.Transactions.OpeningBalance transaction)
        {
            var response = new Models.Transaction.OpeningBalance()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                Units = transaction.Units,
                CostBase = transaction.CostBase,
                AquisitionDate = transaction.AquisitionDate
            };

            return response;
        }

        public Domain.Transactions.ReturnOfCapital FromApi(Models.Transaction.ReturnOfCapital transaction)
        {
            var response = new Domain.Transactions.ReturnOfCapital()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                RecordDate = transaction.RecordDate,
                Amount = transaction.Amount,
                CreateCashTransaction = transaction.CreateCashTransaction
            };

            return response;
        }

        public Models.Transaction.ReturnOfCapital ToApi(Domain.Transactions.ReturnOfCapital transaction)
        {
            var response = new Models.Transaction.ReturnOfCapital()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                RecordDate = transaction.RecordDate,
                Amount = transaction.Amount,
                CreateCashTransaction = transaction.CreateCashTransaction
            };

            return response;
        }

        public Domain.Transactions.UnitCountAdjustment FromApi(Models.Transaction.UnitCountAdjustment transaction)
        {
            var response = new Domain.Transactions.UnitCountAdjustment()
            {
                Id = transaction.Id,
                Stock = _StockResolver.GetStock(transaction.Stock),
                Date = transaction.TransactionDate,
                Comment = transaction.Comment,
                OriginalUnits = transaction.OriginalUnits,
                NewUnits = transaction.NewUnits
            };

            return response;
        }

        public Models.Transaction.UnitCountAdjustment ToApi(Domain.Transactions.UnitCountAdjustment transaction)
        {
            var response = new Models.Transaction.UnitCountAdjustment()
            {
                Id = transaction.Id,
                Stock = transaction.Stock.Id,
                TransactionDate = transaction.Date,
                Comment = transaction.Comment,
                Description = transaction.Description,
                OriginalUnits = transaction.OriginalUnits,
                NewUnits = transaction.NewUnits
            };

            return response;
        }

        public TransactionsResponse.TransactionItem ToTransactionItem(Domain.Transactions.IPortfolioTransaction transaction, Date date)
        {
            var transactionItem = new TransactionsResponse.TransactionItem();

            transactionItem.Id = transaction.Id;
            if (transaction.Stock != null)
                transactionItem.Stock = transaction.Stock.ToSummaryResponse(date);
            else
                transactionItem.Stock = null;
            transactionItem.TransactionDate = transaction.Date;
            transactionItem.Description = transaction.Description;
            transactionItem.Comment = transaction.Comment;

            return transactionItem;
        }

    }

}
