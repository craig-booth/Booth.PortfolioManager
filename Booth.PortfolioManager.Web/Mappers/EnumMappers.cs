using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booth.PortfolioManager.Web.Mappers
{
    static class EnumMappers
    {
        public static Models.Stock.AssetCategory ToResponse(this Domain.Stocks.AssetCategory assetCategory)
        {
            return (Models.Stock.AssetCategory)assetCategory;
        }

        public static Domain.Stocks.AssetCategory ToDomain(this Models.Stock.AssetCategory assetCategory)
        {
            return (Domain.Stocks.AssetCategory)assetCategory;
        }

        public static Models.Stock.DrpMethod ToResponse(this Domain.Stocks.DrpMethod method)
        {
            return (Models.Stock.DrpMethod)method;
        }
        public static Domain.Stocks.DrpMethod ToDomain(this Models.Stock.DrpMethod method)
        {
            return (Domain.Stocks.DrpMethod)method;
        }

        public static Models.Transaction.CashTransactionType ToResponse(this Domain.Transactions.BankAccountTransactionType type)
        {
            return (Models.Transaction.CashTransactionType)type;
        }
        public static Domain.Transactions.BankAccountTransactionType ToDomain(this Models.Transaction.CashTransactionType type)
        {
            return (Domain.Transactions.BankAccountTransactionType)type;
        }
        public static Models.Portfolio.CgtMethod ToResponse(this Domain.Portfolios.CgtMethod method)
        {
            return (Models.Portfolio.CgtMethod)method;
        }
        public static Domain.Portfolios.CgtMethod ToDomain(this Models.Portfolio.CgtMethod method)
        {
            return (Domain.Portfolios.CgtMethod)method;
        }

        public static Models.Transaction.CgtCalculationMethod ToResponse(this Domain.Utils.CgtCalculationMethod method)
        {
            return (Models.Transaction.CgtCalculationMethod)method;
        }
        public static Domain.Utils.CgtCalculationMethod ToDomain(this Models.Transaction.CgtCalculationMethod method)
        {
            return (Domain.Utils.CgtCalculationMethod)method;
        }
    }
}
