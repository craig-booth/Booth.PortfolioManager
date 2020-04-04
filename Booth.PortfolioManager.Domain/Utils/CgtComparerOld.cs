using System;
using System.Collections.Generic;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;


namespace Booth.PortfolioManager.Domain.Utils
{
    public class CgtComparerOld : Comparer<Parcel>
    {
        public Date DisposalDate { get; private set; }
        public CGTCalculationMethod Method { get; private set; }

        public CgtComparerOld(Date disposalDate, CGTCalculationMethod method)
        {
            DisposalDate = disposalDate;
            Method = method;
        }

        public override int Compare(Parcel a, Parcel b)
        {
            if (Method == CGTCalculationMethod.FirstInFirstOut)
                return a.AquisitionDate.CompareTo(b.AquisitionDate);
            else if (Method == CGTCalculationMethod.LastInFirstOut)
                return b.AquisitionDate.CompareTo(a.AquisitionDate);
            else
            {
                var discountAppliesA = (CgtCalculatorOld.CgtMethodForParcel(a.AquisitionDate, DisposalDate) == CGTMethod.Discount);
                var discountAppliesB = (CgtCalculatorOld.CgtMethodForParcel(b.AquisitionDate, DisposalDate) == CGTMethod.Discount);

                if (discountAppliesA && !discountAppliesB)
                    return -1;
                else if (discountAppliesB && !discountAppliesA)
                    return 1;
                else
                {
                    decimal unitCostBaseA = a.Properties[DisposalDate].CostBase / a.Properties[DisposalDate].Units;
                    decimal unitCostBaseB = b.Properties[DisposalDate].CostBase / b.Properties[DisposalDate].Units;


                    if (Method == CGTCalculationMethod.MaximizeGain)
                    {
                        if (unitCostBaseA > unitCostBaseB)
                            return 1;
                        else if (unitCostBaseA < unitCostBaseB)
                            return -1;
                        else
                            return a.AquisitionDate.CompareTo(b.AquisitionDate);
                    }
                    else
                    {
                        if (unitCostBaseA > unitCostBaseB)
                            return -1;
                        else if (unitCostBaseA < unitCostBaseB)
                            return 1;
                        else
                            return a.AquisitionDate.CompareTo(b.AquisitionDate);
                    }
                }
            }

        }
    }
}
