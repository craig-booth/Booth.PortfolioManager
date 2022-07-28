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
    class EffectivePropertyValuesSerializer<T> : SerializerBase<IEffectivePropertyValues<T>> where T : struct
    {
        private BsonClassMap _ClassMap;

        public EffectivePropertyValuesSerializer()
        {
            _ClassMap = BsonClassMap.LookupClassMap(typeof(T));
        }

        public override IEffectivePropertyValues<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            
            bsonReader.ReadStartDocument();

            Date changeDate = Date.MinValue;
            T property = default(T);

            if (bsonReader.ReadName() == "date")
                changeDate = BsonSerializer.Deserialize<Date>(bsonReader);

            if (bsonReader.ReadName() == "properties")
                property = GetEffectivePropertyValue(context);

            bsonReader.ReadEndDocument();

            return new EffectivePropertyValues<T>(changeDate, property);
        }
        
        private T GetEffectivePropertyValue(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            var values = new Dictionary<string, object>();

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                var memberMap = _ClassMap.GetMemberMapForElement(name);

                var value = memberMap.GetSerializer().Deserialize(context);

                values.Add(memberMap.MemberName, value);
            }
            bsonReader.ReadEndDocument();


            // Create Effective Property
            T result;
            if (_ClassMap.HasCreatorMaps)
            {
                var creatorMap = _ClassMap.CreatorMaps.First();

                var constructorArgs = new List<object>();
                var contructorTypes = new List<Type>();

                foreach (var arg in creatorMap.Arguments)
                {
                    if (values.ContainsKey(arg.Name))
                    {
                        constructorArgs.Add(values[arg.Name]);
                        contructorTypes.Add(typeof(T).GetProperty(arg.Name).PropertyType);

                        values.Remove(arg.Name);
                    }

                }

                var constructor = typeof(T).GetConstructor(contructorTypes.ToArray());
                result = (T)constructor.Invoke(constructorArgs.ToArray());
            }
            else
                result = new T();

            // Map fields
            foreach (var value in values)
            {
                var memberMap = _ClassMap.GetMemberMapForElement(value.Key);
                memberMap.Setter(value.Key, value.Value);
            }

            return result;
        }
         
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IEffectivePropertyValues<T> value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            context.Writer.WriteName("date");
            BsonSerializer.Serialize<Date>(bsonWriter, value.EffectivePeriod.FromDate);

            context.Writer.WriteName("properties");
            BsonSerializer.Serialize<T>(bsonWriter, value.Properties);

            bsonWriter.WriteEndDocument();
        }
    }
}
