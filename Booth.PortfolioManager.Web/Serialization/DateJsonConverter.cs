using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Booth.Common;

namespace Booth.PortfolioManager.Web.Serialization
{
    class DateJsonConverter : JsonConverter<Date>
    {
        public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Date.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToIsoDateString());
        }
    }
}

