using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    class TransformationTests
    {
        [TestCase]
        public void HasBeenAppliedNoResultStockNoTransactionsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedNoResultStockNoDisposalsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 10.00m, true, resultStocks);

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
                DRPCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction  });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedNoResultStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 10.00m, true, resultStocks);

            var transaction = new Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock,
                Comment = "Test Disposal",
                Units = 100,
                AveragePrice = 10.00m,
                TransactionCosts = 0.00m,
                CGTMethod = CGTCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedResultStocksNoTransactionsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 1, 10.00m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock2.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedResultStocksNoDisposalsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 1, 10.00m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 10.00m, true, resultStocks);

            var transaction = new IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock2,
                Comment = "Test Dividend",
                RecordDate = new Date(2020, 01, 01),
                FrankedAmount = 10.00m,
                UnfrankedAmount = 20.00m,
                FrankingCredits = 30.00m,
                Interest = 40.00m,
                TaxDeferred = 0.00m,
                CreateCashTransaction = false,
                DRPCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock2.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.False);

            mockRepository.Verify();
        }

        [TestCase]
        public void HasBeenAppliedResultStocks()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 1, 10.00m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 10.00m, true, resultStocks);

            var transaction = new OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2020, 02, 01),
                Stock = stock2,
                Comment = "Test Opening Balance",
                Units = 100,
                CostBase = 10.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock2.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = transformation.HasBeenApplied(transactions.Object);

            Assert.That(result, Is.True);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 1, 10.00m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 10.00m, true, resultStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void RolloverReliefNoResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void RolloverReliefNoResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 1.20m, true, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            if (result.Count >= 1)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result[0], Is.TypeOf(typeof(Disposal)));
                    var disposal = result[0] as Disposal;
                    Assert.That(disposal.Date, Is.EqualTo(transformation.ImplementationDate));
                    Assert.That(disposal.Stock, Is.EqualTo(transformation.Stock));
                    Assert.That(disposal.Comment, Is.EqualTo("Test Transformation"));
                    Assert.That(disposal.Units, Is.EqualTo(100));
                    Assert.That(disposal.AveragePrice, Is.EqualTo(1.20m));
                    Assert.That(disposal.TransactionCosts, Is.EqualTo(0.00m));
                    Assert.That(disposal.CreateCashTransaction, Is.EqualTo(true));
                });
            }

            mockRepository.Verify();
        }

        [TestCase]
        public void RolloverReliefResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() => {
                Assert.That(result, Has.Count.EqualTo(3));

                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(OpeningBalance)), "Transaction 1");
                    if (result[0] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 1");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 1");
                        Assert.That(openingBalance.Units, Is.EqualTo(160), "Transaction 1");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(320.00m), "Transaction 1");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(parcel1.AquisitionDate), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(40), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(80.00m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(parcel2.AquisitionDate), "Transaction 2");
                    }
                }

                if (result.Count >= 3)
                {
                    Assert.That(result[2], Is.TypeOf(typeof(CostBaseAdjustment)), "Transaction 3");
                    if (result[2] is CostBaseAdjustment costBaseAdjustment)
                    {
                        Assert.That(costBaseAdjustment.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 3");
                        Assert.That(costBaseAdjustment.Stock, Is.EqualTo(stock), "Transaction 3");
                        Assert.That(costBaseAdjustment.Comment, Is.EqualTo("Test Transformation"), "Transaction 3");
                        Assert.That(costBaseAdjustment.Percentage, Is.EqualTo(0.60m), "Transaction 3");
                    }
                }

            });

            mockRepository.Verify();
        }

        [TestCase]
        public void RolloverReliefResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 1.20m, true, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(4));

                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(OpeningBalance)), "Transaction 1");
                    if (result[0] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 1");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 1");
                        Assert.That(openingBalance.Units, Is.EqualTo(160), "Transaction 1");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(320.00m), "Transaction 1");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(parcel1.AquisitionDate), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(OpeningBalance)), "Transaction 2");
                    if (result[1] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 2");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 2");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 2");
                        Assert.That(openingBalance.Units, Is.EqualTo(40), "Transaction 2");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(80.00m), "Transaction 2");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(parcel2.AquisitionDate), "Transaction 2");
                    }
                }

                if (result.Count >= 3)
                {
                    Assert.That(result[2], Is.TypeOf(typeof(CostBaseAdjustment)), "Transaction 3");
                    if (result[2] is CostBaseAdjustment costBaseAdjustment)
                    {
                        Assert.That(costBaseAdjustment.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 3");
                        Assert.That(costBaseAdjustment.Stock, Is.EqualTo(stock), "Transaction 3");
                        Assert.That(costBaseAdjustment.Comment, Is.EqualTo("Test Transformation"), "Transaction 3");
                        Assert.That(costBaseAdjustment.Percentage, Is.EqualTo(0.60m), "Transaction 3");
                    }
                }

                if (result.Count >= 4)
                {
                    Assert.That(result[3], Is.TypeOf(typeof(Disposal)), "Transaction 4");
                    if (result[3] is Disposal disposal)
                    {
                        Assert.That(disposal.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 4");
                        Assert.That(disposal.Stock, Is.EqualTo(stock), "Transaction 4");
                        Assert.That(disposal.Comment, Is.EqualTo("Test Transformation"), "Transaction 4");
                        Assert.That(disposal.Units, Is.EqualTo(100), "Transaction 4");
                        Assert.That(disposal.AveragePrice, Is.EqualTo(1.20m), "Transaction 4");
                        Assert.That(disposal.TransactionCosts, Is.EqualTo(0.00m), "Transaction 4");
                        Assert.That(disposal.CreateCashTransaction, Is.EqualTo(true), "Transaction 4");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void NoRolloverReliefNoResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, false, resultStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.That(result, Is.Empty);

            mockRepository.Verify();
        }

        [TestCase]
        public void NoRolloverReliefNoResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 1.20m, false, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));

                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(Disposal)), "Transaction 1");
                    if (result[0] is Disposal disposal)
                    {
                        Assert.That(disposal.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                        Assert.That(disposal.Stock, Is.EqualTo(transformation.Stock), "Transaction 1");
                        Assert.That(disposal.Comment, Is.EqualTo("Test Transformation"), "Transaction 1");
                        Assert.That(disposal.Units, Is.EqualTo(100), "Transaction 1");
                        Assert.That(disposal.AveragePrice, Is.EqualTo(1.20m), "Transaction 1");
                        Assert.That(disposal.TransactionCosts, Is.EqualTo(0.00m), "Transaction 1");
                        Assert.That(disposal.CreateCashTransaction, Is.EqualTo(true), "Transaction 1");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void NoRolloverReliefResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, false, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));

                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(OpeningBalance)), "Transaction 1");
                    if (result[0] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 1");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 1");
                        Assert.That(openingBalance.Units, Is.EqualTo(200), "Transaction 1");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(400.00m), "Transaction 1");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(ReturnOfCapital)), "Transaction 2");
                    if (result[1] is ReturnOfCapital returnOfCapital)
                    {
                        Assert.That(returnOfCapital.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 2");
                        Assert.That(returnOfCapital.Stock, Is.EqualTo(stock), "Transaction 2");
                        Assert.That(returnOfCapital.Comment, Is.EqualTo("Test Transformation"), "Transaction 2");
                        Assert.That(returnOfCapital.RecordDate, Is.EqualTo(transformation.Date), "Transaction 2");
                        Assert.That(returnOfCapital.Amount, Is.EqualTo(400.00m), "Transaction 2");
                        Assert.That(returnOfCapital.CreateCashTransaction, Is.EqualTo(false), "Transaction 2");
                    }
                }
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void NoRolloverReliefResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);
            stockResolver.Setup(x => x.GetStock(stock2.Id)).Returns(stock2);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 2, 0.40m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 1.20m, false, resultStocks);

            var parcel1 = new Parcel(Guid.NewGuid(), new Date(2000, 01, 01), new Date(2000, 01, 01), new ParcelProperties(80, 800.00m, 800.00m), null);
            var parcel2 = new Parcel(Guid.NewGuid(), new Date(2010, 01, 01), new Date(2010, 01, 01), new ParcelProperties(20, 200.00m, 200.00m), null);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(100, 1000.00m, 1000.00m));
            holding.Setup(x => x.Parcels(transformation.Date)).Returns(new Parcel[] { parcel1, parcel2 });

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(3));

                if (result.Count >= 1)
                {
                    Assert.That(result[0], Is.TypeOf(typeof(OpeningBalance)), "Transaction 1");
                    if (result[0] is OpeningBalance openingBalance)
                    {
                        Assert.That(openingBalance.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                        Assert.That(openingBalance.Stock, Is.EqualTo(stock2), "Transaction 1");
                        Assert.That(openingBalance.Comment, Is.EqualTo("Test Transformation"), "Transaction 1");
                        Assert.That(openingBalance.Units, Is.EqualTo(200), "Transaction 1");
                        Assert.That(openingBalance.CostBase, Is.EqualTo(400.00m), "Transaction 1");
                        Assert.That(openingBalance.AquisitionDate, Is.EqualTo(transformation.ImplementationDate), "Transaction 1");
                    }
                }

                if (result.Count >= 2)
                {
                    Assert.That(result[1], Is.TypeOf(typeof(ReturnOfCapital)), "Transaction 2");
                    if (result[1] is ReturnOfCapital returnOfCapital)
                    {
                        Assert.That(returnOfCapital.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 2");
                        Assert.That(returnOfCapital.Stock, Is.EqualTo(stock), "Transaction 2");
                        Assert.That(returnOfCapital.Comment, Is.EqualTo("Test Transformation"), "Transaction 2");
                        Assert.That(returnOfCapital.RecordDate, Is.EqualTo(transformation.Date), "Transaction 2");
                        Assert.That(returnOfCapital.Amount, Is.EqualTo(400.00m), "Transaction 2");
                        Assert.That(returnOfCapital.CreateCashTransaction, Is.EqualTo(false), "Transaction 2");
                    }
                }

                if (result.Count >= 3)
                {
                    Assert.That(result[2], Is.TypeOf(typeof(Disposal)), "Transaction 3");
                    if (result[2] is Disposal disposal)
                    {
                        Assert.That(disposal.Date, Is.EqualTo(transformation.ImplementationDate), "Transaction 3");
                        Assert.That(disposal.Stock, Is.EqualTo(stock), "Transaction 3");
                        Assert.That(disposal.Comment, Is.EqualTo("Test Transformation"), "Transaction 3");
                        Assert.That(disposal.Units, Is.EqualTo(100), "Transaction 3");
                        Assert.That(disposal.AveragePrice, Is.EqualTo(1.20m), "Transaction 3");
                        Assert.That(disposal.TransactionCosts, Is.EqualTo(0.00m), "Transaction 3");
                        Assert.That(disposal.CreateCashTransaction, Is.EqualTo(true), "Transaction 3");
                    }
                }
            });

            mockRepository.Verify();
        }
    }
}
