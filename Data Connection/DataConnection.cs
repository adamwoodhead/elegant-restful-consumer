using DataConnection.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection
{
    public static class DataConnection
    {
        public static string BaseURL { get; private set; }

        private static bool IsInitialized { get; set; } = false;

        internal static RestClient RestClient { get; set; }

        public static Func<AuthenticationPacket> UnauthorizedCallBack { get; set; }

        internal static bool AutoAttemptLogin { get; set; }

        internal static bool HasAttemptedRefresh { get; set; }

        internal static Authenticatable CurrentUser { get; set; }

        public static RollingCounterCollection RollingCounterCollection { get; set; }

        public static void Initialize(string baseRoute, Func<AuthenticationPacket> authCallback, bool autoAttemptLoginRefreshes)
        {
            if (!IsInitialized)
            {
                AutoAttemptLogin = autoAttemptLoginRefreshes;
                System.Net.WebRequest.DefaultWebProxy = null;
                ServicePointManager.UseNagleAlgorithm = false;
                RollingCounterCollection = new RollingCounterCollection(1000);

                BaseURL = baseRoute;
                RestClient = new RestClient(BaseURL)
                {
                    Proxy = null,
                    ThrowOnAnyError = true
                };
                RestClient.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                RestClient.UseSerializer(() => new JSONSerializer());
                UnauthorizedCallBack = authCallback;
            }

            IsInitialized = true;
        }

        public static async Task<IRestResponse<T>> RequestAsync<T>(RestRequest restRequest, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            if (CurrentUser != null && !HasAttemptedRefresh)
            {
                restRequest.AddHeader("Authorization", $"bearer {CurrentUser.Authentication.AccessToken}");
            }

            IRestResponse<T> restResponse = await RestClient.ExecuteAsync<T>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"{restResponse.StatusCode} : {restResponse.Content}");

                    if (CurrentUser?.Authentication != null && AutoAttemptLogin && HasAttemptedRefresh == false)
                    {
                        HasAttemptedRefresh = true;
                        await CurrentUser.RefreshAsync();

                        return await RequestAsync<T>(restRequest, cancellationToken);
                    }
                    else
                    {
                        AuthenticationPacket authenticatedPacket = UnauthorizedCallBack.Invoke();

                        if (authenticatedPacket != null)
                        {
                            CurrentUser = await authenticatedPacket.GetAuthenticatedUser();
                            CurrentUser.Authentication = authenticatedPacket;
                            HasAttemptedRefresh = false;
                        }

                        return await RequestAsync<T>(restRequest, cancellationToken);
                    }
                }
                else
                {
                    Console.WriteLine($"{restResponse.StatusCode} : {restResponse.Content}");
                    throw new Exception($"{restResponse.StatusCode} : {restResponse.Content}");
                }
            }

#if DEBUG
            stopwatch.Stop();
            RollingCounterCollection.Slip<T>(stopwatch.Elapsed.TotalMilliseconds);
            RollingCounterCollection.Report<T>(restRequest.Method.ToString(), stopwatch.Elapsed.TotalMilliseconds);
#endif

            return restResponse;
        }
    }
}
