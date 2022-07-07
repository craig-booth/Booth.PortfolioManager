using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.CorporateActions;

namespace Booth.PortfolioManager.Repository.Serialization
{

    class TransactionListSerializer<T> : SerializerBase<ITransactionList<T>>, IBsonArraySerializer where T : ITransaction
    {
        private IBsonSerializer<T> _Serializer;
    //    private IBsonSerializer<Date> _DateSerializer;
     //   private BsonClassMap _ClassMap;

        public TransactionListSerializer()
        {
            _Serializer = BsonSerializer.LookupSerializer<T>();
      //      _DateSerializer = BsonSerializer.LookupSerializer<Date>();
      //      _ClassMap = BsonClassMap.LookupClassMap(typeof(T));
        }

        public override ITransactionList<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {       
            throw new NotSupportedException();
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ITransactionList<T> value)
        {
            context.Writer.WriteStartArray();
            foreach (var transaction in value)
            {
                var serializer = BsonSerializer.LookupSerializer<T>();
                serializer.Serialize(context, transaction);
            }
            context.Writer.WriteEndArray();
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
