using System;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Test.Services
{
    static class PortfolioTestCreator
    {
        public static RestApi.Portfolios.Stock Stock_ARG = new RestApi.Portfolios.Stock() { Id = Guid.NewGuid(), AsxCode = "ARG", Name = "Argo", Category = RestApi.Stocks.AssetCategory.AustralianStocks };
        public static RestApi.Portfolios.Stock Stock_WAM = new RestApi.Portfolios.Stock() { Id = Guid.NewGuid(), AsxCode = "WAM", Name = "Wilson Asset Management", Category = RestApi.Stocks.AssetCategory.AustralianStocks };

        public static Portfolio CreatePortfolio()
        {
            var stockCache = new EntityCache<Stock>();

            var arg = new Stock(Stock_ARG.Id);
            arg.List(Stock_ARG.AsxCode, Stock_ARG.Name, new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(arg);

            var argStockPrice = new StockPriceHistory(arg.Id);
            arg.SetPriceHistory(argStockPrice);
            argStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.00m);
            argStockPrice.UpdateClosingPrice(new Date(2001, 01, 01), 1.05m);
            argStockPrice.UpdateClosingPrice(new Date(2005, 01, 02), 1.10m);
            argStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            argStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.70m);
            argStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 2.00m);

            var wam = new Stock(Stock_WAM.Id);
            wam.List(Stock_WAM.AsxCode, Stock_WAM.Name, new Date(2000, 01, 01), false, AssetCategory.AustralianStocks);
            stockCache.Add(wam);

            var wamStockPrice = new StockPriceHistory(wam.Id);
            wam.SetPriceHistory(wamStockPrice);
            wamStockPrice.UpdateClosingPrice(new Date(2000, 01, 01), 1.20m);
            wamStockPrice.UpdateClosingPrice(new Date(2001, 01, 01), 1.15m);
            wamStockPrice.UpdateClosingPrice(new Date(2005, 01, 02), 1.10m);
            wamStockPrice.UpdateClosingPrice(new Date(2007, 01, 02), 0.90m);
            wamStockPrice.UpdateClosingPrice(new Date(2009, 01, 02), 1.30m);
            wamStockPrice.UpdateClosingPrice(new Date(2010, 01, 01), 1.50m);

            var stockResolver = new StockResolver(stockCache);
            var portfolioFactory = new PortfolioFactory(stockResolver);
            var portfolio = portfolioFactory.CreatePortfolio(Guid.NewGuid(), "Test", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2000, 01, 01), Domain.Transactions.BankAccountTransactionType.Deposit, 10000m, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2000, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_WAM.Id, new Date(2000, 01, 01), 200, 1.20m, 19.95m, true, "", Guid.NewGuid());      

            portfolio.MakeCashTransaction(new Date(2002, 01, 01), Domain.Transactions.BankAccountTransactionType.Interest, 100m, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2003, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());
            portfolio.AquireShares(Stock_ARG.Id, new Date(2005, 01, 01), 100, 1.00m, 19.95m, true, "", Guid.NewGuid());

            portfolio.IncomeReceived(Stock_ARG.Id, new Date(2005, 01, 02), new Date(2005, 01, 02), 50.00m, 5.00m, 2.00m, 0.00m, 0.00m, 0.00m, true,"",  Guid.NewGuid());
            portfolio.IncomeReceived(Stock_WAM.Id, new Date(2005, 01, 03), new Date(2005, 01, 03), 30.00m, 3.00m, 2.00m, 0.00m, 0.00m, 0.50m, false, "", Guid.NewGuid());
            portfolio.AddOpeningBalance(Stock_WAM.Id, new Date(2005, 01, 03), new Date(2005, 01, 03), 5, 32.50m, "", Guid.NewGuid());
            portfolio.IncomeReceived(Stock_ARG.Id, new Date(2007, 01, 02), new Date(2007, 01, 02), 70.00m, 15.00m, 2.00m, 0.00m, 0.00m, 0.00m, true, "", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2009, 01, 01), Domain.Transactions.BankAccountTransactionType.Deposit, 500m, "", Guid.NewGuid());

            portfolio.MakeCashTransaction(new Date(2008, 01, 01), Domain.Transactions.BankAccountTransactionType.Withdrawl, 5000m, "", Guid.NewGuid());

            return portfolio;
        }


    }
}
