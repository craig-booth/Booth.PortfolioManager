using System;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Services
{
    static class PortfolioTestCreator
    {

        public static Guid ArgId = Guid.NewGuid();
        public static Guid WamId = Guid.NewGuid();

        public static Portfolio CreatePortfolio()
        {
            var stockCache = new EntityCache<Stock>();

            var arg = new Stock(ArgId);
            arg.List("ARG", "Argo", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(arg);

            var argStockPrice = new StockPriceHistory(ArgId);
            arg.SetPriceHistory(argStockPrice);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            argStockPrice.UpdateClosingPrice(new Date(2005, 01, 02), 1.10m);
            argStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            argStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.70m);
            argStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 2.00m);

            var wam = new Stock(WamId);
            wam.List("WAM", "Wilson Asset Management", new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(wam);

            var wamStockPrice = new StockPriceHistory(WamId);
            wam.SetPriceHistory(wamStockPrice);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.20m);
            wamStockPrice.UpdateClosingPrice(new Date(2005, 01, 02), 1.10m);
            wamStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            wamStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.30m);
            wamStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 1.50m);

            var stockResolver = new StockResolver(stockCache);
            var portfolioFactory = new PortfolioFactory(stockResolver);
            var portfolio = portfolioFactory.CreatePortfolio(Guid.NewGuid(), "Test", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2000, 01, 01), Domain.Transactions.BankAccountTransactionType.Deposit, 10000m, "", Guid.NewGuid());
            portfolio.AquireShares(ArgId, new Date(2000, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());
            portfolio.AquireShares(WamId, new Date(2000, 01, 01), 200, 1.20m, 19.95m, true, "", Guid.NewGuid());

            return portfolio;
        }


    }
}
