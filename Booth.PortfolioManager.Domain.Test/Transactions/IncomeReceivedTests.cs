﻿using System;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    public class IncomeReceivedTests
    {
        [Fact]
        public void IncorrectTransactionType()
        {
            var transaction = new CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Comment = "Test Deposit",
                CashTransactionType = BankAccountTransactionType.Deposit,
                Amount = 100.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new IncomeReceivedHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);
a.Should().Throw<ArgumentException>();

            mockRepository.Verify();
        }

        [Fact]
        public void NoSharesOwned()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DrpCashBalance = 0.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(false);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new IncomeReceivedHandler();

            Action a = () => handler.Apply(transaction, holding.Object, cashAccount.Object);

            a.Should().Throw<NoSharesOwnedException>();

            mockRepository.Verify();
        }

        [Fact]
        public void DoNotCreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DrpCashBalance = 0.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new IncomeReceivedHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void CreateCashTransaction()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = true,
                DrpCashBalance = 0.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);


            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 02, 01), 70.00m, "Distribution for ABC")).Verifiable();

            var handler = new IncomeReceivedHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void OutstandingDrpBalance()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DrpCashBalance = 50.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(100.00m);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);
            holding.Setup(x => x.AddDrpAccountAmount(new Date(2020, 02, 01), -50.00m)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new IncomeReceivedHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [Fact]
        public void TaxDeferredAmount()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 50.00m,
                CreateCashTransaction = false,
                DrpCashBalance = 0.00m
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var drpAccount = mockRepository.Create<ICashAccount>();
            drpAccount.Setup(x => x.Balance(new Date(2020, 02, 01))).Returns(0.00m);

            var parcel1 = mockRepository.Create<IParcel>();
            parcel1.Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(100, 1000.00m, 1000.00m));
            parcel1.Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 14.29m, transaction)).Verifiable();
            var parcel2 = mockRepository.Create<IParcel>();
            parcel2.Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(50, 500.00m, 500.00m));
            parcel2.Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 7.14m, transaction)).Verifiable();
            var parcel3 = mockRepository.Create<IParcel>();
            parcel3.Setup(x => x.Properties[new Date(2020, 01, 01)]).Returns(new ParcelProperties(200, 1000.00m, 1000.00m));
            parcel3.Setup(x => x.Change(new Date(2020, 01, 01), 0, 0.00m, 28.57m, transaction)).Verifiable(); 

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.IsEffectiveAt(new Date(2020, 01, 01))).Returns(true);
            holding.Setup(x => x.DrpAccount).Returns(drpAccount.Object);
            holding.Setup(x => x.Parcels(new Date(2020, 01, 01))).Returns(new IParcel[] { parcel1.Object, parcel2.Object, parcel3.Object });

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new IncomeReceivedHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

    }
}
