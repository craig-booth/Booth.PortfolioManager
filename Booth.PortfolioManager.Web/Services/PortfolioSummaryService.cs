using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.RestApi.Portfolios;
using Booth.PortfolioManager.Web.Mappers;

namespace Booth.PortfolioManager.Web.Services
{
    public interface IPortfolioSummaryService
    {
        ServiceResult<PortfolioSummaryResponse> GetSummary(Date date);
    }

    public class PortfolioSummaryService : IPortfolioSummaryService
    {
        private readonly IReadOnlyPortfolio _Portfolio;

        public PortfolioSummaryService(IReadOnlyPortfolio portfolio)
        {
            _Portfolio = portfolio;
        }

        public ServiceResult<PortfolioSummaryResponse> GetSummary(Date date)
        {
            if (_Portfolio == null)
                return ServiceResult<PortfolioSummaryResponse>.NotFound();

            var response = new PortfolioSummaryResponse();
            response.Holdings.AddRange(_Portfolio.Holdings.All(date).Select(x => x.ToResponse(date)));
            response.CashBalance = _Portfolio.CashAccount.Balance(date);
            response.PortfolioValue = response.Holdings.Sum(x => x.Value) + response.CashBalance;
            response.PortfolioCost = response.Holdings.Sum(x => x.Cost) + response.CashBalance; 

            response.Return1Year = null;
            response.Return3Year = null;
            response.Return5Year = null;
            response.ReturnAll = null;
            if (_Portfolio.StartDate != Date.MinValue)
            {
                var fromDate = date.AddYears(-1).AddDays(1);
                if (fromDate >= _Portfolio.StartDate)
                    response.Return1Year = _Portfolio.CalculateIRR(new DateRange(fromDate, date));

                fromDate = date.AddYears(-3).AddDays(1);
                if (fromDate >= _Portfolio.StartDate)
                    response.Return3Year = _Portfolio.CalculateIRR(new DateRange(fromDate, date));

                fromDate = date.AddYears(-5).AddDays(1);
                if (fromDate >= _Portfolio.StartDate)
                    response.Return5Year = _Portfolio.CalculateIRR(new DateRange(fromDate, date));

                if (date >= _Portfolio.StartDate)
                    response.ReturnAll = _Portfolio.CalculateIRR(new DateRange(_Portfolio.StartDate, date));
            } 

            return ServiceResult<PortfolioSummaryResponse>.Ok(response); 

        }
    } 
}
