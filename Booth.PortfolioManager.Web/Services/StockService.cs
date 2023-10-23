using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Booth.Common;
using Booth.PortfolioManager.Repository;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Web.Utilities;

namespace Booth.PortfolioManager.Web.Services
{

    public interface IStockService
    {
        ServiceResult ListStock(Guid id, string asxCode, string name, Date listingDate, bool trust, AssetCategory category);
        ServiceResult DelistStock(Guid id, Date date);
        ServiceResult ChangeStock(Guid id, Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory);
        ServiceResult UpdateCurrentPrice(Guid id, decimal price);
        ServiceResult UpdateClosingPrices(Guid id, IEnumerable<StockPrice> closingPrices);
        ServiceResult ChangeDividendRules(Guid id, Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod);
    } 
    
    class StockService : IStockService
    {
        private IStockQuery _StockQuery;
        private IStockRepository _StockRepository;

        private IEntityCache<StockPriceHistory> _StockPriceHistoryCache;
        private IStockPriceRepository _StockPriceHistoryRepository;
  
        public StockService(IStockQuery stockQuery, IStockRepository stockRepository, IEntityCache<StockPriceHistory> stockPriceHistoryCache, IStockPriceRepository stockPriceHistoryRepository)
        {
            _StockQuery = stockQuery;
            _StockRepository = stockRepository;
            _StockPriceHistoryCache = stockPriceHistoryCache;
            _StockPriceHistoryRepository = stockPriceHistoryRepository;
        }

        public ServiceResult ListStock(Guid id, string asxCode, string name, Date listingDate, bool trust, AssetCategory category)
        {
            // Check that id is unique
            var stock = _StockRepository.Get(id);
            if (stock != null)
                return ServiceResult.Error("A stock with id {0} already exists", id);

            // Check if stock already exists with this code
            var effectivePeriod = new DateRange(listingDate, Date.MaxValue);
            if (_StockQuery.Find(effectivePeriod, y => y.AsxCode == asxCode).Any())
                return ServiceResult.Error("A stock already exists with the code {0} on {1}", asxCode, listingDate.ToShortDateString());

            stock = new Stock(id);
            stock.List(asxCode, name, listingDate, trust, category);
            _StockRepository.Add(stock);

            var stockPriceHistory = new StockPriceHistory(id);
            _StockPriceHistoryRepository.Add(stockPriceHistory);
            _StockPriceHistoryCache.Add(stockPriceHistory);

            stock.SetPriceHistory(stockPriceHistory);

            return ServiceResult.Ok();
        }

        public ServiceResult ChangeStock(Guid id, Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return ServiceResult.NotFound();

            if (stock.EffectivePeriod.FromDate == Date.MinValue)
                return ServiceResult.Error("Stock is not listed");

            if (stock.EffectivePeriod.ToDate != Date.MaxValue)
                return ServiceResult.Error("Stock is delisted");

            var stockProperties = stock.Properties.Value(changeDate);
            if (stockProperties.EffectivePeriod.ToDate != Date.MaxValue)
                return ServiceResult.Error("A later change has been made on {0}", stockProperties.EffectivePeriod.ToDate.AddDays(1).ToShortDateString());

            if (stockProperties.Properties.AsxCode != newAsxCode)
            {
                if (_StockQuery.Get(newAsxCode, changeDate) != null)
                    return ServiceResult.Error("A stock already exists with code {0} on {1}", newAsxCode, changeDate.ToShortDateString());
            }

            stock.ChangeProperties(changeDate, newAsxCode, newName, newAssetCategory);
            _StockRepository.UpdateProperties(stock, changeDate); 

            return ServiceResult.Ok();
        }

        public ServiceResult DelistStock(Guid id, Date date)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return ServiceResult.NotFound();

            if (stock.EffectivePeriod.FromDate == Date.MinValue)
                return ServiceResult.Error("Stock is not listed");

            if (stock.EffectivePeriod.ToDate != Date.MaxValue)
                return ServiceResult.Error("Stock has already been delisted");

            stock.DeList(date);
            _StockRepository.Update(stock); 
            
            return ServiceResult.Ok();
        }

        public ServiceResult UpdateCurrentPrice(Guid id, decimal price)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return ServiceResult.NotFound();

            if (stock.EffectivePeriod.FromDate == Date.MinValue)
                return ServiceResult.Error("Stock is not listed");

            if (stock.EffectivePeriod.ToDate != Date.MaxValue)
                return ServiceResult.Error("Stock is delisted");

            if (price < 0.00m)
                return ServiceResult.Error("Closing price is negative");

            var stockPriceHistory = _StockPriceHistoryCache.Get(id);
            stockPriceHistory.UpdateCurrentPrice(price); 

            return ServiceResult.Ok();
        }

        public ServiceResult UpdateClosingPrices(Guid id, IEnumerable<StockPrice> closingPrices)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return ServiceResult.NotFound();

            if (stock.EffectivePeriod.FromDate == Date.MinValue)
                return ServiceResult.Error("Stock is not listed");

            var firstDate = Date.MaxValue;
            var lastDate = Date.MinValue;
            foreach (var closingPrice in closingPrices)
            {
                if (closingPrice.Price < 0.00m)
                    return ServiceResult.Error("Closing price on {0} is negative", closingPrice.Date.ToShortDateString());

                if (closingPrice.Date > stock.EffectivePeriod.ToDate)
                    return ServiceResult.Error("Stock is no listed on {0}", closingPrice.Date.ToShortDateString());

                if (closingPrice.Date < firstDate)
                    firstDate = closingPrice.Date;

                if (closingPrice.Date > lastDate)
                    lastDate = closingPrice.Date;
            }


            var stockPriceHistory = _StockPriceHistoryCache.Get(id);
            stockPriceHistory.UpdateClosingPrices(closingPrices);

            _StockPriceHistoryRepository.UpdatePrices(stockPriceHistory, new DateRange(firstDate, lastDate)); 

            return ServiceResult.Ok();
        }

        public ServiceResult ChangeDividendRules(Guid id, Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod)
        {
            var stock = _StockQuery.Get(id);
            if (stock == null)
                return ServiceResult.NotFound();

            if (!stock.IsEffectiveAt(changeDate))
                return ServiceResult.Error("Stock is not active at {0}", changeDate.ToShortDateString());

            if ((companyTaxRate < 0.00m) || (companyTaxRate >= 1.00m))
                return ServiceResult.Error("Company tax rate must be between 0 and 1");

            stock.ChangeDividendRules(changeDate, companyTaxRate, newDividendRoundingRule, drpActive, newDrpMethod);
            _StockRepository.UpdateDividendRules(stock, changeDate); 

            return ServiceResult.Ok();
        }

    } 
}
