using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.CorporateActions;

namespace Booth.PortfolioManager.Domain.Stocks
{
    public enum AssetCategory { AustralianStocks, InternationalStocks, AustralianProperty, InternationalProperty, AustralianFixedInterest, InternationlFixedInterest, Cash }
    public enum DrpMethod { Round, RoundDown, RoundUp, RetainCashBalance }

    public interface IReadOnlyStock : IEffectiveEntity
    {
        ITransactionList<CorporateAction> CorporateActions { get; }
        IEffectiveProperties<DividendRules> DividendRules { get; }
        IEffectiveProperties<StockProperties> Properties { get; }
        bool Trust { get; }
        string ToString();
    }

    public interface IStock : IReadOnlyStock
    {
        new ICorporateActionList CorporateActions { get; }
        void ChangeDividendRules(Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod);
        void ChangeProperties(Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory);
        void DeList(Date date);
        void List(string asxCode, string name, Date date, bool trust, AssetCategory category);
    }

    public class Stock : EffectiveEntity, IEntity, IStock, IReadOnlyStock
    {
        public bool Trust { get; private set; }

        protected EffectiveProperties<StockProperties> _Properties = new EffectiveProperties<StockProperties>();
        public IEffectiveProperties<StockProperties> Properties => _Properties;

        protected EffectiveProperties<DividendRules> _DividendRules = new EffectiveProperties<DividendRules>();
        public IEffectiveProperties<DividendRules> DividendRules => _DividendRules;

        protected CorporateActionList _CorporateActions;
        public ICorporateActionList CorporateActions => _CorporateActions;

        ITransactionList<CorporateAction> IReadOnlyStock.CorporateActions => _CorporateActions;

        public Stock(Guid id)
            : base(id)
        {
            _CorporateActions = new CorporateActionList(this);
        }

        public override string ToString()
        {
            var properties = Properties.ClosestTo(Date.Today);
            return String.Format("{0} - {1}", properties.AsxCode, properties.Name);
        }

        public virtual void List(string asxCode, string name, Date date, bool trust, AssetCategory category)
        {
            if ((date <= Date.MinValue) || (date >= Date.MaxValue))
                throw new ArgumentOutOfRangeException("Listing date is invalid");

            Trust = trust;

            Start(date);

            var properties = new StockProperties(asxCode, name, category);
            _Properties.Change(date, properties);

            var dividendRules = new DividendRules(0.30m, RoundingRule.Round, false, DrpMethod.Round);
            _DividendRules.Change(date, dividendRules);
        }

        public virtual void DeList(Date date)
        {
            _Properties.End(date);
            _DividendRules.End(date);

            End(date);
        }

        public void ChangeProperties(Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory)
        {
            if (!IsEffectiveAt(changeDate))
                throw new EffectiveDateException(String.Format("Stock not active at {0}", changeDate));

            var newProperties = new StockProperties(newAsxCode, newName, newAssetCategory);
            _Properties.Change(changeDate, newProperties);
        }

        public void ChangeDividendRules(Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod)
        {
            if (!IsEffectiveAt(changeDate))
                throw new EffectiveDateException(String.Format("Stock not active at {0}", changeDate));

            var newProperties = new DividendRules(companyTaxRate, newDividendRoundingRule, drpActive, newDrpMethod);

            _DividendRules.Change(changeDate, newProperties);
        }

    }

    public struct StockProperties
    {
        public string AsxCode { get; }
        public string Name { get; }
        public AssetCategory Category { get; }

        public StockProperties(string asxCode, string name, AssetCategory category)
        {
            AsxCode = asxCode;
            Name = name;
            Category = category;
        }
    }

    public struct DividendRules
    {
        public decimal CompanyTaxRate { get; }
        public RoundingRule DividendRoundingRule { get; }

        public bool DrpActive { get; }
        public DrpMethod DrpMethod { get; }

        public DividendRules(decimal companyTaxRate, RoundingRule dividendRoundingRule, bool drpActive, DrpMethod drpMethod)
        {
            CompanyTaxRate = companyTaxRate;
            DividendRoundingRule = dividendRoundingRule;
            DrpActive = drpActive;
            DrpMethod = drpMethod;
        }
    }


}
