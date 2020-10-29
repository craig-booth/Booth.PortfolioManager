using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain.Utils
{
    public class IrrCalculator
    {
        private const double RequiredPrecision = 0.000001;
        private const int MaximumIterations = 100;

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

        public static double CalculateIrr(Date startDate, decimal initialnvestment, Date finalDate, decimal finalValue, CashFlows cashFlows)
        {
            cashFlows.GetCashFlows(startDate, initialnvestment, finalDate, finalValue, out var values, out var periods);

            var irr = Calculate(values, periods, 0.10);

            return irr;
        }

    }
}
