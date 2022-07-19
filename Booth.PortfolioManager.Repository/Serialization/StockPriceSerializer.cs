using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class StockPriceSerializer : SerializerBase<StockPrice>
    {
        public override StockPrice Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            Date date = Date.MinValue;
            decimal closingPrice = 0.00m;

            bsonReader.ReadStartDocument();

            if (bsonReader.ReadName() == "date")
                date = BsonSerializer.Deserialize<Date>(bsonReader);

            if (bsonReader.ReadName() == "price")
                closingPrice = (decimal)bsonReader.ReadDecimal128();

            bsonReader.ReadEndDocument();

            return new StockPrice(date, closingPrice);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StockPrice value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("date");
            BsonSerializer.Serialize<Date>(bsonWriter, value.Date);

            bsonWriter.WriteName("price");
            bsonWriter.WriteDecimal128(value.Price);

            bsonWriter.WriteEndDocument();
        }
    }
}
