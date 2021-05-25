using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.TradingCalendars;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class TradingCalendarSerializer : SerializerBase<TradingCalendar>
    {
        public override TradingCalendar Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartDocument();

            bsonReader.ReadName();
            var id = BsonSerializer.Deserialize<Guid>(bsonReader);

            var calendar = new TradingCalendar(id);

            bsonReader.ReadName();
            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                bsonReader.ReadStartDocument();

                bsonReader.ReadName();
                var year = bsonReader.ReadInt32();

                bsonReader.ReadName();
                var days = BsonSerializer.Deserialize<IEnumerable<NonTradingDay>>(bsonReader);

                bsonReader.ReadEndDocument();

                calendar.SetNonTradingDays(year, days);
            }
            bsonReader.ReadEndArray();

            bsonReader.ReadEndDocument();


            return calendar;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TradingCalendar value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("_id");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);

            bsonWriter.WriteName("years");
            bsonWriter.WriteStartArray();
            foreach (var year in value.Years)
            {
                var days = value.NonTradingDays(year);

                bsonWriter.WriteStartDocument();

                bsonWriter.WriteName("year");
                bsonWriter.WriteInt32(year);

                bsonWriter.WriteName("days");
                BsonSerializer.Serialize<IEnumerable<NonTradingDay>>(bsonWriter, days);

                bsonWriter.WriteEndDocument();
            }
            bsonWriter.WriteEndArray();

            bsonWriter.WriteEndDocument();
        }
    }
}
