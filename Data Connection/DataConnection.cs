using DataConnection.Models;
using LogHandler;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ContentType = RestSharp.ContentType;
using System.Text.Json;
using Newtonsoft.Json.Serialization;

namespace DataConnection
{
    public static class DataConnection
    {
        private static Authenticatable currentUser;

        public static string BaseURL { get; private set; }

        private static bool IsInitialized { get; set; } = false;

        internal static RestClient RestClient { get; set; }

        internal static bool IsRefreshing { private get; set; }

        public static Func<AuthenticationPacket> UnauthorizedCallBack { get; set; }

        public static Func<bool> NotConnectedCallBack { get; set; }

        public static event EventHandler CurrentUserChanged;

        internal static bool AutoAttemptLogin { get; set; }

        public static RollingCounterCollection RollingCounterCollection { get; set; }

        public static DateTime? LoggedInAt { get; set; }

        public static Authenticatable CurrentUser
        {
            get => currentUser;
            internal set
            {
                currentUser = value;
                LoggedInAt = DateTime.UtcNow;

                OnCurrentUserChanged(value);
            }
        }

        private static void OnCurrentUserChanged(Authenticatable value)
        {
            CurrentUserChanged?.Invoke(value, null);
        }

        private static RestClientOptions RestOptions()
        {
            RestClientOptions restClientOptions = new RestClientOptions()
            {
                Proxy = null,
                ThrowOnAnyError = true
            };

            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            return restClientOptions;
        }

        private static SerializerConfig RestSerializerConfig()
        {
            SerializerConfig serializerConfig = new SerializerConfig();
            serializerConfig.UseOnlySerializer(() => new JSONSerializer());

            //DefaultContractResolver contractResolver = new DefaultContractResolver
            //{
            //    NamingStrategy = new SnakeCaseNamingStrategy()
            //};

            //serializerConfig.UseNewtonsoftJson(new JsonSerializerSettings()
            //{
            //    ContractResolver = contractResolver,
            //    Formatting = Formatting.Indented
            //});

            return serializerConfig;
        }

        public static void Initialize(string baseRoute, Func<AuthenticationPacket> authCallback = null, Func<bool> notConnectedCallback = null, bool autoAttemptLoginRefreshes = false)
        {
            if (!IsInitialized)
            {
                AutoAttemptLogin = autoAttemptLoginRefreshes;
                WebRequest.DefaultWebProxy = null;
                ServicePointManager.UseNagleAlgorithm = false;
                RollingCounterCollection = new RollingCounterCollection(1000);

                
                ConfigureRestClient configureRestClient = new ConfigureRestClient(x => RestOptions());
                ConfigureSerialization configureSerialization = new ConfigureSerialization(x => RestSerializerConfig());

                RestClient = new RestClient(baseRoute, configureRestClient, null, configureSerialization);

                UnauthorizedCallBack = authCallback;
                NotConnectedCallBack = notConnectedCallback;
            }

            IsInitialized = true;
        }

        public static async Task<bool> AttemptForcedAuthentication()
        {
            AuthenticationPacket authenticatedPacket = UnauthorizedCallBack?.Invoke();

            if (authenticatedPacket != null && !string.IsNullOrEmpty(authenticatedPacket.AccessToken))
            {
                if (CurrentUser == null)
                {
                    CurrentUser = await authenticatedPacket.GetAuthenticatedUser();
                }

                CurrentUser.Authentication = authenticatedPacket;
                CurrentUser.Authentication.BeginAutoRefreshAsync();

                return true;
            }

            return false;
        }

        public static async Task AttemptLogout()
        {
            try
            {
                if (CurrentUser?.Authentication != null)
                {
                    CurrentUser?.Authentication?.CancellationTokenSource?.Cancel();

                    await CurrentUser.LogoutAsync();

                    CurrentUser = null;
                }
            }
            catch (Exception)
            {
                CurrentUser = null;
            }
        }

