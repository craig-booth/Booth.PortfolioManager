using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;
using FluentAssertions;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class PortfolioReturnCalculationTests
    {
        [Fact]
        public void CalculateIRR()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2010, 01, 01));

            var stockId1 = Guid.NewGuid();
            var stock1 = mockRepository.Create<IReadOnlyStock>();
            stock1.Setup(x => x.Id).Returns(stockId1);
            var holding1 = mockRepository.Create<IReadOnlyHolding>();
            holding1.Setup(x => x.Stock).Returns(stock1.Object);
            holding1.Setup(x => x.Properties[dateRange.FromDate]).Returns(new HoldingProperties(100, 100m, 100m));

            var stockId2 = Guid.NewGuid();
            var stock2 = mockRepository.Create<IReadOnlyStock>();
            stock2.Setup(x => x.Id).Returns(stockId2);
            var holding2 = mockRepository.Create<IReadOnlyHolding>();
            holding2.Setup(x => x.Stock).Returns(stock2.Object);
            holding2.Setup(x => x.Properties[dateRange.FromDate]).Returns(new HoldingProperties(200, 200m, 200m));
            holding2.Setup(x => x.Properties[dateRange.ToDate]).Returns(new HoldingProperties(200, 200m, 200m));

            var stockId3 = Guid.NewGuid();
            var stock3 = mockRepository.Create<IReadOnlyStock>();
            stock3.Setup(x => x.Id).Returns(stockId3);
            var holding3 = mockRepository.Create<IReadOnlyHolding>();
            holding3.Setup(x => x.Stock).Returns(stock3.Object);
            holding3.Setup(x => x.Properties[dateRange.ToDate]).Returns(new HoldingProperties(50, 100m, 100m));

            var initialHoldings = new IReadOnlyHolding[] { holding1.Object, holding2.Object };

            var finalHoldings = new IReadOnlyHolding[] { holding2.Object, holding3.Object };

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x.All(dateRange.FromDate)).Returns(initialHoldings);
            holdings.Setup(x => x.All(dateRange.ToDate)).Returns(finalHoldings);

            var priceRetriever = mockRepository.Create<IStockPriceRetriever>();
            priceRetriever.Setup(x => x.GetPrice(stockId1, dateRange.FromDate)).Returns(10.0m);
            priceRetriever.Setup(x => x.GetPrice(stockId2, dateRange.FromDate)).Returns(10.0m);
            priceRetriever.Setup(x => x.GetPrice(stockId2, dateRange.ToDate)).Returns(15.0m);
            priceRetriever.Setup(x => x.GetPrice(stockId3, dateRange.ToDate)).Returns(80.0m);

            var transactions = new List<CashAccountTransaction>();
            transactions.Add(new CashAccountTransaction(Guid.NewGuid(), new Date(2001, 01, 01), "", 200.00m, BankAccountTransactionType.Deposit, 300.00m));
            transactions.Add(new CashAccountTransaction(Guid.NewGuid(), new Date(2002, 01, 01), "", -100.00m, BankAccountTransactionType.Withdrawl, 200.00m));
            transactions.Add(new CashAccountTransaction(Guid.NewGuid(), new Date(2004, 01, 01), "", 200.00m, BankAccountTransactionType.Deposit, 500.00m));
            transactions.Add(new CashAccountTransaction(Guid.NewGuid(), new Date(2005, 01, 01), "", -450.00m, BankAccountTransactionType.Withdrawl, 50.00m));

            var transactionRange = mockRepository.Create<ITransactionRange<CashAccountTransaction>>();
            transactionRange.Setup(x => x.GetEnumerator()).Returns(transactions.GetEnumerator());

            var transactionList = mockRepository.Create<ITransactionList<CashAccountTransaction>>();
            transactionList.Setup(x => x.InDateRange(new DateRange(dateRange.FromDate.AddDays(1), dateRange.ToDate))).Returns(transactionRange.Object);
            

            var cashAccount = mockRepository.Create<IReadOnlyCashAccount>();
            cashAccount.Setup(x => x.Balance(dateRange.FromDate)).Returns(100.00m);
            cashAccount.Setup(x => x.Balance(dateRange.ToDate)).Returns(50.00m);
            cashAccount.Setup(x => x.Transactions).Returns(transactionList.Object);

            var portfolio = mockRepository.Create<IPortfolio>();
            portfolio.Setup(x => x.Holdings).Returns(holdings.Object);
            portfolio.Setup(x => x.CashAccount).Returns(cashAccount.Object);

            var calculator = new IrrReturnCalculator(priceRetriever.Object);
            var irr = calculator.Calculate(portfolio.Object, dateRange);

            irr.Should().Be(0.08745m);

            mockRepository.Verify();
        }

    }
}