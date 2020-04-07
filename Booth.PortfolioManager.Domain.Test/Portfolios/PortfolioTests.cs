using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Stocks;

namespace Booth.PortfolioManager.Domain.Test.Portfolios
{
    class PortfolioTests
    {
        [TestCase]
        public void Create()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stockResolver = mockRepository.Create<IStockResolver>();

            var id = Guid.NewGuid();
            var portfolio = new Portfolio(id, stockResolver.Object);
            var owner = Guid.NewGuid();
            portfolio.Create("Test", owner);

            Assert.Multiple(() =>
            {
                Assert.That(portfolio.Id, Is.EqualTo(id));
                Assert.That(portfolio.Name, Is.EqualTo("Test"));
                Assert.That(portfolio.Owner, Is.EqualTo(owner));
            });

            mockRepository.Verify();
        }

        [TestCase]
        public void ChangeDrpParticipationHoldingNotOwned()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void ChangeDrpParticipation()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void AddOpeningBalance()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void AdjustUnitCount()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void AquireShares()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void DisposeOfShares()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void IncomeReceived()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void MakeCashTransaction()
        {
            Assert.Inconclusive();
        }

        [TestCase]
        public void ReturnOfCapitalReceived()
        {
            Assert.Inconclusive();
        }
    }
}
