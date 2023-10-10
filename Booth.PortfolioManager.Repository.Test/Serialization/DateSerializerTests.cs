using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

using Xunit;
using FluentAssertions;
using Moq;

using Booth.Common;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Repository.Serialization;


namespace Booth.PortfolioManager.Repository.Test.Serialization
{

    [Collection(Collection.Serialization)]
    public class DateSerializerTests 
    {
        private readonly SerializationTestFixture _Fixture;

        public DateSerializerTests(SerializationTestFixture fixture)
        {
            _Fixture = fixture;
        }

        [Fact]
        public void ShouldSerializeDate()
        {
            var date = new Date(2021, 03, 05);

            var jsonDate = BsonExtensionMethods.ToJson(date);

            jsonDate.Should().Be("\"2021-03-05\"");
        }

        [Fact]
        public void ShouldDeserializeDate()
        {
            var date = BsonSerializer.Deserialize<Date>("\"2021-03-05\"");

            date.Should().Be(new Date(2021, 03, 05));
        }
    }
}
