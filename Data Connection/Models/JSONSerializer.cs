using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DataConnection.Models
{
    internal class JSONSerializer : IRestSerializer, ISerializer, IDeserializer
    {
        public string Serialize(object obj) => JsonConvert.SerializeObject(obj);

        public string Serialize(Parameter parameter) => JsonConvert.SerializeObject(parameter.Value);

        // public T Deserialize<T>(IRestResponse response) => JsonConvert.DeserializeObject<T>(response.Content);

        public T? Deserialize<T>(RestResponse response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception)
            {
                try
                {
                    // Lets try wrapping it, we may have a singular whilst requesting a list.

                    string wrappedContent = $"[{response.Content}]";
                    return JsonConvert.DeserializeObject<T>(wrappedContent);
                }
                catch (Exception)
                {
                    return default;
                }
            }
        }

        public ContentType ContentType { get; set; } = ContentType.Json;

        public DataFormat DataFormat { get; } = DataFormat.Json;

        // new

        public ISerializer Serializer => new JSONSerializer();

        public IDeserializer Deserializer => new JSONSerializer();

        public string[] AcceptedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "*+json"
        };

        public SupportsContentType SupportsContentType => contentType => GetContentType(contentType);

        static bool GetContentType(ContentType contentType)
        {
            return contentType == ContentType.Json;
        }
    }
}
