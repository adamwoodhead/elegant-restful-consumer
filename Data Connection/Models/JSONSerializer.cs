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
        private readonly JsonSerializerSettings settings;

        public JSONSerializer()
        {
            settings = new JsonSerializerSettings
            {
                // Configure settings as needed (e.g., snake_case, null handling)
                // By default, Newtonsoft.Json respects JsonProperty attributes
            };
        }

        public string Serialize(object obj) => JsonConvert.SerializeObject(obj, settings);

        public string Serialize(Parameter parameter) => JsonConvert.SerializeObject(parameter.Value, settings);

#nullable enable
        public T? Deserialize<T>(RestResponse response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content, settings);
            }
            catch (Exception)
            {
                try
                {
                    string wrappedContent = $"[{response.Content}]";
                    return JsonConvert.DeserializeObject<T>(wrappedContent, settings);
                }
                catch (Exception)
                {
                    return default;
                }
            }
        }
#nullable disable

        public ContentType ContentType { get; set; } = ContentType.Json;

        public DataFormat DataFormat { get; } = DataFormat.Json;

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