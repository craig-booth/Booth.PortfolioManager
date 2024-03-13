using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.Attributes;

using Booth.Common;

namespace Booth.PortfolioManager.Repository.Serialization
{
    [BsonSerializer(typeof(DateSerializer))]
    class DateSerializer : SerializerBase<Date>
    {
        public override Date Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var type = context.Reader.GetCurrentBsonType();

            switch (type)
            {
                case BsonType.String:
                    var stringValue = context.Reader.ReadString();
                    if (string.IsNullOrWhiteSpace(stringValue))
                        return Date.MinValue;
                    
                    if (Date.TryParseExact(stringValue, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out Date date))
                        return date;

                    break;

                case BsonType.DateTime:
                    var dateTimeValue = context.Reader.ReadDateTime();
                    return new Date(DateTimeOffset.FromUnixTimeMilliseconds(dateTimeValue).DateTime);

                case BsonType.Timestamp:
                    var timeStampValue = context.Reader.ReadTimestamp();
                    return new Date(DateTimeOffset.FromUnixTimeMilliseconds(timeStampValue).DateTime);
            }

            return Date.MinValue;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Date value)
        {
            context.Writer.WriteString(value.ToString("yyyy-MM-dd"));
        }
    }
}