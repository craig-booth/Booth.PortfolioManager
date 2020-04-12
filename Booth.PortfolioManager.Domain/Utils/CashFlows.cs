using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Utils
{
    class CashFlows
    {
        private Dictionary<Date, decimal> _Amounts;

        public CashFlows()
        {
            _Amounts = new Dictionary<Date, decimal>();
        }

        public int Count
        {
            get { return _Amounts.Count; }
        }

        public decimal this[Date date]
        {
            get { return _Amounts[date]; }
        }

        public void Add(Date date, decimal amount)
        {
            if (_Amounts.ContainsKey(date))
                _Amounts[date] += amount;
            else
                _Amounts.Add(date, amount);
        }

        public void GetCashFlows(Date startDate, decimal initialnvestment, Date finalDate, decimal finalValue, out double[] values, out double[] periods)
        {
            if (startDate >= finalDate)
                throw new ArgumentException("Start must be before final date");

            if (initialnvestment < 0)
                throw new ArgumentException("Initial investment must be greater than 0");

            var orderedCashFlows = _Amounts.Where(x => (x.Key > startDate) && (x.Key <= finalDate) && (x.Value != 0.00m)).OrderBy(x => x.Key).ToList();

            var cashFlowOnFinalDay = ((orderedCashFlows.Count > 0) && (orderedCashFlows[orderedCashFlows.Count - 1].Key == finalDate));

            if (cashFlowOnFinalDay)
            {
                values = new double[orderedCashFlows.Count + 1];
                periods = new double[orderedCashFlows.Count + 1];
            }
            else
            {
                values = new double[orderedCashFlows.Count + 2];
                periods = new double[orderedCashFlows.Count + 2];
            }
              
            // Add initial investment
            values[0] = (double)-initialnvestment;
            periods[0] = 0d;

            // Add cash flows
            int i = 0;
            foreach (var amount in orderedCashFlows)
            {
                i++;
                values[i] = (double)amount.Value;
                periods[i] = (amount.Key - startDate).Days / 365.0;
            }

            // Add final investment
            if (cashFlowOnFinalDay)
                values[i] += (double)finalValue;
            else
            {
                i++;
                values[i] = (double)finalValue;
                periods[i] = (finalDate - startDate).Days / 365.0;
            }

        }

    }
}
