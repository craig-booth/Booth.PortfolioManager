using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class EffectivePropertiesSerializer<T> : SerializerBase<IEffectiveProperties<T>>, IBsonArraySerializer where T : struct
    {
        public override IEffectiveProperties<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var properties = new EffectiveProperties<T>();

            var bsonReader = context.Reader;
            if (bsonReader.CurrentBsonType == BsonType.Array)
            {
                bsonReader.ReadStartArray();

                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    var effectiveValues = BsonSerializer.Deserialize<IEffectivePropertyValues<T>>(bsonReader);
                    properties.Change(effectiveValues.EffectivePeriod.FromDate, effectiveValues.Properties);
                }

                bsonReader.ReadEndArray();
            }

            return properties; 
        }
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IEffectiveProperties<T> value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartArray();
            foreach (var effectiveValue in value.Values.Reverse())
            {
                BsonSerializer.Serialize<IEffectivePropertyValues<T>>(bsonWriter, effectiveValue);        
            }
            bsonWriter.WriteEndArray(); 
        }

        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(T));
            var nominalType = typeof(T);
            serializationInfo = new BsonSerializationInfo(null, serializer, nominalType);
            return true;
        }
    }
}
