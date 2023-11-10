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
    class StockPriceHistorySerializer : SerializerBase<StockPriceHistory>
    {
        public override StockPriceHistory Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            Guid id = Guid.Empty;
            List<StockPrice> closingPrices = null;

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                switch (name)
                {
                    case "_id":
                        id = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "closingPrices":
                        closingPrices = BsonSerializer.Deserialize<List<StockPrice>>(bsonReader);
                        break;
                }
            }

            var stockPriceHistory = new StockPriceHistory(id);

            stockPriceHistory.UpdateClosingPrices(closingPrices);
           
            return stockPriceHistory;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, StockPriceHistory value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("_id");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);

            bsonWriter.WriteName("closingPrices");
            BsonSerializer.Serialize<IEnumerable<StockPrice>>(bsonWriter, value.GetPrices(new DateRange(Date.MinValue, Date.MaxValue)));

            bsonWriter.WriteEndDocument();
        }
    }
}
