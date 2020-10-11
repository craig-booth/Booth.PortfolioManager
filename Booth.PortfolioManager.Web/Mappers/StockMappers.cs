﻿using System;
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

        public static RestApi.Portfolios.Stock ToSummaryResponse(this IReadOnlyStock stock, Date date)
        {
            var stockProperties = stock.Properties.ClosestTo(date);    

            var result = new RestApi.Portfolios.Stock()
            {
                Id = stock.Id,
                AsxCode = stockProperties.AsxCode,
                Name = stockProperties.Name,
                Category = stockProperties.Category.ToResponse()
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
    }
}
