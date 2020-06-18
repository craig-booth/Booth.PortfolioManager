using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.Common;
using Booth.EventStore;
using Booth.PortfolioManager.Web.Services;
using Booth.PortfolioManager.Domain.TradingCalanders;

namespace Booth.PortfolioManager.Web.Test.Services
{
    public class TradingCalanderServiceTests
    {

        [Fact]
        public void CreateServiceForExistingCalander()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalander = new TradingCalander(Guid.NewGuid());
            tradingCalander.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalander>>();
            repository.Setup(x => x.Get(tradingCalander.Id)).Returns(tradingCalander);

            var service = new TradingCalanderService(repository.Object, tradingCalander.Id);

            service.TradingCalander.Should().Be(tradingCalander);

            mockRepository.Verify();
        }

        [Fact]
        public void CreateServiceForNewCalander()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var id = Guid.NewGuid();
            var repository = mockRepository.Create<IRepository<TradingCalander>>();
            repository.Setup(x => x.Get(id)).Returns(default(TradingCalander));
            repository.Setup(x => x.Add(It.Is<TradingCalander>(y => y.Id == id)));

            var service = new TradingCalanderService(repository.Object, id);

            mockRepository.Verify();
        }

        [Fact]
        public void SetNonTradingDays()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);

            var tradingCalander = new TradingCalander(Guid.NewGuid());
            tradingCalander.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 01, 01), "New Year's Day") });
            var repository = mockRepository.Create<IRepository<TradingCalander>>();
            repository.Setup(x => x.Get(tradingCalander.Id)).Returns(tradingCalander);
            repository.Setup(x => x.Update(tradingCalander));

            var service = new TradingCalanderService(repository.Object, tradingCalander.Id);

            var calanderReference = service.TradingCalander;

            using (new AssertionScope())
            {
                calanderReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeFalse();
                calanderReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeTrue();
            }

            service.SetNonTradingDays(2000, new[] { new NonTradingDay(new Date(2000, 12, 25), "Christmas Day") });

            using (new AssertionScope())
            {
                calanderReference.IsTradingDay(new Date(2000, 01, 01)).Should().BeTrue();
                calanderReference.IsTradingDay(new Date(2000, 12, 25)).Should().BeFalse();
            }

            mockRepository.Verify();
        }

        /*
         * 
         *       private TradingCalander _TradingCalander;
        private IRepository<TradingCalander> _Repository;

        public ITradingCalander TradingCalander => _TradingCalander;

        public TradingCalanderService(IRepository<TradingCalander> repository, Guid calanderId)
        {
            _Repository = repository;
            _TradingCalander = _Repository.Get(calanderId);
        }

        public void SetNonTradingDays(int year, IEnumerable<NonTradingDay> nonTradingDays)
        {
            _TradingCalander.SetNonTradingDays(year, nonTradingDays);
            _Repository.Update(_TradingCalander);
        }

    */
    }
}
