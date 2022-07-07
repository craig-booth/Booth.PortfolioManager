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

            Guid id;
            Date listingDate = Date.MinValue;
            Date delistingDate = Date.MinValue;
            bool trust = false;
            IEffectiveProperties<StockProperties> effectiveProperties = null;
            IEffectiveProperties<DividendRules> dividendRules = null;
            List<CorporateAction> corporateActions = null;

            List<StapledSecurityChild> childSecurities = null;
            IEffectiveProperties<RelativeNTA> relativeNTAs = null;

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
                    case "childSecurities":
                        childSecurities = BsonSerializer.Deserialize<List<StapledSecurityChild>>(bsonReader);
                        break;
                    case "relativeNTAs":
                        relativeNTAs = BsonSerializer.Deserialize<IEffectiveProperties<RelativeNTA>>(bsonReader);
                        break;
                }
            }

            Stock stock = null;
            if (childSecurities.Count == 0)
            {
                stock = new Stock(id);

                if (listingDate != Date.MinValue)
                {
                    var listingProperties = effectiveProperties[listingDate];
                    stock.List(listingProperties.AsxCode, listingProperties.Name, listingDate, trust, listingProperties.Category);
                }
            }
            else
            {
                var stapledSecurity = new StapledSecurity(id);

                if (listingDate != Date.MinValue)
                {
                    var listingProperties = effectiveProperties[listingDate];
                    stapledSecurity.List(listingProperties.AsxCode, listingProperties.Name, listingDate, listingProperties.Category, childSecurities);
                }

                foreach (var relativeNTA in relativeNTAs.Values.Reverse())
                    stapledSecurity.SetRelativeNTAs(relativeNTA.EffectivePeriod.FromDate, relativeNTA.Properties.Percentages);

                stock = stapledSecurity;
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
                    stock.CorporateActions.Add(action);
            }

            if (delistingDate != Date.MaxValue)
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

            if (value is StapledSecurity stapledSecurity)
            {
                bsonWriter.WriteName("childSecurities");
                BsonSerializer.Serialize<IEnumerable<StapledSecurityChild>>(bsonWriter, stapledSecurity.ChildSecurities);

                bsonWriter.WriteName("relativeNTAs");
                BsonSerializer.Serialize<IEffectiveProperties<RelativeNTA>>(bsonWriter, stapledSecurity.RelativeNTAs);
            }
            else
            {

                bsonWriter.WriteName("trust");
                bsonWriter.WriteBoolean(value.Trust);
            }

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
