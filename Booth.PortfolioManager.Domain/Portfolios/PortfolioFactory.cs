using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.PortfolioManager.Domain.Transactions;


namespace Booth.PortfolioManager.Domain.Portfolios
{
    public interface IPortfolioFactory
    {
        Portfolio CreatePortfolio(Guid id);
    }

    public class PortfolioFactory : IPortfolioFactory
    {

        private IStockResolver _StockResolver;
        private IServiceFactory<ITransactionHandler> _TransactionHandlers = new ServiceFactory<ITransactionHandler>();
        public PortfolioFactory(IStockResolver stockResolver)
        {
            _StockResolver = stockResolver;

            _TransactionHandlers.Register<Aquisition>(() => new AquisitionHandler());
            _TransactionHandlers.Register<Disposal>(() => new DisposalHandler());
            _TransactionHandlers.Register<CashTransaction>(() => new CashTransactionHandler());
            _TransactionHandlers.Register<OpeningBalance>(() => new OpeningBalanceHandler());
            _TransactionHandlers.Register<IncomeReceived>(() => new IncomeReceivedHandler());
            _TransactionHandlers.Register<ReturnOfCapital>(() => new ReturnOfCapitalHandler());
            _TransactionHandlers.Register<CostBaseAdjustment>(() => new CostBaseAdjustmentHandler());
            _TransactionHandlers.Register<UnitCountAdjustment>(() => new UnitCountAdjustmentHandler());
        }

        public Portfolio CreatePortfolio(Guid id)
        {
            if (_StockResolver == null)
                throw new Exception("StockResolver");
            if (_TransactionHandlers == null)
                throw new Exception("TransactionHandlers");
            if (id == null)
                throw new Exception("Id");
            var portfolio = new Portfolio(id, _StockResolver, _TransactionHandlers);
            if (portfolio == null)
                throw new Exception("Portfolio");
            return portfolio;
        }
    }
}
