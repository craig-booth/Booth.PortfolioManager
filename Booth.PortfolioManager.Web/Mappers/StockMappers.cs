using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Booth.Common;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.RestApi.Stocks;

namespace Booth.PortfolioManager.Web.Mappers
{
    static class StockMappers
    {

        public static StockResponse ToResponse(this Stock stock, Date date)
        {
            var stockProperties = stock.Properties.ClosestTo(date);
            var dividendRules = stock.DividendRules.ClosestTo(date);

            var result = new StockResponse()
            {
                Id = stock.Id,
                AsxCode = stockProperties.AsxCode,
                Name = stockProperties.Name,
                ListingDate = stock.EffectivePeriod.FromDate,
                Category = stockProperties.Category.ToResponse(),
                Trust = stock.Trust,
                StapledSecurity = false,
                DelistedDate = stock.EffectivePeriod.ToDate,
                LastPrice = stock.GetPrice(Date.Today),
                CompanyTaxRate = dividendRules.CompanyTaxRate,
                DividendRoundingRule = dividendRules.DividendRoundingRule,
                DrpActive = dividendRules.DrpActive,
                DrpMethod = dividendRules.DrpMethod.ToResponse()
            };

            return result;
        }

        public static StockHistoryResponse ToHistoryResponse(this Stock stock)
        {
            var stockProperties = stock.Properties.ClosestTo(Date.Today);

            var result = new StockHistoryResponse()
            {
                Id = stock.Id,
                AsxCode = stockProperties.AsxCode,
                Name = stockProperties.Name,
                ListingDate = stock.EffectivePeriod.FromDate,
                DelistedDate = stock.EffectivePeriod.ToDate,
            };

            foreach (var property in stock.Properties.Values.Reverse())
                result.AddHistory(property.EffectivePeriod.FromDate, property.EffectivePeriod.ToDate, property.Properties.AsxCode, property.Properties.Name, property.Properties.Category.ToResponse());

            foreach (var rules in stock.DividendRules.Values.Reverse())
                result.AddDividendRules(rules.EffectivePeriod.FromDate, rules.EffectivePeriod.ToDate, rules.Properties.CompanyTaxRate, rules.Properties.DividendRoundingRule, rules.Properties.DrpActive, rules.Properties.DrpMethod.ToResponse());

            return result;
        }

        public static StockPriceResponse ToPriceResponse(this Stock stock, DateRange dateRange)
        {
            var stockProperties = stock.Properties.ClosestTo(dateRange.ToDate);

            var result = new StockPriceResponse()
            {
                Id = stock.Id,
                AsxCode = stockProperties.AsxCode,
                Name = stockProperties.Name
            };

            foreach (var price in stock.GetPrices(dateRange))
                result.AddClosingPrice(price.Date, price.Price);

            return result;
        }

        public static RestApi.Stocks.AssetCategory ToResponse(this Domain.Stocks.AssetCategory assetCategory)
        {
            return (RestApi.Stocks.AssetCategory)assetCategory;
        }

        public static Domain.Stocks.AssetCategory ToDomain(this RestApi.Stocks.AssetCategory assetCategory)
        {
            return (Domain.Stocks.AssetCategory)assetCategory;
        }

        public static RestApi.Stocks.DrpMethod ToResponse(this Domain.Stocks.DrpMethod method)
        {
            return (RestApi.Stocks.DrpMethod)method;
        }
        public static Domain.Stocks.DrpMethod ToDomain(this RestApi.Stocks.DrpMethod method)
        {
            return (Domain.Stocks.DrpMethod)method;
        }
    }
}
