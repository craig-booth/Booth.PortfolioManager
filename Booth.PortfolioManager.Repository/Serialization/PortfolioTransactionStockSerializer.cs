using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.CorporateActions;
using Booth.PortfolioManager.Domain.Utils;
using Booth.PortfolioManager.Domain.Portfolios;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class PortfolioTransactionStockSerializer : SerializerBase<IReadOnlyStock>
    { 
        private readonly IStockResolver _StockResolver;
        public PortfolioTransactionStockSerializer(IStockResolver stockResolver)
        {
            _StockResolver = stockResolver;
        }

        public override IReadOnlyStock Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var id = BsonSerializer.Deserialize<Guid>(bsonReader);

            if (id != Guid.Empty)
                return _StockResolver.GetStock(id);
            else
                return null;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IReadOnlyStock value)
        {
            var bsonWriter = context.Writer;
            
            if (value != null)
                BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);       
            else
                BsonSerializer.Serialize<Guid>(bsonWriter, Guid.Empty);
        }

    }
}
