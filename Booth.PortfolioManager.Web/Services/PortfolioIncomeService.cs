using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioIncomeService
    {
        ServiceResult<IncomeResponse> GetIncome(DateRange dateRange);
    }

    
    public class PortfolioIncomeService : IPortfolioIncomeService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioIncomeService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<IncomeResponse> GetIncome(DateRange dateRange)
        {/*
            var portfolio = _PortfolioCache.Get(portfolioId);

            var response = new IncomeResponse();

            var incomes = portfolio.Transactions.InDateRange(dateRange).OfType<IncomeReceived>()
                .GroupBy(x => x.Stock,
                        x => x,
                        (key, result) => new IncomeResponse.IncomeItem()
                        {
                            Stock = key.Convert(dateRange.ToDate),
                            UnfrankedAmount = result.Sum(x => x.UnfrankedAmount),
                            FrankedAmount = result.Sum(x => x.FrankedAmount),
                            FrankingCredits = result.Sum(x => x.FrankingCredits),
                            NettIncome = result.Sum(x => x.CashIncome),
                            GrossIncome = result.Sum(x => x.TotalIncome)
                        });

            response.Income.AddRange(incomes);

            return response; */
            throw new NotSupportedException();
        }
    } 
}
