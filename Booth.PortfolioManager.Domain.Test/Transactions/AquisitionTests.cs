using System;
using System.Linq;

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
        public void NoExistingHoldings()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();
            var handler = new AquisitionHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            var holding = holdings.Get(stock.Id);
            var parcels = holding.Parcels(new Date(2020, 01, 01)).ToList();
            Assert.Multiple(() =>
            {       
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));

                Assert.That(parcels, Has.Count.EqualTo(1));

                Assert.That(parcels[0].EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));
                Assert.That(parcels[0].AquisitionDate, Is.EqualTo(new Date(2020, 01, 01)));
                Assert.That(parcels[0].Properties.Values.Count(), Is.EqualTo(1));

                var properties = parcels[0].Properties[new Date(2020, 01, 01)];
                Assert.That(properties.Amount, Is.EqualTo(1020.00m));
                Assert.That(properties.CostBase, Is.EqualTo(1020.00m));
                Assert.That(properties.Units, Is.EqualTo(100));

                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));
            }); 
        }

        [TestCase]
        public void NoCashTransactionNoTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();
            var handler = new AquisitionHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            var holding = holdings.Get(stock.Id);
            var parcels = holding.Parcels(new Date(2020, 01, 01)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));

                Assert.That(parcels, Has.Count.EqualTo(1));

                Assert.That(parcels[0].EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));
                Assert.That(parcels[0].AquisitionDate, Is.EqualTo(new Date(2020, 01, 01)));
                Assert.That(parcels[0].Properties.Values.Count(), Is.EqualTo(1));

                var properties = parcels[0].Properties[new Date(2020, 01, 01)];
                Assert.That(properties.Amount, Is.EqualTo(1000.00m));
                Assert.That(properties.CostBase, Is.EqualTo(1000.00m));
                Assert.That(properties.Units, Is.EqualTo(100));

                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));
            });
        }

        [TestCase]
        public void NoCashTransactionWithTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();
            var handler = new AquisitionHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            var holding = holdings.Get(stock.Id);
            var parcels = holding.Parcels(new Date(2020, 01, 01)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));

                Assert.That(parcels, Has.Count.EqualTo(1));

                Assert.That(parcels[0].EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));
                Assert.That(parcels[0].AquisitionDate, Is.EqualTo(new Date(2020, 01, 01)));
                Assert.That(parcels[0].Properties.Values.Count(), Is.EqualTo(1));

                var properties = parcels[0].Properties[new Date(2020, 01, 01)];
                Assert.That(properties.Amount, Is.EqualTo(1020.00m));
                Assert.That(properties.CostBase, Is.EqualTo(1020.00m));
                Assert.That(properties.Units, Is.EqualTo(100));

                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));
            });
        }

        [TestCase]
        public void WithCashTransactionNoTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();
            var handler = new AquisitionHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            var holding = holdings.Get(stock.Id);
            var parcels = holding.Parcels(new Date(2020, 01, 01)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));

                Assert.That(parcels, Has.Count.EqualTo(1));

                Assert.That(parcels[0].EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));
                Assert.That(parcels[0].AquisitionDate, Is.EqualTo(new Date(2020, 01, 01)));
                Assert.That(parcels[0].Properties.Values.Count(), Is.EqualTo(1));

                var properties = parcels[0].Properties[new Date(2020, 01, 01)];
                Assert.That(properties.Amount, Is.EqualTo(1000.00m));
                Assert.That(properties.CostBase, Is.EqualTo(1000.00m));
                Assert.That(properties.Units, Is.EqualTo(100));

                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));
            });
        }

        [TestCase]
        public void WithCashTransactionWithTransactionCosts()
        {
            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();
            var handler = new AquisitionHandler(holdings, cashAccount);

            handler.ApplyTransaction(transaction);

            var holding = holdings.Get(stock.Id);
            var parcels = holding.Parcels(new Date(2020, 01, 01)).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(holding.EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));

                Assert.That(parcels, Has.Count.EqualTo(1));

                Assert.That(parcels[0].EffectivePeriod, Is.EqualTo(new DateRange(new Date(2020, 01, 01), Date.MaxValue)));
                Assert.That(parcels[0].AquisitionDate, Is.EqualTo(new Date(2020, 01, 01)));
                Assert.That(parcels[0].Properties.Values.Count(), Is.EqualTo(1));

                var properties = parcels[0].Properties[new Date(2020, 01, 01)];
                Assert.That(properties.Amount, Is.EqualTo(1020.00m));
                Assert.That(properties.CostBase, Is.EqualTo(1020.00m));
                Assert.That(properties.Units, Is.EqualTo(100));

                Assert.That(cashAccount.Transactions.Count, Is.EqualTo(0));
            });
        }

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

            var holdings = new HoldingCollection();
            var cashAccount = new CashAccount();

            var handler = new AquisitionHandler(holdings, cashAccount);

            Assert.That(() => handler.ApplyTransaction(transaction), Throws.ArgumentException);
        }

    }
}
