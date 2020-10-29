using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Mappers
{
    static class EnumMappers
    {
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

        public static RestApi.Transactions.CashTransactionType ToResponse(this Domain.Transactions.BankAccountTransactionType type)
        {
            return (RestApi.Transactions.CashTransactionType)type;
        }
        public static Domain.Transactions.BankAccountTransactionType ToDomain(this RestApi.Transactions.CashTransactionType type)
        {
            return (Domain.Transactions.BankAccountTransactionType)type;
        }
        public static RestApi.Portfolios.CgtMethod ToResponse(this Domain.Portfolios.CgtMethod method)
        {
            return (RestApi.Portfolios.CgtMethod)method;
        }
        public static Domain.Portfolios.CgtMethod ToDomain(this RestApi.Portfolios.CgtMethod method)
        {
            return (Domain.Portfolios.CgtMethod)method;
        }

        public static RestApi.Transactions.CgtCalculationMethod ToResponse(this Domain.Utils.CgtCalculationMethod method)
        {
            return (RestApi.Transactions.CgtCalculationMethod)method;
        }
        public static Domain.Utils.CgtCalculationMethod ToDomain(this RestApi.Transactions.CgtCalculationMethod method)
        {
            return (Domain.Utils.CgtCalculationMethod)method;
        }
    }
}