        public static async Task<T> RequestAsync<T>(RestRequest restRequest, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            if (IsRefreshing)
            {
                Log.Warning("Currently refreshing token! Waiting...");

                while (IsRefreshing)
                {
                    await Task.Delay(10);
                }
            }

            if (CurrentUser?.Authentication != null)
            {
                restRequest.AddHeader("Authorization", $"bearer {CurrentUser.Authentication.AccessToken}");
            }

            //IRestResponse<T> restResponse = await RestClient.ExecuteAsync<T>(restRequest, cancellationToken);

            Debug.WriteLine(restRequest.Resource);

            RestResponse restResponse = await RestClient.ExecuteAsync(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if ((int)restResponse.StatusCode == 0)
                {
                    if (NotConnectedCallBack.Invoke())
                    {
                        RestRequest replicatedRequest = new RestRequest { Resource = restRequest.Resource, Method = restRequest.Method, RequestFormat = restRequest.RequestFormat };
                        replicatedRequest.AddBody(restRequest.Parameters.Where(x => x.ContentType == ContentType.Json).First().Value);

                        if (restRequest.Files.Count > 0)
                        {
                            restRequest.Files.ToList().ForEach(x => replicatedRequest.AddFile(x.Name, x.FileName));
                        }

                        return await RequestAsync<T>(replicatedRequest, cancellationToken);
                    }
                }
                else if((int)restResponse.StatusCode == 429 )
                {
                    Log.Warning($"We've just received a 429 error (Too Many Requests), waiting 10 seconds.");

                    await Task.Delay(10000);

                    RestRequest replicatedRequest = new RestRequest { Resource = restRequest.Resource, Method = restRequest.Method, RequestFormat = restRequest.RequestFormat };
                    replicatedRequest.AddBody(restRequest.Parameters.Where(x => x.ContentType == ContentType.Json).First().Value);

                    if (restRequest.Files.Count > 0)
                    {
                        restRequest.Files.ToList().ForEach(x => replicatedRequest.AddFile(x.Name, x.FileName));
                    }

                    return await RequestAsync<T>(replicatedRequest, cancellationToken);
                }
                else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    RestRequest replicatedRequest = new RestRequest { Resource = restRequest.Resource, Method = restRequest.Method, RequestFormat = restRequest.RequestFormat };
                    replicatedRequest.AddBody(restRequest.Parameters.Where(x => x.ContentType == ContentType.Json).First().Value);

                    if (restRequest.Files.Count > 0)
                    {
                        restRequest.Files.ToList().ForEach(x => replicatedRequest.AddFile(x.Name, x.FileName));
                    }

                    if (IsRefreshing)
                    {
                        Log.Warning("We sent this request whilst refreshing our token! Waiting...");

                        while (IsRefreshing)
                        {
                            await Task.Delay(10);
                        }

                        Log.Verbose("Finished Refresh Task, lets try again!");

                        return await RequestAsync<T>(replicatedRequest, cancellationToken);
                    }

                    CurrentUser?.Authentication?.CancellationTokenSource.Cancel();
                    CurrentUser = null;

                    Log.Verbose($"{restResponse.StatusCode} : {restResponse.Content}");

                    if (await AttemptForcedAuthentication() == false)
                    {
                        return default;
                    }

                    return await RequestAsync<T>(replicatedRequest, cancellationToken);
                }
                else if (restResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }
                else
                {
                    Log.Verbose($"{restRequest.Resource} | {restResponse.StatusCode} | {restResponse.Content}");
                    throw new Exception($"{restRequest.Resource} | {restResponse.StatusCode} | {restResponse.Content}");
                }
            }


            Log.Verbose("Starting Deserialization");

            T dataObject = JsonConvert.DeserializeObject<T>(restResponse.Content);

#if DEBUG
            stopwatch.Stop();
            RollingCounterCollection.Slip<T>(stopwatch.Elapsed.TotalMilliseconds);
            RollingCounterCollection.Report<T>(restRequest.Method.ToString(), stopwatch.Elapsed.TotalMilliseconds);
#endif

            return dataObject;
        }
    }
}
