﻿using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Domain.Utils
{
    class MaximizeGainCgtComparer : Comparer<IParcel>
    {
        private Date _DisposalDate;

        public MaximizeGainCgtComparer(Date disposalDate)
        {
            _DisposalDate = disposalDate;
        }

        public override int Compare(IParcel a, IParcel b)
        {
            var discountAppliesA = (CgtUtils.CgtMethodForParcel(a.AquisitionDate, _DisposalDate) == CgtMethod.Discount);
            var discountAppliesB = (CgtUtils.CgtMethodForParcel(b.AquisitionDate, _DisposalDate) == CgtMethod.Discount);

            if (discountAppliesA && !discountAppliesB)
                return -1;
            else if (discountAppliesB && !discountAppliesA)
                return 1;
            else
            {
                decimal unitCostBaseA = a.Properties[_DisposalDate].CostBase / a.Properties[_DisposalDate].Units;
                decimal unitCostBaseB = b.Properties[_DisposalDate].CostBase / b.Properties[_DisposalDate].Units;

                if (unitCostBaseA > unitCostBaseB)
                    return 1;
                else if (unitCostBaseA < unitCostBaseB)
                    return -1;
                else
                    return a.AquisitionDate.CompareTo(b.AquisitionDate);         
            }
        }
    }
}
