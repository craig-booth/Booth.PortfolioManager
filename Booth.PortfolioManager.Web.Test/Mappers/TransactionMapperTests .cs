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
using Booth.PortfolioManager.RestApi.Stocks;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.RestApi.Transactions;

namespace Booth.PortfolioManager.Web.Test.Mappers
{
    public class TransactionMapperTests
    {


        [Fact]
        public void AquisitionToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var aquisition = new Domain.Transactions.Aquisition()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                Units = 10,
                AveragePrice= 1.20m,
                TransactionCosts = 19.95m,
                CreateCashTransaction = false
            };

            var response = aquisition.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = aquisition.Id,
                Type = RestApi.Transactions.TransactionType.Aquisition,
                Stock = stock.Object.Id,
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
        public void CashTransactionToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var cashTransaction = new Domain.Transactions.CashTransaction()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Comment = "Test",
                CashTransactionType = BankAccountTransactionType.Withdrawl,
                Amount = 12.00m,
            };

            var response = cashTransaction.ToResponse();

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
        public void CostBaseAdjustmentToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var costBaseAdjustment = new Domain.Transactions.CostBaseAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                Percentage = 0.35m
            };

            var response = costBaseAdjustment.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = costBaseAdjustment.Id,
                Type = RestApi.Transactions.TransactionType.CostBaseAdjustment,
                Stock = stock.Object.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Adjust cost base by 35.00%",
                Percentage = 0.35m
            });
        }

        [Fact]
        public void DisposalToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var disposal = new Domain.Transactions.Disposal()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                Units = 10,
                AveragePrice = 1.20m,
                TransactionCosts = 19.95m,
                CgtMethod = Domain.Utils.CgtCalculationMethod.MinimizeGain,
                CreateCashTransaction = true
            };

            var response = disposal.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = disposal.Id,
                Type = RestApi.Transactions.TransactionType.Disposal,
                Stock = stock.Object.Id,
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
        public void IncomeReceivedToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var income = new Domain.Transactions.IncomeReceived()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
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

            var response = income.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = income.Id,
                Type = RestApi.Transactions.TransactionType.IncomeReceived,
                Stock = stock.Object.Id,
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
        public void OpeningBalanceToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var openingBalance = new Domain.Transactions.OpeningBalance()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            };

            var response = openingBalance.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = openingBalance.Id,
                Type = RestApi.Transactions.TransactionType.OpeningBalance,
                Stock = stock.Object.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Opening balance of 10 shares",
                Units = 10,
                AquisitionDate = new Date(2001, 01, 15),
                CostBase = 1095.45m
            });
        }

        [Fact]
        public void ReturnOfCapitalToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var returnOfCapital = new Domain.Transactions.ReturnOfCapital()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            };

            var response = returnOfCapital.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = returnOfCapital.Id,
                Type = RestApi.Transactions.TransactionType.ReturnOfCapital,
                Stock = stock.Object.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Return of capital $15.00",
                RecordDate = new Date(2001, 01, 15),
                Amount = 15.00m,
                CreateCashTransaction = true
            });
        }


        [Fact]
        public void UnitCountAdjustmentToResponse()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stock = mockRepository.Create<IReadOnlyStock>();
            stock.SetupGet(x => x.Id).Returns(Guid.NewGuid());

            var adjustment = new Domain.Transactions.UnitCountAdjustment()
            {
                Id = Guid.NewGuid(),
                Date = new Date(2001, 01, 01),
                Stock = stock.Object,
                Comment = "Test",
                NewUnits = 2,
                OriginalUnits = 3
            };

            var response = adjustment.ToResponse();

            response.Should().BeEquivalentTo(new
            {
                Id = adjustment.Id,
                Type = RestApi.Transactions.TransactionType.UnitCountAdjustment,
                Stock = stock.Object.Id,
                TransactionDate = new Date(2001, 01, 01),
                Comment = "Test",
                Description = "Adjust unit count using ratio 3:2",
                NewUnits = 2,
                OriginalUnits = 3
            });
        }

       
    }
}
