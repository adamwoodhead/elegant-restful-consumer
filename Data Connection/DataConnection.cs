using DataConnection.Models;
using LogHandler;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection
{
    public static class DataConnection
    {
        private static Authenticatable currentUser;

        public static string BaseURL { get; private set; }

        private static bool IsInitialized { get; set; } = false;

        internal static RestClient RestClient { get; set; }

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
                LoggedInAt = DateTime.Now;

                OnCurrentUserChanged(value);
            }
        }

        private static void OnCurrentUserChanged(Authenticatable value)
        {
            CurrentUserChanged?.Invoke(value, null);
        }

        public static void Initialize(string baseRoute, Func<AuthenticationPacket> authCallback = null, Func<bool> notConnectedCallback = null, bool autoAttemptLoginRefreshes = false)
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

        public static async Task<IRestResponse<T>> RequestAsync<T>(RestRequest restRequest, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            if (CurrentUser != null)
            {
                restRequest.AddHeader("Authorization", $"bearer {CurrentUser.Authentication.AccessToken}");
            }

            IRestResponse<T> restResponse = await RestClient.ExecuteAsync<T>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if ((int)restResponse.StatusCode == 0)
                {
                    if (NotConnectedCallBack.Invoke())
                    {
                        return await RequestAsync<T>(restRequest, cancellationToken);
                    }
                }
                else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    CurrentUser?.Authentication?.CancellationTokenSource.Cancel();
                    CurrentUser = null;

                    Log.Verbose($"{restResponse.StatusCode} : {restResponse.Content}");

                    if (await AttemptForcedAuthentication() == false)
                    {
                        return null;
                    }

                    return await RequestAsync<T>(restRequest, cancellationToken);
                }
                else
                {
                    Log.Verbose($"{restResponse.StatusCode} : {restResponse.Content}");
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
