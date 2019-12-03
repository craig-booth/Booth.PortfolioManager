using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Utils
{
    public class CashFlows
    {
        private Dictionary<Date, decimal> _Amounts;

        public CashFlows()
        {
            _Amounts = new Dictionary<Date, decimal>();
        }

        public void Add(Date date, decimal amount)
        {
            if (_Amounts.ContainsKey(date))
                _Amounts[date] += amount;
            else
                _Amounts.Add(date, amount);
        }

        public void GetCashFlows(out double[] values, out double[] periods)
        {
            values = new double[_Amounts.Count];
            periods = new double[_Amounts.Count];

            int i = 0;
            var startDate = _Amounts.First().Key;
            foreach (var amount in _Amounts)
            {
                if (amount.Value != 0.00m)
                {
                    values[i] = (double)amount.Value;
                    periods[i] = (amount.Key - startDate).Days / 365.0;
                }

                i++;
            }

        }

    }

    public class IrrCalculator
    {
        private const double RequiredPrecision = 0.000001;
        private const int MaximumIterations = 50;

        private static double CalculateResult(double[] values, double[] period, double guess)
        {
            double r = guess + 1;
            double result = values[0];

            for (var i = 1; i < values.Length; i++)
                result += values[i] / Math.Pow(r, period[i]);

            return result;
        }

        private static double CalculateDerivative(double[] values, double[] period, double guess)
        {
            double r = guess + 1;
            double result = 0;

            for (var i = 1; i < values.Length; i++)
            {
                var frac = period[i];
                result -= frac * values[i] / Math.Pow(r, frac + 1);
            }

            return result;
        }


        private static double Calculate(double[] values, double[] periods, double guess)
        {
            var irr = guess;

            var iteration = 0;
            do
            {
                iteration++;

                var resultValue = CalculateResult(values, periods, irr);
                var derivativeValue = CalculateDerivative(values, periods, irr);

                var newIRR = irr - (resultValue / derivativeValue);

                if (double.IsNaN(newIRR))
                    return 0;

                var epsilon = Math.Abs(newIRR - irr);
                irr = newIRR;

                // Check if required precision achieved
                if (epsilon < RequiredPrecision)
                    break;

            } while (iteration < MaximumIterations);

            return irr;
        }

        public static double CalculateIrr(CashFlows cashFlows)
        {
            return CalculateIrr(cashFlows, 0.10);
        }

        public static double CalculateIrr(CashFlows cashFlows, double guess)
        {
            double[] values;
            double[] periods;

            cashFlows.GetCashFlows(out values, out periods);

            if (periods.Count() <= 1)
                return 0.00;

            return Calculate(values, periods, guess);
        }

    }
}
