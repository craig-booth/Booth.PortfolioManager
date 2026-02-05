

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booth.PortfolioManager.Web.Serialization
{
    public static class SerializerSettings
    {
        public static JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                var options = new JsonSerializerOptions();
                ConfigureOptions(options);

                return options;
            }

        }
        public static void ConfigureOptions(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(new DateJsonConverter());
            options.Converters.Add(new TimeJsonConverter());
            options.Converters.Add(new TransactionConverter());
            options.Converters.Add(new CorporateActionConverter());
            options.Converters.Add(new CorporateActionListConverter());
        }
    }
}
