using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class SingleValueTestData
    {
        public string Field { get; set; }
    }

    public class StandardTypesTestData
    {
        public int Integer { get; set; }
        public string String { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class DateTestData
    {
        public Date Date { get; set; }
    }

    public class TimeTestData
    {
        public Time Time { get; set; }
    }
}
