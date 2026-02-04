using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;

using Booth.Common;
using Booth.PortfolioManager.Web.Serialization;

namespace Booth.PortfolioManager.Web.Test.Serialization
{
    public class SerializeTests
    {
        [Fact]
        public void StandardTypes()
        {
            var serializer = new ModelSerializer();

            var obj = new StandardTypesTestData()
            {
                Integer = 5,
                String = "Hello",
                Decimal = 123.45m,
                DateTime = new DateTime(2004, 02, 03, 14, 22, 04)
            };
            var result = JToken.Parse(serializer.Serialize<StandardTypesTestData>(obj));
          
            var expectedJson = JToken.Parse("{\"integer\":5,\"string\":\"Hello\",\"decimal\":123.45,\"dateTime\":\"2004-02-03T14:22:04\"}");

            result.Should().BeEquivalentTo(expectedJson);
        }


        [Fact]
        public void DateType()
        {
            var serializer = new ModelSerializer();

            var obj = new DateTestData()
            {
                Date = new Date(2004, 01, 02)
            };
            var result = JToken.Parse(serializer.Serialize<DateTestData>(obj));

            var expectedJson = JToken.Parse("{\"date\":\"2004-01-02\"}");

            result.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void TimeType()
        {
            var serializer = new ModelSerializer();

            var obj = new TimeTestData()
            {
                Time = new Time(13, 45, 01)
            };
            var result = JToken.Parse(serializer.Serialize<TimeTestData>(obj));

            var expectedJson = JToken.Parse("{\"time\":\"13:45:01\"}");

            result.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void NullValue()
        {
            var serializer = new ModelSerializer();

            var obj = new StandardTypesTestData()
            {
                Integer = 5,
                String = null,
                Decimal = 123.45m,
                DateTime = new DateTime(2004, 02, 03, 14, 22, 04)
            };
            var result = JToken.Parse(serializer.Serialize<StandardTypesTestData>(obj));

            var expectedJson = JToken.Parse("{\"integer\":5,\"decimal\":123.45,\"dateTime\":\"2004-02-03T14:22:04\"}");

            result.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void SerializeUntypedToStream()
        {
            var serializer = new ModelSerializer();
            var stream = new MemoryStream();

            JToken json;
            var expectedJson = JToken.Parse("{\"field\":\"Hello\"}");

            using (var streamWriter = new StreamWriter(stream))
            {
                var obj = new SingleValueTestData()
                {
                    Field = "Hello",
                };

                serializer.Serialize(streamWriter, obj);

                streamWriter.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var result = reader.ReadToEnd();
                    json = JToken.Parse(result);
                }
            }

            json.Should().BeEquivalentTo(expectedJson);
        }

        [Fact]
        public void SerializeTypedToStream()
        {
            var serializer = new ModelSerializer();
            var stream = new MemoryStream();

            JToken json;
            var expectedJson = JToken.Parse("{\"field\":\"Hello\"}");

            using (var streamWriter = new StreamWriter(stream))
            {
                var obj = new SingleValueTestData()
                {
                    Field = "Hello",
                };
                serializer.Serialize<SingleValueTestData>(streamWriter, obj);

                streamWriter.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var result = reader.ReadToEnd();
                    json = JToken.Parse(result);
                }
            }
                


            json.Should().BeEquivalentTo(expectedJson);
        }
        
    }

}
