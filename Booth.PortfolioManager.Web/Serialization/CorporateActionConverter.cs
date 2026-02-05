using Booth.Common;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using Booth.PortfolioManager.Web.Models.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booth.PortfolioManager.Web.Serialization
{
    class CorporateActionConverter : JsonConverter<CorporateAction>
    {
        private Dictionary<CorporateActionType, Type> _ActionTypes = new Dictionary<CorporateActionType, Type>();
        public CorporateActionConverter()
        {
            foreach (var actionType in TypeUtils.GetSubclassesOf(typeof(CorporateAction), true))
            {
                var action = Activator.CreateInstance(actionType) as CorporateAction;
                _ActionTypes.Add(action.Type, actionType);
            }
        }

        public override CorporateAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                throw new JsonException("Type field is missing. Unable to determine the type of the corporate action");

            if (!Enum.TryParse(typeof(CorporateActionType), typeProperty, true, out var corporateActionType))
                throw new JsonException($"Unknown corporate action type {typeProperty}");

            var optionsWithOutTransactionConverter = new JsonSerializerOptions(options);
            optionsWithOutTransactionConverter.Converters.Remove(optionsWithOutTransactionConverter.Converters.FirstOrDefault(c => c is CorporateActionConverter));
            var corporateAction = JsonSerializer.Deserialize(ref reader, _ActionTypes[(CorporateActionType)corporateActionType], optionsWithOutTransactionConverter);

            return (CorporateAction)corporateAction;
        }

        public override void Write(Utf8JsonWriter writer, CorporateAction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
       
    }
}
