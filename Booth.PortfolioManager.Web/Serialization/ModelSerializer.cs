using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


namespace Booth.PortfolioManager.Web.Serialization
{
    public interface IModelSerializer
    {
        string Serialize(object obj);
        string Serialize<T>(T obj);
        void Serialize(StreamWriter streamWriter, object obj);
        void Serialize<T>(StreamWriter streamWriter, T obj);

        T Deserialize<T>(string source);
        T Deserialize<T>(StreamReader streamReader);
    }
    public class ModelSerializer : IModelSerializer
    {
        private readonly JsonSerializer _Serializer;
        public ModelSerializer()
        {
            _Serializer = JsonSerializer.CreateDefault(SerializerSettings.Settings);
        }

        public T Deserialize<T>(string source)
        {
            using (var textReader = new StringReader(source))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return _Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public T Deserialize<T>(StreamReader streamReader)
        {
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var obj = _Serializer.Deserialize<T>(jsonReader);

                return obj;
            }
        }

        public string Serialize(object obj)
        {
            var textWriter = new StringWriter();
            _Serializer.Serialize(textWriter, obj);

            return textWriter.ToString();
        }

        public string Serialize<T>(T obj)
        {
            var textWriter = new StringWriter();
            _Serializer.Serialize(textWriter, obj, typeof(T));

            return textWriter.ToString();
        }

        public void Serialize(StreamWriter streamWriter, object obj)
        {
            _Serializer.Serialize(streamWriter, obj);
        }

        public void Serialize<T>(StreamWriter streamWriter, T obj)
        {
            _Serializer.Serialize(streamWriter, obj, typeof(T));
        }
    }
}
