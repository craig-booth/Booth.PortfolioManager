using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using Xunit;
using FluentAssertions;

using Booth.Common;
using Booth.PortfolioManager.Web.Serialization;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class DeserializeTests
    {
        [Fact]
        public void StandardTypes()
        {
            var serializer = new ModelSerializer();

            var json = "{\"integer\":5,\"string\":\"Hello\", \"decimal\":123.45,\"dateTime\":\"2004-02-03T14:22:04\"}";

            var result = serializer.Deserialize<StandardTypesTestData>(json);

            var expected = new StandardTypesTestData()
            {
                Integer = 5,
                String = "Hello",
                Decimal = 123.45m,
                DateTime = new DateTime(2004, 02, 03, 14, 22, 04)
            };

            result.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public void DateType()
        {
            var serializer = new ModelSerializer();

            var json = "{\"date\":\"2004-01-02\"}";

            var result = serializer.Deserialize<DateTestData>(json);

            result.Should().BeEquivalentTo(new { Date = new Date(2004, 01, 02) });
        }

        [Fact]
        public void TimeType()
        {
            var serializer = new ModelSerializer();

            var json = "{\"time\":\"13:45:01\"}";

            var result = serializer.Deserialize<TimeTestData>(json);

            result.Should().BeEquivalentTo(new { Time = new Time(13, 45, 01) });
        }

        [Fact]
        public void ExtraFieldReceived()
        {
            var serializer = new ModelSerializer();

            var json = "{\"field\":\"Hello\",\"extra\":\"value\"}";

            var result = serializer.Deserialize<SingleValueTestData>(json);

            result.Should().BeEquivalentTo(new { Field = "Hello" } );
        }

        [Fact]
        public void NotAllFieldsReceived()
        {
            var serializer = new ModelSerializer();

            var json = "{\"string\":\"Hello\"}";

            var result = serializer.Deserialize<StandardTypesTestData>(json);

            var expected = new StandardTypesTestData() { String = "Hello" };
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DeserializeFromStream()
        {
            var serializer = new ModelSerializer();

            var json = "{\"field\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            SingleValueTestData result;
            using (var streamReader = new StreamReader(stream))
            {
                result = serializer.Deserialize<SingleValueTestData>(streamReader);
            }

            var expected = new SingleValueTestData() { Field = "Hello" };
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DeserializeFromStreamInvalidJson()
        {
            var serializer = new ModelSerializer();

            var json = "{\"field\" \"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            Action a = () =>
            {
                using (var streamReader = new StreamReader(stream))
                {
                    serializer.Deserialize<StandardTypesTestData>(streamReader);
                }
            };

            a.Should().ThrowExactly<System.Text.Json.JsonException>();
        }

        [Fact]
        public void DeserializeFromStreamTypeWrong()
        {
            var serializer = new ModelSerializer();

            var json = "{\"field\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            StandardTypesTestData result;
            using (var streamReader = new StreamReader(stream))
            {
                result = serializer.Deserialize<StandardTypesTestData>(streamReader);
            }

            result.Should().BeEquivalentTo(new StandardTypesTestData());
        } 

    }

}
