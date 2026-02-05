using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Booth.Common;


namespace Booth.PortfolioManager.Web.Serialization
{
    class TimeJsonConverter : JsonConverter<Time>
    {
        public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Time.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(@"HH\:mm\:ss"));
        }
    }
}

