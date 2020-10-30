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

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    public class PortfolioReturnCalculationTests
    {
        [Fact]
        public void CalculateIRR()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var dateRange = new DateRange(new Date(2000, 01, 01), new Date(2010, 01, 01));

            var holding1 = mockRepository.Create<IReadOnlyHolding>();
            holding1.Setup(x => x.Value(dateRange.FromDate)).Returns(1000.00m);
            
            var holding2 = mockRepository.Create<IReadOnlyHolding>();
            holding2.Setup(x => x.Value(dateRange.FromDate)).Returns(2000.00m);
            holding2.Setup(x => x.Value(dateRange.ToDate)).Returns(3000.00m);

            var holding3 = mockRepository.Create<IReadOnlyHolding>();
            holding3.Setup(x => x.Value(dateRange.ToDate)).Returns(4000.00m);

            var initialHoldings = new IReadOnlyHolding[] { holding1.Object, holding2.Object };

            var finalHoldings = new IReadOnlyHolding[] { holding2.Object, holding3.Object };

            var holdings = mockRepository.Create<IHoldingCollection>();
            holdings.Setup(x => x.All(dateRange.FromDate)).Returns(initialHoldings);
            holdings.Setup(x => x.All(dateRange.ToDate)).Returns(finalHoldings);

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

            var irr = portfolio.Object.CalculateIRR(dateRange);

            irr.Should().Be(0.08745m);

            mockRepository.Verify();
        }

    }
}