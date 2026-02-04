using System;

using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.TradingCalendar;
using Booth.PortfolioManager.Web.Serialization;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class TradingCalendarSerializationTests
    {
        [Fact]
        public void SerializeTradingCalander()
        {
            var serializer = new ModelSerializer();

            var request = new TradingCalendar() { Year = 2013 };
            request.AddNonTradingDay(new Date(2013, 01, 01), "New Year's Day");
            request.AddNonTradingDay(new Date(2013, 12, 25), "Christmas Day");

            var json = JToken.Parse(serializer.Serialize(request));

            var expectedJson = JToken.Parse("{\"year\":2013,\"nonTradingDays\":["
                            + "{\"date\":\"2013-01-01\",\"description\":\"New Year's Day\"},"
                            + "{\"date\":\"2013-12-25\",\"description\":\"Christmas Day\"}]}");

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void DeserializeTradingCalander()
        {
            var serializer = new ModelSerializer();

            var json = "{\"year\":2013,\"nonTradingDays\":["
                            + "{\"date\":\"2013-01-01\",\"description\":\"New Year's Day\"},"
                            + "{\"date\":\"2013-12-25\",\"description\":\"Christmas Day\"}]}";

            var request = serializer.Deserialize<TradingCalendar>(json);

            var expected = new
            {
                Year = 2013,
                NonTradingDays = new object[] {
                    new { Date = new Date(2013, 01, 01), Description = "New Year's Day"},
                    new { Date = new Date(2013, 12, 25), Description = "Christmas Day"}
                }
            };               
            request.Should().BeEquivalentTo(expected);
        }
    }
}
