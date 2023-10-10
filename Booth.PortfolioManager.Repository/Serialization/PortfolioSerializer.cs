﻿using System;
using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using Booth.Common;
using Booth.PortfolioManager.Domain;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Repository.Serialization
{
    class PortfolioSerializer : SerializerBase<Portfolio>
    {
        private IPortfolioFactory _Factory;
        public PortfolioSerializer(IPortfolioFactory factory)
        {
            _Factory = factory;
        }

        public override Portfolio Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            Guid id;
            string portfolioName = "";
            Guid owner;
            List<PortfolioTransaction> transactions = null;
            List<Guid> participateInDrp = null;

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                switch (name)
                {
                    case "_id":
                        id = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "name":
                        portfolioName = bsonReader.ReadString();
                        break;
                    case "owner":
                        owner = BsonSerializer.Deserialize<Guid>(bsonReader);
                        break;
                    case "transactions":
                        transactions = BsonSerializer.Deserialize<List<PortfolioTransaction>>(bsonReader);
                        break;

                    case "participateInDrp":
                        participateInDrp = BsonSerializer.Deserialize<List<Guid>>(bsonReader);
                        break;
                }
            }

            if (_Factory == null)
                throw new Exception("Help");
            var portfolio = _Factory.CreatePortfolio(id);
            if (portfolio == null)
                throw new Exception("Help !!!!!!!!");
            portfolio.Create(portfolioName, owner);

            if (transactions != null)
            { 
                portfolio.AddTransactions(transactions);
            }

            if (participateInDrp != null)
            {
                foreach (var stockId in participateInDrp)
                {
                    portfolio.ChangeDrpParticipation(stockId, true);
                }
            }

            return portfolio; 
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Portfolio value)
        {
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            bsonWriter.WriteName("_id");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Id);

            bsonWriter.WriteName("name");
            bsonWriter.WriteString(value.Name);

            bsonWriter.WriteName("owner");
            BsonSerializer.Serialize<Guid>(bsonWriter, value.Owner);

            bsonWriter.WriteName("participateInDrp");
            bsonWriter.WriteStartArray();
            foreach (var holding in value.Holdings.All())
            {
                if (holding.Settings.ParticipateInDrp)
                    BsonSerializer.Serialize<Guid>(bsonWriter, holding.Id);
            }
            bsonWriter.WriteEndArray();

            bsonWriter.WriteName("transactions");
            BsonSerializer.Serialize<ITransactionList<IPortfolioTransaction>>(bsonWriter, value.Transactions);

            bsonWriter.WriteEndDocument(); 
        }
    }
}
