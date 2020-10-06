using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public AuthenticationPacket() { }

        public async Task<Authenticatable> GetAuthenticatedUser(CancellationToken cancellationToken = default)
        {
            string url = $"{DataConnection.BaseURL}/me";

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddHeader("Authorization", $"bearer {AccessToken}");

            IRestResponse<Authenticatable> restResponse = await DataConnection.RestClient.ExecuteAsync<Authenticatable>(request, cancellationToken);

            return restResponse.Data;
        }
    }
}
