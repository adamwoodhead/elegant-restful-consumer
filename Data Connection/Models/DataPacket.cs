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

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        #region Pagination

        public static async Task<PaginatedCollection<T>> PaginateAsync()
        {
            return await PaginatedCollection<T>.InstantiateIndex();
        }

        public static async Task<PaginatedCollection<T>> PaginateRelativeAsync(string extension, int? id = null)
        {
            return await PaginatedCollection<T>.InstantiateExtension(id, extension);
        }

        public static async Task<PaginatedCollection<T>> PaginateSearchAsync(string haystack, string needle)
        {
            return await PaginatedCollection<T>.InstantiateSearch(haystack, needle);
        }

        public static async Task<PaginatedCollection<G>> GetRelatedModelPaginationAsync<G>(string relation, int? id)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<PaginatedCollection<G>> restResponse = await DataConnection.RequestAsync<PaginatedCollection<G>>(request, default);

            return restResponse.Data;
        }


        #endregion

        #region Relations

        public static async Task<G> GetRelatedModelAsync<G>(string relation, int? id)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<G> restResponse = await DataConnection.RequestAsync<G>(request, default);

            return restResponse.Data;
        }

        public static async Task<G> PostRelatedModelAsync<G>(string relation, int? id, G obj)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddJsonBody(obj);

            IRestResponse<G> restResponse = await DataConnection.RequestAsync<G>(request, default);

            return restResponse.Data;
        }

        public static async Task<List<G>> GetRelatedModelListAsync<G>(string relation, int? id)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<List<G>> restResponse = await DataConnection.RequestAsync<List<G>>(request, default);

            return restResponse.Data;
        }

        #endregion

        #region GET

        public static async Task<T> GetAsync(int? id) => await GetAsync((int)id);

        public static async Task<T> GetAsync(int id)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(id);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, default);

            return restResponse.Data;
        }

        public static async Task<List<T>> GetAsync()
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetIndexRoute();

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<List<T>> restResponse = await DataConnection.RequestAsync<List<T>>(request, default);

            return restResponse.Data;
        }

        public static async Task<List<T>> SearchAsync(string haystack, string needle)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSearchRoute(haystack, needle);

            RestRequest request = new RestRequest(url, Method.GET, DataFormat.Json);

            IRestResponse<List<T>> restResponse = await DataConnection.RequestAsync<List<T>>(request, default);

            return restResponse.Data;
        }

        #endregion

        #region POST
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

            string url = routeAttribute.GetIndexRoute();

            RestRequest request = new RestRequest(url, Method.POST, DataFormat.Json);

            request.AddJsonBody(this);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            this.ID = (restResponse.Data as DataPacket<T>).ID;
            this.CreatedAt = (restResponse.Data as DataPacket<T>).CreatedAt;
            this.UpdatedAt = (restResponse.Data as DataPacket<T>).UpdatedAt;

            return restResponse.Data;
        }
        #endregion

        #region UPDATE

        public async Task<T> UpdateAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest(url, Method.PUT, DataFormat.Json);

            request.AddJsonBody(this);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            this.UpdatedAt = (restResponse.Data as DataPacket<T>).UpdatedAt;

            return restResponse.Data;
        }

        #endregion

        #region DELETE

        public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest(url, Method.DELETE, DataFormat.Json);

            IRestResponse<T> restResponse = await DataConnection.RequestAsync<T>(request, cancellationToken);

            return restResponse.IsSuccessful;
        }

        #endregion
    }
}
