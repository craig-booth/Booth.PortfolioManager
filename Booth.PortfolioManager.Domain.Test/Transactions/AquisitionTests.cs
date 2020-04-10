﻿using System;
using System.Collections.Generic;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain.Test.Transactions
{
    class AquisitionTests
    {
        [TestCase]
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

            var handler = new AquisitionHandler();

            Assert.That(() => handler.Apply(transaction, holding.Object, cashAccount.Object), Throws.ArgumentException);

            mockRepository.Verify();
        }

        [TestCase]
        public void StockNotActiveAtDate()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(2020, 01, 01), false, AssetCategory.AustralianStocks);

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2000, 01, 01),
                Stock = stock,
                Comment = "Test Aquisition",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 20.00m,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();           
            var cashAccount = mockRepository.Create<ICashAccount>();
            
            var handler = new AquisitionHandler();
            Assert.That(() => handler.Apply(transaction, holding.Object, cashAccount.Object), Throws.TypeOf(typeof(StockNotActive)));

            mockRepository.Verify();
        }

        [TestCase]
        public void NoCashTransactionNoTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Stock = stock,
                Comment = "Test Aquisition",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 0.00m,
                CreateCashTransaction = false
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 01, 01), 100, 1000.00m, 1000.00m, transaction)).Returns(default(IParcel)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new AquisitionHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoCashTransactionWithTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Stock = stock,
                Comment = "Test Aquisition",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 20.00m,
                CreateCashTransaction = false
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 01, 01), 100, 1020.00m, 1020.00m, transaction)).Returns(default(IParcel)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();

            var handler = new AquisitionHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [TestCase]
        public void WithCashTransactionNoTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Stock = stock,
                Comment = "Test Aquisition",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 0.00m,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 01, 01), 100, 1000.00m, 1000.00m, transaction)).Returns(default(IParcel)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 01, 01), -1000.00m, "Purchase of ABC")).Verifiable();

            var handler = new AquisitionHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

        [TestCase]
        public void WithCashTransactionWithTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", Date.MinValue, false, AssetCategory.AustralianStocks);

            var transaction = new Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 01, 01),
                Stock = stock,
                Comment = "Test Aquisition",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 20.00m,
                CreateCashTransaction = true
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var holding = mockRepository.Create<IHolding>();
            holding.Setup(x => x.AddParcel(new Date(2020, 01, 01), new Date(2020, 01, 01), 100, 1020.00m, 1020.00m, transaction)).Returns(default(IParcel)).Verifiable();

            var cashAccount = mockRepository.Create<ICashAccount>();
            cashAccount.Setup(x => x.Transfer(new Date(2020, 01, 01), -1000.00m, "Purchase of ABC")).Verifiable();
            cashAccount.Setup(x => x.FeeDeducted(new Date(2020, 01, 01), 20.00m, "Brokerage for purchase of ABC")).Verifiable();

            var handler = new AquisitionHandler();
            handler.Apply(transaction, holding.Object, cashAccount.Object);

            mockRepository.Verify();
        }

    }
}
