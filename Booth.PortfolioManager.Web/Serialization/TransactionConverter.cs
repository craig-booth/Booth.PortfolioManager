using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.Transaction;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Booth.PortfolioManager.Web.Serialization
{
    class TransactionConverter : JsonConverter
    {
        private Dictionary<TransactionType, Type> _TransactionTypes = new Dictionary<TransactionType, Type>();
        public TransactionConverter()
        {
            foreach (var transactionType in TypeUtils.GetSubclassesOf(typeof(Transaction), true))
            {
                var transaction = Activator.CreateInstance(transactionType) as Transaction;
                _TransactionTypes.Add(transaction.Type, transactionType);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Transaction));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JToken.ReadFrom(reader) as JObject;

            if (!jObject.TryGetValue("type", out var jToken))
                throw new JsonReaderException("Type field is missing. Unable to determine the type of the transaction");

            TransactionType type;
            try
            {
                type = jToken.ToObject<TransactionType>();
            }
            catch
            {
                throw new JsonReaderException("Type field is invalid. Unable to determine the type of the transaction");
            }         

            var transaction = Activator.CreateInstance(_TransactionTypes[type]);

            serializer.Populate(jObject.CreateReader(), transaction);

            return transaction;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }

}
