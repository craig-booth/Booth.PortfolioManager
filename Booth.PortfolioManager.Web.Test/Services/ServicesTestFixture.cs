using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xunit;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Web.Utilities;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class ServicesTestFixture
    {
        public IStockResolver StockResolver;
        public IPortfolioFactory PortfolioFactory;
        public ITradingCalendarRepository TradingCalendarRepository;

        private StockPriceRetriever_ _StockPriceRetriever;
        public IStockPriceRetriever StockPriceRetriever => _StockPriceRetriever;

        public IStockMapper StockMapper;
        public ITransactionMapper TransactionMapper;
        public IHoldingMapper HoldingMapper;

        public Models.Portfolio.Stock Stock_ARG;
        public Models.Portfolio.Stock Stock_WAM; 

        public Guid ARG_CapitalReturn = Guid.NewGuid();
        public Guid WAM_Split = Guid.NewGuid();
        public Guid WAM_OpeningBalance = Guid.NewGuid();

        public ServicesTestFixture()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var _StockCache = new EntityCache<Stock>();
            StockResolver = new StockResolver(_StockCache);
            PortfolioFactory = new PortfolioFactory(StockResolver);

            var tradingCalendarRepository = mockRepository.Create<ITradingCalendarRepository>();
            tradingCalendarRepository.Setup(x => x.GetAsync(TradingCalendarIds.ASX)).Returns(Task.FromResult<TradingCalendar>(new TradingCalendar(TradingCalendarIds.ASX)));
            TradingCalendarRepository = tradingCalendarRepository.Object;

            _StockPriceRetriever = new StockPriceRetriever_();

            StockMapper = new StockMapper(StockPriceRetriever);
            TransactionMapper = new TransactionMapper(StockResolver);
            HoldingMapper = new HoldingMapper(StockPriceRetriever);

            Stock_ARG = new Models.Portfolio.Stock() { Id = Guid.NewGuid(), AsxCode = "ARG", Name = "Argo", Category = Models.Stock.AssetCategory.AustralianStocks };
            Stock_WAM = new Models.Portfolio.Stock() { Id = Guid.NewGuid(), AsxCode = "WAM", Name = "Wilson Asset Management", Category = Models.Stock.AssetCategory.AustralianStocks };

            var arg = new Stock(Stock_ARG.Id);
            arg.List(Stock_ARG.AsxCode, Stock_ARG.Name, new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            _StockCache.Add(arg);

            arg.CorporateActions.Add(new CapitalReturn(ARG_CapitalReturn, arg, new Date(2001, 01, 01), "ARG Capital Return", new Date(2001, 01, 02), 10.00m));

            var argStockPrice = new StockPriceHistory(arg.Id);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 03), 1.01m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 04), 1.00m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 05), 1.03m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 06), 1.02m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 07), 1.01m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 10), 1.05m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 14), 1.07m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 17), 1.08m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 31), 1.09m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 02, 29), 1.10m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 03, 31), 1.07m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 04, 28), 1.07m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 05, 25), 1.03m);
            argStockPrice.UpdateClosingPrice(new Date(2000, 12, 29), 1.04m);
            argStockPrice.UpdateClosingPrice(new Date(2001, 01, 01), 1.05m);
            argStockPrice.UpdateClosingPrice(new Date(2001, 12, 31), 1.01m);
            argStockPrice.UpdateClosingPrice(new Date(2002, 12, 31), 0.99m);
            argStockPrice.UpdateClosingPrice(new Date(2003, 12, 31), 1.29m);
            argStockPrice.UpdateClosingPrice(new Date(2003, 05, 23), 1.40m);
            argStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            argStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.70m);
            argStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 2.00m);
            _StockPriceRetriever.StockPrices.Add(Stock_ARG.Id, argStockPrice);

            var wam = new Stock(Stock_WAM.Id);
            wam.List(Stock_WAM.AsxCode, Stock_WAM.Name, new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            _StockCache.Add(wam);

            wam.CorporateActions.Add(new SplitConsolidation(WAM_Split, wam, new Date(2002, 01, 01), "WAM Split", 1, 2));

            var wamStockPrice = new StockPriceHistory(wam.Id);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.20m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 03), 1.21m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 04), 1.20m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 05), 1.23m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 06), 1.22m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 07), 1.21m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 10), 1.25m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 14), 1.24m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 17), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 31), 1.28m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 02, 29), 1.29m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 03, 31), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 04, 28), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 05, 25), 1.23m);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 12, 29), 1.14m);
            wamStockPrice.UpdateClosingPrice(new Date(2001, 01, 01), 1.15m);
            wamStockPrice.UpdateClosingPrice(new Date(2001, 12, 31), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2002, 12, 31), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2003, 12, 31), 1.27m);
            wamStockPrice.UpdateClosingPrice(new Date(2003, 05, 23), 1.40m);
            wamStockPrice.UpdateClosingPrice(new Date(2005, 01, 02), 1.10m);
            wamStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            wamStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.30m);
            wamStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 1.50m);
            _StockPriceRetriever.StockPrices.Add(Stock_WAM.Id, wamStockPrice);
        }

        public Portfolio CreateEmptyPortfolio()
        {
            var portfolio = PortfolioFactory.CreatePortfolio(Guid.NewGuid());
            portfolio.Create("Test", Guid.NewGuid());

            return portfolio;
        }

        public Portfolio CreateDefaultPortfolio()
        {
            var portfolio = CreateEmptyPortfolio();

            portfolio.MakeCashTransaction(new Date(2000, 01, 01), Domain.Transactions.BankAccountTransactionType.Deposit, 10000m, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2000, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_WAM.Id, new Date(2000, 01, 01), 200, 1.20m, 19.95m, true, "", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2002, 01, 01), Domain.Transactions.BankAccountTransactionType.Interest, 100m, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2003, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());
            portfolio.DisposeOfShares(Stock_ARG.Id, new Date(2004, 01, 01), 50, 1.02m, 19.95m, Domain.Utils.CgtCalculationMethod.MinimizeGain, true, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2005, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());

            portfolio.IncomeReceived(Stock_ARG.Id, new Date(2005, 01, 02), new Date(2005, 01, 02), 50.00m, 5.00m, 2.00m, 0.00m, 0.00m, 0.00m, true, "", Guid.NewGuid());
            portfolio.IncomeReceived(Stock_WAM.Id, new Date(2005, 01, 03), new Date(2005, 01, 03), 30.00m, 3.00m, 2.00m, 0.00m, 0.00m, 0.50m, false, "", Guid.NewGuid());
            portfolio.AddOpeningBalance(Stock_WAM.Id, new Date(2005, 01, 03), new Date(2005, 01, 03), 5, 32.50m, "", WAM_OpeningBalance);
            portfolio.IncomeReceived(Stock_ARG.Id, new Date(2007, 01, 02), new Date(2007, 01, 02), 70.00m, 15.00m, 2.00m, 0.00m, 0.00m, 0.00m, true, "", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2008, 01, 01), Domain.Transactions.BankAccountTransactionType.Withdrawl, 5000m, "", Guid.NewGuid());
            portfolio.MakeCashTransaction(new Date(2009, 01, 01), Domain.Transactions.BankAccountTransactionType.Deposit, 500m, "", Guid.NewGuid());

            return portfolio;
        }

        private class StockPriceRetriever_ : IStockPriceRetriever
        {
            public Dictionary<Guid, IStockPriceHistory> StockPrices { get; } = new Dictionary<Guid, IStockPriceHistory>();
            public StockPrice GetLatestPrice(Guid stock)
            {
                var prices = StockPrices[stock];
                return new StockPrice(prices.LatestDate, prices.GetPrice(prices.LatestDate));
            }

            public decimal GetPrice(Guid stock, Date date)
            {
                var prices = StockPrices[stock];
                return prices.GetPrice(date);
            }

            public IEnumerable<StockPrice> GetPrices(Guid stock, DateRange dateRange)
            {
                var prices = StockPrices[stock];
                return prices.GetPrices(dateRange);
            }
        }
    }

    static class Services
    {
        public const string Collection = "Services";
    }


    [CollectionDefinition(Services.Collection)]
    public class ServicesCollection : ICollectionFixture<ServicesTestFixture>
    {
    }
}
