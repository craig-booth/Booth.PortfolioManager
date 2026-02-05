using Booth.Common;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booth.PortfolioManager.Web.Serialization
{
    class CorporateActionListConverter : JsonConverter<List<CorporateAction>>
    {
        public override List<CorporateAction> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            // Add the CorporateActionConverter to the options if it was removed by the CorporateActionConverter
            var optionsWithTransactionConverter = new JsonSerializerOptions(options);
            if (!optionsWithTransactionConverter.Converters.Any(c => c is CorporateActionConverter))
                optionsWithTransactionConverter.Converters.Add(new CorporateActionConverter());

            var list = new List<CorporateAction>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var corporateAction = JsonSerializer.Deserialize<CorporateAction>(ref reader, optionsWithTransactionConverter);

                    if (corporateAction != null)
                        list.Add(corporateAction);
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<CorporateAction> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var corporateAction in value)
                JsonSerializer.Serialize(writer, corporateAction, corporateAction.GetType(), options);
            writer.WriteEndArray();
        }
       
    }
}
