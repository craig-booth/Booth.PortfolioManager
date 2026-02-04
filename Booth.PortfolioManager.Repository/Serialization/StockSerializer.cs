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

namespace Booth.PortfolioManager.Repository.Serialization
{
    class StockSerializer : SerializerBase<Stock>
    {
        public override Stock Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            Guid id = Guid.Empty;
            Date listingDate = Date.MinValue;
            Date delistingDate = Date.MinValue;
            bool trust = false;
            IEffectiveProperties<StockProperties> effectiveProperties = null;
            IEffectiveProperties<DividendRules> dividendRules = null;
            List<CorporateAction> corporateActions = null;

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                switch (name)
                {
                    case "_id":
                        id = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "listingDate":
                        listingDate = BsonSerializer.Deserialize<Date>(bsonReader);
                        break;
                    case "delistingDate":
                        delistingDate = BsonSerializer.Deserialize<Date>(bsonReader);
                        break;
                    case "trust":
                        trust = bsonReader.ReadBoolean();
                        break;
                    case "properties":
                        effectiveProperties = BsonSerializer.Deserialize<IEffectiveProperties<StockProperties>>(bsonReader);
                        break;
                    case "dividendRules":
                        dividendRules = BsonSerializer.Deserialize<IEffectiveProperties<DividendRules>>(bsonReader);
                        break;
                    case "corporateActions":
                        corporateActions = BsonSerializer.Deserialize<List<CorporateAction>>(bsonReader);
                        break;

                }
            }

            var stock = new Stock(id);

            if (listingDate != Date.MinValue)
            {
                var listingProperties = effectiveProperties[listingDate];
                stock.List(listingProperties.AsxCode, listingProperties.Name, listingDate, trust, listingProperties.Category);
            }

            if (effectiveProperties != null)
            {
                foreach (var effectiveProperty in effectiveProperties.Values.Reverse().Skip(1))
                {
                    var property = effectiveProperty.Properties;
                    stock.ChangeProperties(effectiveProperty.EffectivePeriod.FromDate, property.AsxCode, property.Name, property.Category);
                }
            }

            if (dividendRules != null)
            {
                foreach (var dividendRule in dividendRules.Values.Reverse())
                {
                    var property = dividendRule.Properties;

                    stock.ChangeDividendRules(dividendRule.EffectivePeriod.FromDate, property.CompanyTaxRate, property.DividendRoundingRule, property.DrpActive, property.DrpMethod);
                }
            }

            if (corporateActions != null)
            {
                foreach (var action in corporateActions)
                {
                    stock.CorporateActions.Add(action);
                }
            }

            if (delistingDate != Date.MinValue)
            {
                stock.DeList(delistingDate);
            }

            return stock;            
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Stock value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("_id");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);

            if (value.EffectivePeriod.FromDate != Date.MinValue)
            {
                bsonWriter.WriteName("listingDate");
                BsonSerializer.Serialize<Date>(bsonWriter, value.EffectivePeriod.FromDate);
            }

            if (value.EffectivePeriod.ToDate != Date.MaxValue)
            {
                bsonWriter.WriteName("delistingDate");
                BsonSerializer.Serialize<Date>(bsonWriter, value.EffectivePeriod.ToDate);
            }

            bsonWriter.WriteName("trust");
            bsonWriter.WriteBoolean(value.Trust);

            bsonWriter.WriteName("properties");
            BsonSerializer.Serialize<IEffectiveProperties<StockProperties>>(bsonWriter, value.Properties);

            bsonWriter.WriteName("dividendRules");
            BsonSerializer.Serialize<IEffectiveProperties<DividendRules>>(bsonWriter, value.DividendRules);

            bsonWriter.WriteName("corporateActions");
            BsonSerializer.Serialize<ITransactionList<CorporateAction>>(bsonWriter, value.CorporateActions);

            bsonWriter.WriteEndDocument();
        }
    }
}
