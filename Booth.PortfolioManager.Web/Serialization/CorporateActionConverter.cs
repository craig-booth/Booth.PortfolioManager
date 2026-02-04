using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

using Booth.Common;
using Booth.PortfolioManager.Web.Models.CorporateAction;
using Newtonsoft.Json.Linq;


namespace Booth.PortfolioManager.Web.Serialization
{
    class CorporateActionConverter : JsonConverter
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

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(CorporateAction));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JToken.ReadFrom(reader) as JObject;

            if (!jObject.TryGetValue("type", out var jToken))
                throw new JsonReaderException("Type field is missing. Unable to determine the type of the corporate action");

            CorporateActionType type;
            try
            {
                type = jToken.ToObject<CorporateActionType>();
            }
            catch
            {
                throw new JsonReaderException("Type field is invalid. Unable to determine the type of the corporate action");
            }

            var action = Activator.CreateInstance(_ActionTypes[type]);

            serializer.Populate(jObject.CreateReader(), action);

            return action;
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
