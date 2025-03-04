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
        public static string AuthenticateRoute => "/login";

        [JsonIgnore]
        public static string RefreshRoute => "/refresh";

        [JsonIgnore]
        public static string LogoutRoute => "/logout";

        [JsonIgnore]
        public static string UserProfileRoute => "/user-profile";

        [JsonProperty("first_name")]
        public virtual string FirstName { get; set; }

        [JsonProperty("last_name")]
        public virtual string LastName { get; set; }

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

                RestRequest request = new RestRequest(url, Method.Post);

                request.AddJsonBody(this);

                RestResponse restResponse = await DataConnection.RestClient.ExecuteAsync(request, cancellationToken);

                AuthenticationPacket authenticationPacket = JsonConvert.DeserializeObject<AuthenticationPacket>(restResponse.Content);

                if ((int)restResponse.StatusCode == 0)
                {
                    DataConnection.NotConnectedCallBack?.Invoke();
                    return null;
                }

                return authenticationPacket;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }

        internal async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(Authentication?.AccessToken) && !cancellationToken.IsCancellationRequested)
            {
                Log.Verbose("Refreshing Token...");

                try
                {
                    string url = $"{DataConnection.BaseURL}" + RefreshRoute;

                    RestRequest request = new RestRequest(url, Method.Post);

                    request.AddHeader("Authorization", $"bearer {Authentication?.AccessToken}");

                    RestResponse<AuthenticationPacket> restResponse = await DataConnection.RestClient.ExecuteAsync<AuthenticationPacket>(request, cancellationToken);

                    if (restResponse.IsSuccessful)
                    {
                        AuthenticationPacket tmp = restResponse.Data;

                        Authentication.AccessToken = tmp.AccessToken;
                        Authentication.ExpiresAt = tmp.ExpiresAt;
                        Authentication.ExpiresIn = tmp.ExpiresIn;
                        Authentication.TypeType = tmp.TypeType;

                        Log.Verbose("Token Refreshed Successfully");
                    }
                    else
                    {

                        Log.Error("Token Refresh Unsuccessful");
                    }
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
                Log.Verbose("Logging Out...");

                try
                {
                    string url = $"{DataConnection.BaseURL}" + LogoutRoute;

                    RestRequest request = new RestRequest(url, Method.Post);

                    request.AddHeader("Authorization", $"bearer {Authentication.AccessToken}");

                    RestResponse restResponse = await DataConnection.RestClient.ExecuteAsync(request, cancellationToken);

                    if (restResponse.IsSuccessful)
                    {
                        Log.Verbose("Token Sucessfully Invalidated");
                    }

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
