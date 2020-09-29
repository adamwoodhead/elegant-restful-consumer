using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection.Models
{
    [JsonObject]
    public class DataPacket<T> : IDataPacket
    {
        [JsonProperty("id")]
        public int? ID { get; set; } = null;

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Incredibly intensive workload, use with caution
        /// </summary>
        /// <returns></returns>
        public static async Task<List<T>> GetAllParallelAsync()
        {
            PaginatedCollection<T> paginatedCollection = await PaginateAsync();

            List<T> completeData = new List<T>();

            List<Task<List<T>>> tasks = new List<Task<List<T>>>();

            foreach (Page<T> page in paginatedCollection.Pages)
            {
                tasks.Add(
                    Task.Run(() => page.GetDataOrInitializeAsync())
                );
            }

            await Task.WhenAll(tasks);

            foreach (Page<T> page in paginatedCollection.Pages)
            {
                completeData.AddRange(page.Data);
            }

            return completeData;
        }

        public static async Task<PaginatedCollection<T>> PaginateAsync()
        {
            return await PaginatedCollection<T>.Instantiate(null, null, null);
        }

        public static async Task<PaginatedCollection<T>> PaginateBy(string extension, int? id)
        {
            return await PaginatedCollection<T>.Instantiate(extension, id, null);
        }

        public static async Task<PaginatedCollection<T>> PaginateSearch(string searchTerm)
        {
            return await PaginatedCollection<T>.Instantiate(null, null, searchTerm);
        }

        public static async Task<PaginatedCollection<T>> PaginateByAndSearch(string extension, int? id, string searchTerm)
        {
            return await PaginatedCollection<T>.Instantiate(extension, id, searchTerm);
        }

        public static async Task<G> GetRelatedModelAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<G> restResponse = await DataConnection.RequestAsync<G>(request, cancellationToken);

            return restResponse.Data;
        }

        public static async Task<List<G>> GetRelatedModelListAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<List<G>> restResponse = await DataConnection.RequestAsync<List<G>>(request, cancellationToken);

            return restResponse.Data;
        }

        public static async Task<PaginatedCollection<G>> GetRelatedModelPaginationAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<PaginatedCollection<G>> restResponse = await DataConnection.RequestAsync<PaginatedCollection<G>>(request, cancellationToken);

            return restResponse.Data;
        }

        public static async Task<T> GetAsync(int? id, CancellationToken cancellationToken = default) => await GetAsync((int)id, cancellationToken);

        public static async Task<T> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(id);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            return restResponse.Data;
        }

        public async Task<G> PostRelatedAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddJsonBody(this);

            IRestResponse<G> restResponse = await DataConnection.RequestAsync<G>(request, cancellationToken);

            return restResponse.Data;
        }

        public async Task<T> CreateAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.IndexRoute;

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddJsonBody(this);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            return restResponse.Data;
        }

        public async Task<T> UpdateAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest(url, Method.PUT, DataFormat.Json);

            request.AddJsonBody(this);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            return restResponse.Data;
        }

        public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest(url, Method.DELETE, DataFormat.Json);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            return restResponse.IsSuccessful;
        }
    }
}
