using Booth.Common;
using Booth.PortfolioManager.Web.Models.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booth.PortfolioManager.Web.Serialization
{
    class TransactionConverter : JsonConverter<Transaction>
    {
        private readonly Dictionary<TransactionType, Type> _TransactionTypes = [];
        public TransactionConverter()
        {
            foreach (var transactionType in TypeUtils.GetSubclassesOf(typeof(Transaction), true))
            {
                var transaction = Activator.CreateInstance(transactionType) as Transaction;
                _TransactionTypes.Add(transaction.Type, transactionType);
            }
        }

        public override Transaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerCopy = reader;

            if (readerCopy.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object");

            string typeProperty = null;
            while (readerCopy.Read())
            {
                if (readerCopy.TokenType == JsonTokenType.EndObject)
                    break;

                if (readerCopy.TokenType == JsonTokenType.PropertyName && readerCopy.GetString() == "type")
                {
                    readerCopy.Read();
                    typeProperty = readerCopy.GetString();
                    break;
                }
            }

            if (typeProperty == null)
                throw new JsonException("Type field is missing. Unable to determine the type of the transaction");

            if (!Enum.TryParse(typeof(TransactionType), typeProperty, true, out var transactionType))
                throw new JsonException($"Unknown tranasction type {typeProperty}");

            var optionsWithTransactionConverter = new JsonSerializerOptions(options);
            optionsWithTransactionConverter.Converters.Remove(optionsWithTransactionConverter.Converters.FirstOrDefault(c => c is TransactionConverter));
            var transaction = JsonSerializer.Deserialize(ref reader, _TransactionTypes[(TransactionType)transactionType], optionsWithTransactionConverter);

            return (Transaction)transaction;
        }

        public override void Write(Utf8JsonWriter writer, Transaction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

    }

}
