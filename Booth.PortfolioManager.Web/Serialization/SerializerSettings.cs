using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Booth.PortfolioManager.Web.Serialization
{
    public static class SerializerSettings
    {
        public static JsonSerializerSettings Settings
        {
            get
            {
                var settings = new JsonSerializerSettings();

                Configure(settings);

                return settings;
            }
        }

        public static void Configure(JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            settings.Converters.Add(new DateJsonConverter());
            settings.Converters.Add(new TimeJsonConverter());
            settings.Converters.Add(new TransactionConverter());
            settings.Converters.Add(new CorporateActionConverter());
        }
    }
}
