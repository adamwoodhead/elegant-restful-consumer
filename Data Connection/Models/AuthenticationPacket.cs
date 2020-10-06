using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataConnection.Models
{
    [JsonObject]
    public class AuthenticationPacket
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TypeType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
