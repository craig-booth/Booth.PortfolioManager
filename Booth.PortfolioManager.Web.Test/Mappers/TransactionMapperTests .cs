using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Mappers;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class TransactionMapperTests
    {


        [Fact]
        public void AquisitionToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var aquisition = new Domain.Transactions.Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AveragePrice= 1.20m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = false
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(aquisition);

            response.Should().BeEquivalentTo(new
            {
                Id = aquisition.Id,
                Type = RestApi.Transactions.TransactionType.Aquisition,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Aquired 10 shares @ $1.20",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = false
            });

        }

        [Fact]
        public void AquisitionFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var aquisition = new RestApi.Transactions.Aquisition()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Aquired 10 shares @ $1.20",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = false
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(aquisition);

            response.Should().BeEquivalentTo(new
            {
                Id = aquisition.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = false
            });
        }

        [Fact]
        public void CashTransactionToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var cashTransaction = new Domain.Transactions.CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Comment = "Test",
                CashTransactionType = BankAccountTransactionType.Withdrawl,
                Amount = 12.00m,
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(cashTransaction);

            response.Should().BeEquivalentTo(new
            {
                Id = cashTransaction.Id,
                Type = RestApi.Transactions.TransactionType.CashTransaction,
                Stock = Guid.Empty,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Withdrawl $12.00",
                CashTransactionType = RestApi.Transactions.CashTransactionType.Withdrawl,
                Amount = 12.00m
            });
        }

        [Fact]
        public void CashTransactionFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>();

            var cashTransaction = new RestApi.Transactions.CashTransaction()
            {
                Id = Guid.NewGuid(),
                Stock = Guid.Empty,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Withdrawl $12.00",
                CashTransactionType = RestApi.Transactions.CashTransactionType.Withdrawl,
                Amount = 12.00m
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(cashTransaction);

            response.Should().BeEquivalentTo(new
            {
                Id = cashTransaction.Id,
                Date = new Date(2001, 01, 01),
                Comment = "Test",
                CashTransactionType = BankAccountTransactionType.Withdrawl,
                Amount = 12.00m
            });
        }

        [Fact]
        public void CostBaseAdjustmentToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var costBaseAdjustment = new Domain.Transactions.CostBaseAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Percentage = 0.35m
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(costBaseAdjustment);

            response.Should().BeEquivalentTo(new
            {
                Id = costBaseAdjustment.Id,
                Type = RestApi.Transactions.TransactionType.CostBaseAdjustment,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = String.Format("Adjust cost base by {0:P2}", 0.35m),
                Percentage = 0.35m
            });
        }

        [Fact]
        public void CostBaseAdjustmentFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var costBaseAdjustment = new RestApi.Transactions.CostBaseAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = String.Format("Adjust cost base by {0:P2}", 0.35m),
                Percentage = 0.35m
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(costBaseAdjustment);

            response.Should().BeEquivalentTo(new
            {
                Id = costBaseAdjustment.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Percentage = 0.35m
            });
        }

        [Fact]
        public void DisposalToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var disposal = new Domain.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CgtMethod = Domain.Utils.CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(disposal);

            response.Should().BeEquivalentTo(new
            {
                Id = disposal.Id,
                Type = RestApi.Transactions.TransactionType.Disposal,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Disposed of 10 shares @ $1.20",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CgtMethod = RestApi.Transactions.CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            });
        }

        [Fact]
        public void DisposalFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var disposal = new RestApi.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Disposed of 10 shares @ $1.20",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CgtMethod = RestApi.Transactions.CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(disposal);

            response.Should().BeEquivalentTo(new
            {
                Id = disposal.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CgtMethod = Domain.Utils.CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            });
        }


        [Fact]
        public void IncomeReceivedToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var income = new Domain.Transactions.IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 2.00m,
                Interest = 3.00m,
                TaxDeferred = 5.00m,
                RecordDate = new Date(2001, 01, 15),
                DrpCashBalance = 1.30m,
                CreateCashTransaction = false
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(income);

            response.Should().BeEquivalentTo(new
            {
                Id = income.Id,
                Type = RestApi.Transactions.TransactionType.IncomeReceived,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Income received $29.00",
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 2.00m,
                Interest = 3.00m,
                TaxDeferred = 5.00m,
                RecordDate = new Date(2001, 01, 15),
                DrpCashBalance = 1.30m,
                CreateCashTransaction = false
            });
        }

        [Fact]
        public void IncomeReceivedFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var income = new RestApi.Transactions.IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Income received $29.00",
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 2.00m,
                Interest = 3.00m,
                TaxDeferred = 5.00m,
                RecordDate = new Date(2001, 01, 15),
                DrpCashBalance = 1.30m,
                CreateCashTransaction = false
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(income);

            response.Should().BeEquivalentTo(new
            {
                Id = income.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                FrankedAmount = 10.00m,
                UnfrankedAmount = 11.00m,
                FrankingCredits = 2.00m,
                Interest = 3.00m,
                TaxDeferred = 5.00m,
                RecordDate = new Date(2001, 01, 15),
                DrpCashBalance = 1.30m,
                CreateCashTransaction = false
            });
        }

        [Fact]
        public void OpeningBalanceToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var openingBalance = new Domain.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(openingBalance);

            response.Should().BeEquivalentTo(new
            {
                Id = openingBalance.Id,
                Type = RestApi.Transactions.TransactionType.OpeningBalance,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Opening balance of 10 shares",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            });
        }

        [Fact]
        public void OpeningBalanceFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var openingBalance = new RestApi.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Opening balance of 10 shares",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(openingBalance);

            response.Should().BeEquivalentTo(new
            {
                Id = openingBalance.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            });
        }

        [Fact]
        public void ReturnOfCapitalToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var returnOfCapital = new Domain.Transactions.ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(returnOfCapital);

            response.Should().BeEquivalentTo(new
            {
                Id = returnOfCapital.Id,
                Type = RestApi.Transactions.TransactionType.ReturnOfCapital,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Return of capital $15.00",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            });
        }

        [Fact]
        public void ReturnOfCapitalFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var returnOfCapital = new RestApi.Transactions.ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Return of capital $15.00",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(returnOfCapital);

            response.Should().BeEquivalentTo(new
            {
                Id = returnOfCapital.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            });
        }

        [Fact]
        public void UnitCountAdjustmentToApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var adjustment = new Domain.Transactions.UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                NewUnits = 2,
                OriginalUnits = 3
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.ToApi(adjustment);

            response.Should().BeEquivalentTo(new
            {
                Id = adjustment.Id,
                Type = RestApi.Transactions.TransactionType.UnitCountAdjustment,
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Adjust unit count using ratio 3:2",
                NewUnits = 2,
                OriginalUnits = 3
            });
        }

        [Fact]
        public void UnitCountAdjustmentFromApi()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = new Stock(Guid.NewGuid());

            var stockResolver = mockRepository.Create<IStockResolver>();
            stockResolver.Setup(x => x.GetStock(stock.Id)).Returns(stock);

            var adjustment = new RestApi.Transactions.UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Stock = stock.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Adjust unit count using ratio 3:2",
                NewUnits = 2,
                OriginalUnits = 3
            };

            var mapper = new TransactionMapper(stockResolver.Object);
            var response = mapper.FromApi(adjustment);

            response.Should().BeEquivalentTo(new
            {
                Id = adjustment.Id,
                Date = new Date(2001, 01, 01),
                Stock = stock,
                Comment = "Test",
                NewUnits = 2,
                OriginalUnits = 3
            });
        }


    }
}
