using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace SE.Core.Extensions
{
    public static class SerializerExtensions
    {
        private static JsonSerializerSettings serializerOptions;

        public static string Serialize<T>(this T obj, bool compress = false, JsonSerializerSettings options = null)
        {
            JsonSerializerSettings o = options ?? serializerOptions;
            if (!compress) 
                return JsonConvert.SerializeObject(obj, o);

            string str = JsonConvert.SerializeObject(obj, o);
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress)) {
                msBytes.CopyTo(gs);
                return str;
            }
        }

        public static T Deserialize<T>(this string jsonString, bool compress = false, JsonSerializerSettings options = null)
        {
            JsonSerializerSettings o = options ?? serializerOptions;
            if(!compress)
                return JsonConvert.DeserializeObject<T>(jsonString, o);

            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            using (MemoryStream msBytes = new MemoryStream(bytes))
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream gs = new GZipStream(msBytes, CompressionMode.Decompress)) {
                gs.CopyTo(ms);
                return JsonConvert.DeserializeObject<T>(jsonString, o);
            }
        }

        static SerializerExtensions()
        {
            serializerOptions = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }
    }
}