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
#pragma warning disable IDE0051 // Remove unused private members
        private static string AuthenticationToken { get; set; }
#pragma warning restore IDE0051 // Remove unused private members

        public static string BaseURL { get; private set; }

        private static bool IsInitialized { get; set; } = false;

        internal static RestClient RestClient { get; set; }

        public static RollingCounterCollection RollingCounterCollection { get; set; }

        public static void BumpUpCounter(int amount)
        {
            foreach (RollingCounter counter in RollingCounterCollection.TypeRollingCounters.Select(x => x.Value))
            {
                counter.Limit += amount;
            }
        }

        public static void BumpDownCounter(int amount)
        {
            foreach (RollingCounter counter in RollingCounterCollection.TypeRollingCounters.Select(x => x.Value))
            {
                counter.Limit = Math.Max(counter.Limit - amount, 100);
            }
        }

        public static void Initialize(string baseRoute)
        {
            if (!IsInitialized)
            {
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
            }

            IsInitialized = true;
        }

        // TODO Add Authentication
        public static bool Authenticate()
        {
            throw new NotImplementedException();
        }

        public static async Task<IRestResponse<T>> RequestAsync<T>(RestRequest restRequest, CancellationToken cancellationToken = default)
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            IRestResponse<T> restResponse = await RestClient.ExecuteAsync<T>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
               throw new Exception($"{restResponse.StatusCode} : {restResponse.Content}");
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
