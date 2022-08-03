using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IReadOnlyPortfolio
    {
        Guid Id { get; }
        IReadOnlyCashAccount CashAccount { get; }
        ITransactionList<CgtEvent> CgtEvents { get; }
        Date EndDate { get; }
        IHoldingCollection Holdings { get; }
        string Name { get; }
        Guid Owner { get; }
        Date StartDate { get; }
        IPortfolioTransactionList Transactions { get; }
    }

    public interface IPortfolio : IReadOnlyPortfolio
    {
        void ChangeDrpParticipation(Guid stockId, bool participateInDrp);

        void AddTransaction(PortfolioTransaction transaction);
        void UpdateTransaction(PortfolioTransaction transaction);
        void DeleteTransaction(Guid transactionId);

        void AddOpeningBalance(Guid stockId, Date transactionDate, Date aquisitionDate, int units, decimal costBase, string comment, Guid transactionId);
        void AdjustUnitCount(Guid stockId, Date date,int oldCount, int NewCount, string comment, Guid transactionId);
        void AdjustCostBase(Guid stockId, Date date, decimal percentage, string comment, Guid transactionId);
        void AquireShares(Guid stockId, Date aquisitionDate, int units, decimal averagePrice, decimal transactionCosts, bool createCashTransaction, string comment, Guid transactionId);
        void DisposeOfShares(Guid stockId, Date disposalDate, int units, decimal averagePrice, decimal transactionCosts, CgtCalculationMethod cgtMethod, bool createCashTransaction, string comment, Guid transactionId);
        void IncomeReceived(Guid stockId, Date recordDate, Date paymentDate, decimal frankedAmount, decimal unfrankedAmount, decimal frankingCredits, decimal interest, decimal taxDeferred, decimal drpCashBalance, bool createCashTransaction, string comment, Guid transactionId);
        void MakeCashTransaction(Date transactionDate, BankAccountTransactionType type, decimal amount, string comment, Guid transactionId);
        void ReturnOfCapitalReceived(Guid stockId, Date paymentDate, Date recordDate, decimal amount, bool createCashTransaction, string comment, Guid transactionId);
    }

    public class Portfolio : IEntity, IPortfolio, IReadOnlyPortfolio
    {
        public Guid Id { get; }

        private IServiceFactory<ITransactionHandler> _TransactionHandlers;

        public string Name { get; private set; }
        public Guid Owner { get; private set; }

        private IStockResolver _StockResolver;

        private HoldingCollection _Holdings = new HoldingCollection();
        public IHoldingCollection Holdings => _Holdings;

        private PortfolioTransactionList _Transactions = new PortfolioTransactionList();
        public IPortfolioTransactionList Transactions => _Transactions;

        private CashAccount _CashAccount = new CashAccount();
        public IReadOnlyCashAccount CashAccount => _CashAccount;

        private CgtEventCollection _CgtEvents = new CgtEventCollection();
        public ITransactionList<CgtEvent> CgtEvents => _CgtEvents;

        public Date StartDate
        {
            get { return DateUtils.Earlist(_Transactions.Earliest, _CashAccount.Transactions.Earliest); }
        }

        public Date EndDate
        {
            get { return Date.MaxValue; }
        }

        internal Portfolio(Guid id, IStockResolver stockResolver, IServiceFactory<ITransactionHandler> transactionHandlers)
        {
            Id = id;
            _StockResolver = stockResolver;
            _TransactionHandlers = transactionHandlers;
        }

        public void Create(string name, Guid owner)
        {
            Name = name;
            Owner = owner;
        }

        public void ChangeDrpParticipation(Guid stockId, bool participateInDrp)
        {
            var holding = _Holdings[stockId];
            if (holding == null)
                throw new ArgumentException("No holding found");

            holding.ChangeDrpParticipation(participateInDrp);
        }

        private void RecreatePortfolio()
        {
            _Holdings.Clear();
            _CashAccount.Clear();
            _CgtEvents.Clear();

            foreach (var transaction in _Transactions)
                ApplyTransaction((PortfolioTransaction)transaction);
        }

        private void ApplyTransaction(PortfolioTransaction transaction)
        {
            var handler = _TransactionHandlers.GetService(transaction);

            Holding holding = null;
            if (transaction.Stock != null)
            {
                var existingHolding = _Holdings[transaction.Stock.Id];
                if (existingHolding != null)
                    holding = existingHolding;
                else
                {
                    if (handler.CanCreateHolding)
                    {
                        holding = _Holdings.Add(transaction.Stock, transaction.Date);
                        holding.CgtEventOccurred += Holding_CgtEventOccurred;
                    }
                    else
                        throw new NoSharesOwnedException("No shares owned");
                }
            }

            handler.Apply(transaction, holding, _CashAccount);
        }

        public void AddTransaction(PortfolioTransaction transaction)
        {
            ApplyTransaction(transaction);
            _Transactions.Add(transaction);
        }

        public void AddTransactions(IEnumerable<PortfolioTransaction> transactions)
        {
            foreach (var transaction in transactions)
                _Transactions.Add(transaction);

            RecreatePortfolio();
        }

        public void UpdateTransaction(PortfolioTransaction transaction)
        {
            _Transactions.Update(transaction);
            RecreatePortfolio();
        }

        public void DeleteTransaction(Guid transactionId)
        {
            _Transactions.Remove(transactionId);
            RecreatePortfolio();
        }

        public void MakeCashTransaction(Date transactionDate, BankAccountTransactionType type, decimal amount, string comment, Guid transactionId)
        {
            var cashTransaction = new CashTransaction
            {
                Id = transactionId,
                Date = transactionDate,
                Stock = null,
                Comment = comment,
                CashTransactionType = type,
                Amount = amount
            };

            AddTransaction(cashTransaction);
        }

        public void AquireShares(Guid stockId, Date aquisitionDate, int units, decimal averagePrice, decimal transactionCosts, bool createCashTransaction, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var aquisition = new Aquisition
            {
                Id = transactionId,
                Date = aquisitionDate,
                Stock = stock,
                Comment = comment,
                Units = units,
                AveragePrice = averagePrice,
                TransactionCosts = transactionCosts,
                CreateCashTransaction = createCashTransaction
            };

            AddTransaction(aquisition);
        }

        public void DisposeOfShares(Guid stockId, Date disposalDate, int units, decimal averagePrice, decimal transactionCosts, CgtCalculationMethod cgtMethod, bool createCashTransaction, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var disposal = new Disposal
            {
                Id = transactionId,
                Date = disposalDate,
                Stock = stock,
                Comment = comment,
                Units = units,
                AveragePrice = averagePrice,
                TransactionCosts = transactionCosts,
                CreateCashTransaction = createCashTransaction
            };

            AddTransaction(disposal);
        }

        public void IncomeReceived(Guid stockId, Date recordDate, Date paymentDate, decimal frankedAmount, decimal unfrankedAmount, decimal frankingCredits, decimal interest, decimal taxDeferred, decimal drpCashBalance, bool createCashTransaction, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var incomeReceived = new IncomeReceived
            {
                Id = transactionId,
                Date = paymentDate,
                Stock = stock,
                Comment = comment,
                RecordDate = recordDate,
                FrankedAmount = frankedAmount,
                UnfrankedAmount = unfrankedAmount,
                FrankingCredits = frankingCredits,
                Interest = interest,
                TaxDeferred = taxDeferred,
                CreateCashTransaction = createCashTransaction,
                DrpCashBalance = drpCashBalance
            };

            AddTransaction(incomeReceived);
        }

        public void AddOpeningBalance(Guid stockId, Date transactionDate, Date aquisitionDate, int units, decimal costBase, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var openingBalance = new OpeningBalance
            {
                Id = transactionId,
                Date = transactionDate,
                Stock = stock,
                Comment = comment,
                AquisitionDate = aquisitionDate,
                Units = units,
                CostBase = costBase
            };

            AddTransaction(openingBalance);
        }

        public void ReturnOfCapitalReceived(Guid stockId, Date paymentDate, Date recordDate, decimal amount, bool createCashTransaction, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var returnOfCapital = new ReturnOfCapital
            {
                Id = transactionId,
                Date = paymentDate,
                Stock = stock,
                Comment = comment,
                RecordDate = recordDate,
                Amount = amount,
                CreateCashTransaction = createCashTransaction
            };

            AddTransaction(returnOfCapital);
        }

        public void AdjustCostBase(Guid stockId, Date date, decimal percentage, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var costBaseAdjustment = new CostBaseAdjustment
            {
                Id = transactionId,
                Date = date,
                Stock = stock,
                Comment = comment,
                Percentage = percentage
            };

            AddTransaction(costBaseAdjustment);
        }

        public void AdjustUnitCount(Guid stockId, Date date, int oldCount, int newCount, string comment, Guid transactionId)
        {
            var stock = _StockResolver.GetStock(stockId);
            var unitCountAdjustment = new UnitCountAdjustment
            {
                Id = transactionId,
                Date = date,
                Stock = stock,
                Comment = comment,
                OriginalUnits = oldCount,
                NewUnits = newCount
            };

            AddTransaction(unitCountAdjustment);
        }

        private void Holding_CgtEventOccurred(object sender, CgtEventArgs e)
        {
            _CgtEvents.Add(e.EventDate, e.Stock, e.Units, e.CostBase, e.AmountReceived, e.CapitalGain, e.CgtMethod);
        }

    }
}
