using Microsoft.AspNetCore.Mvc.Formatters;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace Booth.PortfolioManager.Web.Serialization
{
    public interface IModelSerializer
    {
        string Serialize(object obj);
        string Serialize<T>(T obj);
        void Serialize(StreamWriter streamWriter, object obj);
        void Serialize(Stream stream, object obj);
        void Serialize<T>(StreamWriter streamWriter, T obj);
        void Serialize<T>(Stream stream, T obj);

        T Deserialize<T>(string source);
        T Deserialize<T>(StreamReader streamReader);
        T Deserialize<T>(Stream stream);
    }
    public class ModelSerializer : IModelSerializer
    {
        private readonly JsonSerializerOptions _Options;

        public ModelSerializer()
        {
            _Options = SerializerSettings.JsonSerializerOptions;
        }

        public T Deserialize<T>(string source)
        {
            return JsonSerializer.Deserialize<T>(source, _Options);
        }

        public T Deserialize<T>(StreamReader streamReader)
        {
            return Deserialize<T>(streamReader.BaseStream);
        }

        public T Deserialize<T>(Stream stream)
        {
            var obj = JsonSerializer.Deserialize<T>(stream, _Options);

            return obj;
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _Options);
        }

        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, typeof(T), _Options);
        }

        public void Serialize(StreamWriter streamWriter, object obj)
        {
            Serialize(streamWriter.BaseStream, obj);
        }

        public void Serialize(Stream stream, object obj)
        {
            JsonSerializer.Serialize(stream, obj, _Options);
        }

        public void Serialize<T>(StreamWriter streamWriter, T obj)
        {
            Serialize<T>(streamWriter.BaseStream, obj);
        }

        public void Serialize<T>(Stream stream, T obj)
        {
            JsonSerializer.Serialize<T>(stream, obj, _Options);
        }
    }
}
