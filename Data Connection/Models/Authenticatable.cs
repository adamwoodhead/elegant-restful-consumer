using LogHandler;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection.Models
{
    [JsonObject]
    public class Authenticatable
    {
        [JsonProperty("id")]
        public int? ID { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public AuthenticationPacket Authentication { get; set; }

        [JsonIgnore]
        public static string AuthenticateRoute => "/authenticate";

        [JsonIgnore]
        public static string RefreshRoute => "/refresh";

        [JsonIgnore]
        public static string LogoutRoute => "/logout";

        [JsonIgnore]
        public static string CurrentUserRoute => "/me";

        [JsonProperty("email")]
        public virtual string Email { get; set; }

        [JsonProperty("password")]
        public virtual string Password { get; set; }

        public Authenticatable(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public async Task<AuthenticationPacket> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            Log.Verbose("Authenticating...");

            try
            {
                string url = $"{DataConnection.BaseURL}" + AuthenticateRoute;

                RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

                request.AddJsonBody(new { email = this.Email, password = this.Password });

                IRestResponse<AuthenticationPacket> restResponse = await DataConnection.RestClient.ExecuteAsync<AuthenticationPacket>(request, cancellationToken);

                return restResponse.Data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(Authentication?.AccessToken))
            {
                Log.Verbose("Refreshing Token...");

                try
                {
                    string url = $"{DataConnection.BaseURL}" + RefreshRoute;

                    RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

                    request.AddHeader("Authorization", $"bearer {Authentication.AccessToken}");

                    IRestResponse<AuthenticationPacket> restResponse = await DataConnection.RestClient.ExecuteAsync<AuthenticationPacket>(request, cancellationToken);

                    Authentication = restResponse.Data;

                    Log.Verbose("Token Refreshed Successfully");
                }
                catch (Exception)
                {
                    Authentication = null;
                }
            }
        }

        internal async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(Authentication?.AccessToken))
            {
                Log.Verbose("Refreshing Token...");

                try
                {
                    string url = $"{DataConnection.BaseURL}" + LogoutRoute;

                    RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

                    request.AddHeader("Authorization", $"bearer {Authentication.AccessToken}");

                    IRestResponse<IDataPacket> restResponse = await DataConnection.RestClient.ExecuteAsync<IDataPacket>(request, cancellationToken);

                    Authentication = null;
                }
                catch (Exception)
                {
                    Authentication = null;
                }
            }
        }
    }
}
