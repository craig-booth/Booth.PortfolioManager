using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Booth.Common;
using Booth.EventStore;

using Booth.PortfolioManager.Domain.Stocks.Events;
using Booth.PortfolioManager.Domain.CorporateActions.Events;
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
        Date DateOfLastestPrice();
        decimal GetPrice(Date date);
        IEnumerable<StockPrice> GetPrices(DateRange dateRange);
        string ToString();
    }

    public interface IStock : IReadOnlyStock
    {
        new ICorporateActionList CorporateActions { get; }
        void ChangeDividendRules(Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod);
        void ChangeProperties(Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory);
        void DeList(Date date);
        void List(string asxCode, string name, Date date, bool trust, AssetCategory category);
        void SetPriceHistory(IStockPriceHistory stockPriceHistory);
    }

    public class Stock : EffectiveEntity, ITrackedEntity, IStock, IReadOnlyStock
    {
        public int Version { get; protected set; } = 0;
        private EventList _Events = new EventList();

        private IStockPriceHistory _StockPriceHistory;

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
            _CorporateActions = new CorporateActionList(this, this._Events);
        }

        public override string ToString()
        {
            var properties = Properties.ClosestTo(Date.Today);
            return String.Format("{0} - {1}", properties.AsxCode, properties.Name);
        }

        public void SetPriceHistory(IStockPriceHistory stockPriceHistory)
        {
            _StockPriceHistory = stockPriceHistory;
        }

        protected void PublishEvent(Event @event)
        {
            _Events.Add(@event);
        }

        public void List(string asxCode, string name, Date date, bool trust, AssetCategory category)
        {
            if ((date <= Date.MinValue) || (date >= Date.MaxValue))
                throw new ArgumentOutOfRangeException("Listing date is invalid");


            var @event = new StockListedEvent(Id, Version, asxCode, name, date, category, trust);
            Apply(@event);

            PublishEvent(@event);
        }

        public void Apply(StockListedEvent @event)
        {
            Version++;
            Trust = @event.Trust;

            Start(@event.ListingDate);

            var properties = new StockProperties(@event.ASXCode, @event.Name, @event.Category);
            _Properties.Change(@event.ListingDate, properties);

            var dividendRules = new DividendRules(0.30m, RoundingRule.Round, false, DrpMethod.Round);
            _DividendRules.Change(@event.ListingDate, dividendRules);
        }

        public void DeList(Date date)
        {
            var @event = new StockDelistedEvent(Id, Version, date);
            Apply(@event);

            PublishEvent(@event);
        }

        public virtual void Apply(StockDelistedEvent @event)
        {
            Version++;

            _Properties.End(@event.DelistedDate);
            _DividendRules.End(@event.DelistedDate);

            End(@event.DelistedDate);
        }

        public virtual void Apply(CorporateActionAddedEvent @event)
        {
            Version++;

            dynamic dynamicEvent = @event;
            _CorporateActions.Apply(dynamicEvent);
        }

        public decimal GetPrice(Date date)
        {
            if (_StockPriceHistory != null)
                return _StockPriceHistory.GetPrice(date);
            else
                return 0.00m;
        }

        public IEnumerable<StockPrice> GetPrices(DateRange dateRange)
        {
            if (_StockPriceHistory != null)
                return _StockPriceHistory.GetPrices(dateRange);
            else
                return new StockPrice[0];
        }

        public Date DateOfLastestPrice()
        {
            if (_StockPriceHistory != null)
                return _StockPriceHistory.LatestDate;
            else
                return Date.MinValue;
        }

        public void ChangeProperties(Date changeDate, string newAsxCode, string newName, AssetCategory newAssetCategory)
        {
            if (!IsEffectiveAt(changeDate))
                throw new EffectiveDateException(String.Format("Stock not active at {0}", changeDate));

            var properties = Properties[changeDate];

            var @event = new StockPropertiesChangedEvent(Id, Version, changeDate, newAsxCode, newName, newAssetCategory);

            Apply(@event);
            _Events.Add(@event);
        }

        public void ChangeDividendRules(Date changeDate, decimal companyTaxRate, RoundingRule newDividendRoundingRule, bool drpActive, DrpMethod newDrpMethod)
        {
            if (!IsEffectiveAt(changeDate))
                throw new EffectiveDateException(String.Format("Stock not active at {0}", changeDate));

            var properties = Properties[changeDate];

            var @event = new ChangeDividendRulesEvent(Id, Version, changeDate, companyTaxRate, newDividendRoundingRule, drpActive, newDrpMethod);

            Apply(@event);
            _Events.Add(@event);
        }

        public void Apply(StockPropertiesChangedEvent @event)
        {
            Version++;

            var newProperties = new StockProperties(
                @event.ASXCode,
                @event.Name,
                @event.Category);

            _Properties.Change(@event.ChangeDate, newProperties);
        }

        public void Apply(ChangeDividendRulesEvent @event)
        {
            Version++;

            var newProperties = new DividendRules(
                @event.CompanyTaxRate,
                @event.DividendRoundingRule,
                @event.DrpActive,
                @event.DrpMethod);

            _DividendRules.Change(@event.ChangeDate, newProperties);
        }

        public IEnumerable<Event> FetchEvents()
        {
            return _Events.Fetch();
        }

        public virtual void ApplyEvents(IEnumerable<Event> events)
        {
            foreach (var @event in events)
            {
                dynamic dynamicEvent = @event;
                Apply(dynamicEvent);
            }
        }
    }

    public struct StockProperties
    {
        public readonly string AsxCode;
        public readonly string Name;
        public readonly AssetCategory Category;

        public StockProperties(string asxCode, string name, AssetCategory category)
        {
            AsxCode = asxCode;
            Name = name;
            Category = category;
        }
    }

    public struct DividendRules
    {
        public readonly decimal CompanyTaxRate;
        public readonly RoundingRule DividendRoundingRule;

        public readonly bool DrpActive;       
        public readonly DrpMethod DrpMethod;

        public DividendRules(decimal companyTaxRate, RoundingRule dividendRoundingRule, bool drpActive, DrpMethod drpMethod)
        {
            CompanyTaxRate = companyTaxRate;
            DividendRoundingRule = dividendRoundingRule;
            DrpActive = drpActive;
            DrpMethod = drpMethod;
        }
    }


}
