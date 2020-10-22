using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;


namespace Booth.PortfolioManager.Domain.Test.CorporateActions
{
    public class TransformationTests
    {
        [Fact]
        public void HasBeenAppliedNoResultStockNoTransactionsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { });

            var result = transformation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedNoResultStockNoDisposalsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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
                DrpCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction  });

            var result = transformation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedNoResultStock()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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
                CgtMethod = CgtCalculationMethod.FirstInFirstOut,
                CreateCashTransaction = true
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = transformation.HasBeenApplied(transactions.Object);

            result.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedResultStocksNoTransactionsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var resultStocks = new Transformation.ResultingStock[] {
                new Transformation.ResultingStock(stock2.Id, 1, 1, 10.00m, new Date(2020, 02, 01))
            };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock2.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { });

            var result = transformation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedResultStocksNoDisposalsAtImplementationDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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
                DrpCashBalance = 0.00m
            };

            var transactions = mockRepository.Create<IPortfolioTransactionList>();
            transactions.Setup(x => x.ForHolding(stock2.Id, transformation.ImplementationDate)).Returns(new IPortfolioTransaction[] { transaction });

            var result = transformation.HasBeenApplied(transactions.Object);

            result.Should().BeFalse();

            mockRepository.Verify();
        }

        [Fact]
        public void HasBeenAppliedResultStocks()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().BeTrue();

            mockRepository.Verify();
        }

        [Fact]
        public void NoParcelsAtRecordDate()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().BeEmpty();

            mockRepository.Verify();
        }

        [Fact]
        public void RolloverReliefNoResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, true, resultStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().BeEmpty();

            mockRepository.Verify();
        }

        [Fact]
        public void RolloverReliefNoResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<Disposal>();

                    if (first is Disposal disposal)
                    {
                        disposal.Date.Should().Be(transformation.ImplementationDate);
                        disposal.Stock.Should().Be(transformation.Stock);
                        disposal.Comment.Should().Be("Test Transformation");
                        disposal.Units.Should().Be(100);
                        disposal.AveragePrice.Should().Be(1.20m);
                        disposal.TransactionCosts.Should().Be(0.00m);
                        disposal.CreateCashTransaction.Should().BeTrue();
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void RolloverReliefResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<OpeningBalance>();
                    if (first is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(160);
                        openingBalance.CostBase.Should().Be(320.00m);
                        openingBalance.AquisitionDate.Should().Be(parcel1.AquisitionDate);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();
                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(40);
                        openingBalance.CostBase.Should().Be(80.00m);
                        openingBalance.AquisitionDate.Should().Be(parcel2.AquisitionDate);
                    }
                },
                third =>
                {
                    third.Should().BeOfType<CostBaseAdjustment>();
                    if (third is CostBaseAdjustment costBaseAdjustment)
                    {
                        costBaseAdjustment.Date.Should().Be(transformation.ImplementationDate);
                        costBaseAdjustment.Stock.Should().Be(stock);
                        costBaseAdjustment.Comment.Should().Be("Test Transformation");
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void RolloverReliefResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<OpeningBalance>();
                    if (first is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(160);
                        openingBalance.CostBase.Should().Be(320.00m);
                        openingBalance.AquisitionDate.Should().Be(parcel1.AquisitionDate);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<OpeningBalance>();
                    if (second is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(40);
                        openingBalance.CostBase.Should().Be(80.00m);
                        openingBalance.AquisitionDate.Should().Be(parcel2.AquisitionDate);
                    }
                },
                third =>
                {
                    third.Should().BeOfType<CostBaseAdjustment>();
                    if (third is CostBaseAdjustment costBaseAdjustment)
                    {
                        costBaseAdjustment.Date.Should().Be(transformation.ImplementationDate);
                        costBaseAdjustment.Stock.Should().Be(stock);
                        costBaseAdjustment.Comment.Should().Be("Test Transformation");
                    }
                },
                fourth =>
                {
                    fourth.Should().BeOfType<Disposal>();
                    if (fourth is Disposal disposal)
                    {
                        disposal.Date.Should().Be(transformation.ImplementationDate);
                        disposal.Stock.Should().Be(stock);
                        disposal.Comment.Should().Be("Test Transformation");
                        disposal.Units.Should().Be(100);
                        disposal.AveragePrice.Should().Be(1.20m);
                        disposal.TransactionCosts.Should().Be(.00m);
                        disposal.CreateCashTransaction.Should().BeTrue();
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void NoRolloverReliefNoResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var resultStocks = new Transformation.ResultingStock[] { };
            var transformation = new Transformation(Guid.NewGuid(), stock, new Date(2020, 01, 01), "Test Transformation", new Date(2020, 02, 01), 0.00m, false, resultStocks);

            var holding = mockRepository.Create<IReadOnlyHolding>();
            holding.Setup(x => x.Properties[transformation.Date]).Returns(new HoldingProperties(0, 0.00m, 0.00m));

            var result = transformation.GetTransactionList(holding.Object, stockResolver.Object).ToList();

            result.Should().BeEmpty();

            mockRepository.Verify();
        }

        [Fact]
        public void NoRolloverReliefNoResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(         
                first =>
                {
                    first.Should().BeOfType<Disposal>();
                    if (first is Disposal disposal)
                    {
                        disposal.Date.Should().Be(transformation.ImplementationDate);
                        disposal.Stock.Should().Be(transformation.Stock);
                        disposal.Comment.Should().Be("Test Transformation");
                        disposal.Units.Should().Be(100);
                        disposal.AveragePrice.Should().Be(1.20m);
                        disposal.TransactionCosts.Should().Be(.00m);
                        disposal.CreateCashTransaction.Should().BeTrue();
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void NoRolloverReliefResultStocksNoCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<OpeningBalance>();
                    if (first is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(200);
                        openingBalance.CostBase.Should().Be(400.00m);
                        openingBalance.AquisitionDate.Should().Be(transformation.ImplementationDate);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<ReturnOfCapital>();
                    if (second is ReturnOfCapital returnOfCapital)
                    {
                        returnOfCapital.Date.Should().Be(transformation.ImplementationDate);
                        returnOfCapital.Stock.Should().Be(stock);
                        returnOfCapital.Comment.Should().Be("Test Transformation");
                        returnOfCapital.RecordDate.Should().Be(transformation.Date);
                        returnOfCapital.Amount.Should().Be(400.00m);
                        returnOfCapital.CreateCashTransaction.Should().BeFalse();
                    }
                }
            );

            mockRepository.Verify();
        }

        [Fact]
        public void NoRolloverReliefResultStocksCashComponent()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());
            stock.List("ABC", "ABC Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

            var stock2 = new Stock(Guid.NewGuid());
            stock2.List("XYZ", "XYZ Pty Ltd", new Date(1974, 01, 01), false, AssetCategory.AustralianStocks);

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

            result.Should().SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<OpeningBalance>();
                    if (first is OpeningBalance openingBalance)
                    {
                        openingBalance.Date.Should().Be(transformation.ImplementationDate);
                        openingBalance.Stock.Should().Be(stock2);
                        openingBalance.Comment.Should().Be("Test Transformation");
                        openingBalance.Units.Should().Be(200);
                        openingBalance.CostBase.Should().Be(400.00m);
                        openingBalance.AquisitionDate.Should().Be(transformation.ImplementationDate);
                    }
                },
                second =>
                {
                    second.Should().BeOfType<ReturnOfCapital>();
                    if (second is ReturnOfCapital returnOfCapital)
                    {
                        returnOfCapital.Date.Should().Be(transformation.ImplementationDate);
                        returnOfCapital.Stock.Should().Be(stock);
                        returnOfCapital.Comment.Should().Be("Test Transformation");
                        returnOfCapital.RecordDate.Should().Be(transformation.Date);
                        returnOfCapital.Amount.Should().Be(400.00m);
                        returnOfCapital.CreateCashTransaction.Should().BeFalse();
                    }
                }, 
                third =>
                {
                    third.Should().BeOfType<Disposal>();
                    if (third is Disposal disposal)
                    {
                        disposal.Date.Should().Be(transformation.ImplementationDate);
                        disposal.Stock.Should().Be(stock);
                        disposal.Comment.Should().Be("Test Transformation");
                        disposal.Units.Should().Be(100);
                        disposal.AveragePrice.Should().Be(1.20m);
                        disposal.TransactionCosts.Should().Be(0.00m);
                        disposal.CreateCashTransaction.Should().BeTrue();
                    }
                }
            );

            mockRepository.Verify();
        }
    }
}
