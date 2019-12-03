using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;
using Booth.EventStore;

namespace Booth.PortfolioManager.Domain.CorporateActions.Events
{
    public class DividendAddedEvent : CorporateActionAddedEvent
    {
        public Date PaymentDate { get; set; }
        public decimal DividendAmount { get; set; }
        public decimal PercentFranked { get; set; }
        public decimal DRPPrice { get; set; }

        public DividendAddedEvent(Guid entityId, int version, Guid actionId, Date actionDate, string description, Date paymentDate, decimal dividendAmount, decimal percentFranked, decimal drpPrice)
            : base(entityId, version, actionId, actionDate, description)
        {
            PaymentDate = paymentDate;
            DividendAmount = dividendAmount;
            PercentFranked = percentFranked;
            DRPPrice = drpPrice;
        }
    }
}
