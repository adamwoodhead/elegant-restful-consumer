using LogHandler;
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

        [JsonProperty("expires_at")]
        public int ExpiresAt { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();

        public AuthenticationPacket() { }

        public void BeginAutoRefreshAsync()
        {
            Task.Run(async () => {
                Log.Verbose("Token Refresh Task Started");

                while (DataConnection.CurrentUser?.Authentication?.Equals(this) ?? false)
                {
                    Log.Verbose($"Now: {DateTime.UtcNow}");
                    Log.Verbose($"Expires At: {Helper.UnixTimeStampToDateTime(ExpiresAt)}");
                    Log.Verbose($"Refresh At: {Helper.UnixTimeStampToDateTime(ExpiresAt - 30)}");
                    Log.Verbose($"Expires In: {ExpiresIn}s");
                    Log.Verbose($"Refresh In: {ExpiresIn - 30}s (30 seconds prior)");

                    await Task.Delay((ExpiresIn - 30) * 1000, CancellationTokenSource?.Token ?? default);
                    CancellationTokenSource?.Token.ThrowIfCancellationRequested();

                    Log.Verbose("Attempting Token Refresh");
                    try
                    {
                        DataConnection.IsRefreshing = true;
                        await DataConnection.CurrentUser?.RefreshAsync(CancellationTokenSource?.Token ?? default);
                        DataConnection.IsRefreshing = false;
                    }
                    catch (Exception)
                    {
                        DataConnection.IsRefreshing = false;
                    }
                }
            });
        }

        public async Task<Authenticatable> GetAuthenticatedUser(CancellationToken cancellationToken = default)
        {
            string url = $"{DataConnection.BaseURL}{Authenticatable.UserProfileRoute}";

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddHeader("Authorization", $"bearer {AccessToken}");

            IRestResponse<Authenticatable> restResponse = await DataConnection.RestClient.ExecuteAsync<Authenticatable>(request, cancellationToken);

            return restResponse.Data;
        }
    }
}
