using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Web.Models.Portfolio;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IPortfolioIncomeService
    {
        ServiceResult<IncomeResponse> GetIncome(DateRange dateRange);
    }

    
    public class PortfolioIncomeService : IPortfolioIncomeService
    {
        private readonly IReadOnlyPortfolio _Portfolio;
        private readonly IStockMapper _Mapper;

        public PortfolioIncomeService(IReadOnlyPortfolio portfolio, IStockMapper mapper)
        {
            _Portfolio = portfolio;
            _Mapper = mapper;
        }

        public ServiceResult<IncomeResponse> GetIncome(DateRange dateRange)
        {
            if (_Portfolio == null)
                return ServiceResult<IncomeResponse>.NotFound();
            
            
            var response = new IncomeResponse();

            var incomes = _Portfolio.Transactions.InDateRange(dateRange).OfType<Domain.Transactions.IncomeReceived>()
                .GroupBy(x => x.Stock,
                        x => x,
                        (key, result) => new IncomeResponse.IncomeItem()
                        {
                            Stock = key.ToSummaryResponse(dateRange.ToDate),
                            UnfrankedAmount = result.Sum(x => x.UnfrankedAmount),
                            FrankedAmount = result.Sum(x => x.FrankedAmount),
                            FrankingCredits = result.Sum(x => x.FrankingCredits),
                            NetIncome = result.Sum(x => x.CashIncome),
                            GrossIncome = result.Sum(x => x.TotalIncome)
                        });

            response.Income.AddRange(incomes);
          
            return ServiceResult<IncomeResponse>.Ok(response); 
        }
    } 
}
