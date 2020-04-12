using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Domain.Portfolios.Events;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Transactions.Events;
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
        void AddOpeningBalance(Guid stockId, Date transactionDate, Date aquisitionDate, int units, decimal costBase, string comment, Guid transactionId);
        void AdjustUnitCount(Guid stockId, Date date,int oldCount, int NewCount, string comment, Guid transactionId);
        void AquireShares(Guid stockId, Date aquisitionDate, int units, decimal averagePrice, decimal transactionCosts, bool createCashTransaction, string comment, Guid transactionId);
        void DisposeOfShares(Guid stockId, Date disposalDate, int units, decimal averagePrice, decimal transactionCosts, CgtCalculationMethod cgtMethod, bool createCashTransaction, string comment, Guid transactionId);
        void IncomeReceived(Guid stockId, Date recordDate, Date paymentDate, decimal frankedAmount, decimal unfrankedAmount, decimal frankingCredits, decimal interest, decimal taxDeferred, decimal drpCashBalance, bool createCashTransaction, string comment, Guid transactionId);
        void MakeCashTransaction(Date transactionDate, BankAccountTransactionType type, decimal amount, string comment, Guid transactionId);
        void ReturnOfCapitalReceived(Guid stockId, Date paymentDate, Date recordDate, decimal amount, bool createCashTransaction, string comment, Guid transactionId);
    }

    public class Portfolio : TrackedEntity, IPortfolio, IReadOnlyPortfolio
    {
        private IServiceFactory<ITransactionHandler> _TransactionHandlers;

        public string Name { get; private set; }
        public Guid Owner { get; private set; }

        private IStockResolver _StockResolver;

        private HoldingCollection _Holdings = new HoldingCollection();
        public IHoldingCollection Holdings => _Holdings;

        private PortfolioTransactionList _Transactions = new PortfolioTransactionList();
        public IPortfolioTransactionList Transactions => _Transactions;

        private ICashAccount _CashAccount = new CashAccount();
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
            : base(id)
        {
            _StockResolver = stockResolver;
            _TransactionHandlers = transactionHandlers;
        }

        public void Create(string name, Guid owner)
        {
            var @event = new PortfolioCreatedEvent(Id, Version, name, owner);
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(PortfolioCreatedEvent @event)
        {
            Version++;

            Name = @event.Name;
            Owner = @event.Owner;
        }

        public void ChangeDrpParticipation(Guid stockId, bool participateInDrp)
        {
            var holding = _Holdings[stockId];
            if (holding == null)
                throw new ArgumentException("No holding found");

            var @event = new DrpParticipationChangedEvent(Id, Version, holding.Id, participateInDrp);
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(DrpParticipationChangedEvent @event)
        {
            Version++;

            var holding = _Holdings[@event.Holding];
            holding.ChangeDrpParticipation(@event.ParticipateInDrp);
        }

        public void MakeCashTransaction(Date transactionDate, BankAccountTransactionType type, decimal amount, string comment, Guid transactionId)
        {
            var @event = new CashTransactionOccurredEvent(Id, Version, transactionId, transactionDate, Guid.Empty, comment)
            {
                CashTransactionType = type,
                Amount = amount
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(CashTransactionOccurredEvent @event)
        {
            var cashTransaction = new CashTransaction
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = null,
                Comment = @event.Comment,
                CashTransactionType = @event.CashTransactionType,
                Amount = @event.Amount
            };

            var handler = _TransactionHandlers.GetService<CashTransaction>();
            handler.Apply(cashTransaction, null, _CashAccount);
            _Transactions.Add(cashTransaction);
        }

        public void AquireShares(Guid stockId, Date aquisitionDate, int units, decimal averagePrice, decimal transactionCosts, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new AquisitionOccurredEvent(Id, Version, transactionId, aquisitionDate, stockId, comment)
            {
                Units = units,
                AveragePrice = averagePrice,
                TransactionCosts = transactionCosts,
                CreateCashTransaction = createCashTransaction
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(AquisitionOccurredEvent @event)
        {         
            var holding = _Holdings[@event.Stock];
            if (holding == null)
            {
                var stock = _StockResolver.GetStock(@event.Stock);
                holding = _Holdings.Add(stock, @event.Date);
            }

            var aquisition = new Aquisition
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                Units = @event.Units,
                AveragePrice = @event.AveragePrice,
                TransactionCosts = @event.TransactionCosts,
                CreateCashTransaction = @event.CreateCashTransaction
            };

            var handler = _TransactionHandlers.GetService<Aquisition>();
            handler.Apply(aquisition, holding, _CashAccount);
            _Transactions.Add(aquisition);
        }

        public void DisposeOfShares(Guid stockId, Date disposalDate, int units, decimal averagePrice, decimal transactionCosts, CgtCalculationMethod cgtMethod, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new DisposalOccurredEvent(Id, Version, transactionId, disposalDate, stockId, comment)
            {
                Units = units,
                AveragePrice = averagePrice,
                TransactionCosts = transactionCosts,
                CgtMethod = cgtMethod,
                CreateCashTransaction = createCashTransaction
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(DisposalOccurredEvent @event)
        {
            var holding = _Holdings[@event.Stock];
            if (holding == null)
                throw new NoSharesOwned("No shares owned");

            var disposal = new Disposal
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                Units = @event.Units,
                AveragePrice = @event.AveragePrice,
                TransactionCosts = @event.TransactionCosts,
                CreateCashTransaction = @event.CreateCashTransaction
            };

            var handler = _TransactionHandlers.GetService<Disposal>();
            handler.Apply(disposal, holding, _CashAccount);
            _Transactions.Add(disposal);
        }

        public void IncomeReceived(Guid stockId, Date recordDate, Date paymentDate, decimal frankedAmount, decimal unfrankedAmount, decimal frankingCredits, decimal interest, decimal taxDeferred, decimal drpCashBalance, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new IncomeOccurredEvent(Id, Version, transactionId, paymentDate, stockId, comment)
            {
                RecordDate = recordDate,
                FrankedAmount = frankedAmount,
                UnfrankedAmount = unfrankedAmount,
                FrankingCredits = frankingCredits,
                Interest = interest,
                TaxDeferred = taxDeferred,
                CreateCashTransaction = createCashTransaction,
                DRPCashBalance = drpCashBalance
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(IncomeOccurredEvent @event)
        {
            var holding = _Holdings[@event.Stock];
            if (holding == null)
                throw new NoSharesOwned("No shares owned");

            var incomeReceived = new IncomeReceived
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                RecordDate = @event.RecordDate,
                FrankedAmount = @event.FrankedAmount,
                UnfrankedAmount = @event.UnfrankedAmount,
                FrankingCredits = @event.FrankingCredits,
                Interest = @event.Interest,
                TaxDeferred = @event.TaxDeferred,
                CreateCashTransaction = @event.CreateCashTransaction,
                DRPCashBalance = @event.DRPCashBalance
            };

            var handler = _TransactionHandlers.GetService<IncomeReceived>();
            handler.Apply(incomeReceived, holding, _CashAccount);
            _Transactions.Add(incomeReceived);
        }

        public void AddOpeningBalance(Guid stockId, Date transactionDate, Date aquisitionDate, int units, decimal costBase, string comment, Guid transactionId)
        {
            var @event = new OpeningBalanceOccurredEvent(Id, Version, transactionId, transactionDate, stockId, comment)
            {
                AquisitionDate = aquisitionDate,
                Units = units,
                CostBase = costBase
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(OpeningBalanceOccurredEvent @event)
        {
            var holding = _Holdings[@event.Stock];
            if (holding == null)
            {
                var stock = _StockResolver.GetStock(@event.Stock);
                holding = _Holdings.Add(stock, @event.Date);
            }

            var openingBalance = new OpeningBalance
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                AquisitionDate = @event.AquisitionDate,
                Units = @event.Units,
                CostBase = @event.CostBase
            };

            var handler = _TransactionHandlers.GetService<OpeningBalance>();
            handler.Apply(openingBalance, holding, _CashAccount);
            _Transactions.Add(openingBalance);
        }

        public void ReturnOfCapitalReceived(Guid stockId, Date paymentDate, Date recordDate, decimal amount, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new ReturnOfCapitalOccurredEvent(Id, Version, transactionId, paymentDate, stockId, comment)
            {
                RecordDate = recordDate,
                Amount = amount,
                CreateCashTransaction = createCashTransaction
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(ReturnOfCapitalOccurredEvent @event)
        {
            var holding = _Holdings[@event.Stock];
            if (holding == null)
                throw new NoSharesOwned("No shares owned");

            var returnOfCapital = new ReturnOfCapital
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                RecordDate = @event.RecordDate,
                Amount = @event.Amount,
                CreateCashTransaction = @event.CreateCashTransaction
            };

            var handler = _TransactionHandlers.GetService<ReturnOfCapital>();
            handler.Apply(returnOfCapital, holding, _CashAccount);
            _Transactions.Add(returnOfCapital);
        }

        public void AdjustUnitCount(Guid stockId, Date date, int oldCount, int newCount, string comment, Guid transactionId)
        {
            var @event = new UnitCountAdjustmentOccurredEvent(Id, Version, transactionId, date, stockId, comment)
            {
                OriginalUnitCount = oldCount,
                NewUnitCount = newCount
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(UnitCountAdjustmentOccurredEvent @event)
        {
            var holding = _Holdings[@event.Stock];
            if (holding == null)
                throw new NoSharesOwned("No shares owned");

            var unitCountAdjustment = new UnitCountAdjustment
            {
                Id = @event.TransactionId,
                Date = @event.Date,
                Stock = holding.Stock,
                Comment = @event.Comment,
                OriginalUnits = @event.OriginalUnitCount,
                NewUnits = @event.NewUnitCount
            };

            var handler = _TransactionHandlers.GetService<UnitCountAdjustment>();
            handler.Apply(unitCountAdjustment, holding, _CashAccount);
            _Transactions.Add(unitCountAdjustment);
        }
    }
}
