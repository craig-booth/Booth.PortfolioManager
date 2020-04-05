using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios.Events;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Transactions.Events;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.Portfolios
{

    public class Portfolio : TrackedEntity
    {
        private ServiceFactory<ITransactionHandler> _TransactionHandlers = new ServiceFactory<ITransactionHandler>();

        public string Name { get; private set; }
        public Guid Owner { get; private set; }

        private IStockResolver _StockResolver;

        private IHoldingCollection _Holdings = new HoldingCollection();
        public IReadOnlyHoldingCollection Holdings => _Holdings;

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

        public Portfolio(Guid id, IStockResolver stockResolver)
            : base(id)
        {
            _StockResolver = stockResolver;

            _TransactionHandlers.Register<Aquisition>(() => new AquisitionHandler(_Holdings, _CashAccount));
            _TransactionHandlers.Register<Disposal>(() => new DisposalHandler(_Holdings, _CashAccount, _CgtEvents));
            _TransactionHandlers.Register<CashTransaction>(() => new CashTransactionHandler(_CashAccount));
            _TransactionHandlers.Register<OpeningBalance>(() => new OpeningBalanceHandler(_Holdings, _CashAccount));
            _TransactionHandlers.Register<IncomeReceived>(() => new IncomeReceivedHandler(_Holdings, _CashAccount));
            _TransactionHandlers.Register<ReturnOfCapital>(() => new ReturnOfCapitalHandler(_Holdings, _CashAccount));
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

        public void ChangeDrpParticipation(Guid holding, bool participateInDrp)
        {
            var @event = new DrpParticipationChangedEvent(Id, Version, holding, participateInDrp);
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
            var cashTransaction = new CashTransaction();
            cashTransaction.Id = @event.TransactionId;
            cashTransaction.Date = @event.Date;
            cashTransaction.Stock = _StockResolver.GetStock(@event.Stock);
            cashTransaction.Comment = @event.Comment;
            cashTransaction.CashTransactionType = @event.CashTransactionType;
            cashTransaction.Amount = @event.Amount;

            var handler = _TransactionHandlers.GetService<CashTransaction>();
            handler.ApplyTransaction(cashTransaction);
            _Transactions.Add(cashTransaction);
        }

        public void AquireShares(Date aquisitionDate, Stock stock, int units, decimal averagePrice, decimal transactionCosts, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new AquisitionOccurredEvent(Id, Version, transactionId, aquisitionDate, stock.Id, comment)
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
            var aquisition = new Aquisition();
            aquisition.Id = @event.TransactionId;
            aquisition.Date = @event.Date;
            aquisition.Stock = _StockResolver.GetStock(@event.Stock);
            aquisition.Comment = @event.Comment;
            aquisition.Units = @event.Units;
            aquisition.AveragePrice = @event.AveragePrice;
            aquisition.TransactionCosts = @event.TransactionCosts;
            aquisition.CreateCashTransaction = @event.CreateCashTransaction;

            var handler = _TransactionHandlers.GetService<Aquisition>();
            handler.ApplyTransaction(aquisition);
            _Transactions.Add(aquisition);
        }

        public void DisposeOfShares(Date disposalDate, Stock stock, int units, decimal averagePrice, decimal transactionCosts, CGTCalculationMethod cgtMethod, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new DisposalOccurredEvent(Id, Version, transactionId, disposalDate, stock.Id, comment)
            {
                Units = units,
                AveragePrice = averagePrice,
                TransactionCosts = transactionCosts,
                CGTMethod = cgtMethod,
                CreateCashTransaction = createCashTransaction
            };
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(DisposalOccurredEvent @event)
        {
            var disposal = new Disposal();
            disposal.Id = @event.TransactionId;
            disposal.Date = @event.Date;
            disposal.Stock = _StockResolver.GetStock(@event.Stock);
            disposal.Comment = @event.Comment;
            disposal.Units = @event.Units;
            disposal.AveragePrice = @event.AveragePrice;
            disposal.TransactionCosts = @event.TransactionCosts;
            disposal.CreateCashTransaction = @event.CreateCashTransaction;

            var handler = _TransactionHandlers.GetService<Disposal>();
            handler.ApplyTransaction(disposal);
            _Transactions.Add(disposal);
        }

        public void IncomeReceived(Date recordDate, Date paymentDate, Stock stock, decimal frankedAmount, decimal unfrankedAmount, decimal frankingCredits, decimal interest, decimal taxDeferred, decimal drpCashBalance, bool createCashTransaction, string comment, Guid transactionId)
        {
            var @event = new IncomeOccurredEvent(Id, Version, transactionId, paymentDate, stock.Id, comment)
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
            var incomeReceived = new IncomeReceived();
            incomeReceived.Id = @event.TransactionId;
            incomeReceived.Date = @event.Date;
            incomeReceived.Stock = _StockResolver.GetStock(@event.Stock);
            incomeReceived.Comment = @event.Comment;
            incomeReceived.RecordDate = @event.RecordDate;
            incomeReceived.FrankedAmount = @event.FrankedAmount;
            incomeReceived.UnfrankedAmount = @event.UnfrankedAmount;
            incomeReceived.FrankingCredits = @event.FrankingCredits;
            incomeReceived.Interest = @event.Interest;
            incomeReceived.TaxDeferred = @event.TaxDeferred;
            incomeReceived.CreateCashTransaction = @event.CreateCashTransaction;
            incomeReceived.DRPCashBalance = @event.DRPCashBalance;

            var handler = _TransactionHandlers.GetService<IncomeReceived>();
            handler.ApplyTransaction(incomeReceived);
            _Transactions.Add(incomeReceived);
        }

        public void AddOpeningBalance(Date transactionDate, Date aquisitionDate, Stock stock, int units, decimal costBase, string comment, Guid transactionId)
        {
            var @event = new OpeningBalanceOccurredEvent(Id, Version, transactionId, transactionDate, stock.Id, comment)
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
            var openingBalance = new OpeningBalance();
            openingBalance.Id = @event.TransactionId;
            openingBalance.Date = @event.Date;
            openingBalance.Stock = _StockResolver.GetStock(@event.Stock);
            openingBalance.Comment = @event.Comment;
            openingBalance.AquisitionDate = @event.AquisitionDate;
            openingBalance.Units = @event.Units;
            openingBalance.CostBase = @event.CostBase;

            var handler = _TransactionHandlers.GetService<OpeningBalance>();
            handler.ApplyTransaction(openingBalance);
            _Transactions.Add(openingBalance);
        }

        public void ReturnOfCapitalReceived(Date paymentDate, Date recordDate, Stock stock, decimal amount, bool createCashTransaction, string comment, Guid transactionId)
        {

        }

        public void Apply(ReturnOfCapitalOccurredEvent @event)
        {

        }

        public void AdjustUnitCount(Date date, Stock stock, int oldCount, int NewCount, string comment, Guid transactionId)
        {

        }

        public void Apply(UnitCountAdjustmentOccurredEvent @event)
        {

        }
    }
}
