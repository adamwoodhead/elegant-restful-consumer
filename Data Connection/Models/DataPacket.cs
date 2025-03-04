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
    public interface IDataPacket
    {
        int? ID { get; set; }
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }

    [JsonObject]
    public class DataPacket<T> : IDataPacket
    {
        [JsonProperty("id")]
        public int? ID { get; set; } = null;

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("created_by")]
        public int? CreatedBy { get; set; }

        [JsonProperty("updated_by")]
        public int? UpdatedBy { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        #region Pagination

        public static async Task<PaginatedCollection<T>> PaginateAsync(CancellationToken cancellationToken = default)
        {
            return await PaginatedCollection<T>.InstantiateIndex(cancellationToken);
        }

        public static async Task<PaginatedCollection<T>> PaginateRelativeAsync(string extension, int? id = null, CancellationToken cancellationToken = default)
        {
            return await PaginatedCollection<T>.InstantiateExtension(id, extension, cancellationToken);
        }

        public static async Task<PaginatedCollection<T>> PaginateSearchAsync(string haystack, string needle, CancellationToken cancellationToken = default)
        {
            return await PaginatedCollection<T>.InstantiateSearch(haystack, needle, cancellationToken);
        }

        public static async Task<PaginatedCollection<G>> GetRelatedModelPaginationAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest { Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<PaginatedCollection<G>>(request, cancellationToken);
        }


        #endregion

        #region Relations

        public static async Task<G> GetRelatedModelAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest { Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<G>(request, cancellationToken);
        }

        public static async Task<G> PostRelatedModelAsync<G>(string relation, int? id, G obj, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest { Resource = url, Method = Method.Post, RequestFormat = DataFormat.Json };

            request.AddJsonBody(JsonConvert.SerializeObject(obj));

            return await DataConnection.RequestAsync<G>(request, cancellationToken);
        }

        public static async Task<List<G>> GetRelatedModelListAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<List<G>>(request, cancellationToken);
        }

        #endregion

        #region GET

        public static async Task<T> GetAsync(int? id, CancellationToken cancellationToken = default) => await GetAsync((int)id, cancellationToken);

        public static async Task<T> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(id);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<T>(request, cancellationToken);
        }

        public static async Task<List<T>> GetAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetIndexRoute();

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<List<T>>(request, cancellationToken) ?? new List<T>();
        }

        public static async Task<List<T>> SearchAsync(string haystack, string needle, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSearchRoute(haystack, needle);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Get, RequestFormat = DataFormat.Json };

            return await DataConnection.RequestAsync<List<T>>(request, cancellationToken);
        }

        #endregion

        #region POST

        public virtual async Task<G> PostRelatedAsync<G>(string relation, int? id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetRelationshipRoute(id, relation);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Post, RequestFormat = DataFormat.Json };

            request.AddJsonBody(this);

            return await DataConnection.RequestAsync<G>(request, cancellationToken);
        }

        public virtual async Task<T> CreateAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetIndexRoute();

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Post, RequestFormat = DataFormat.Json };

            request.AddJsonBody(this);

            var data  = await DataConnection.RequestAsync<T>(request, cancellationToken);

            this.ID = (data as DataPacket<T>)?.ID;
            this.CreatedAt = (data as DataPacket<T>)?.CreatedAt;
            this.UpdatedAt = (data as DataPacket<T>)?.UpdatedAt;

            return data;
        }

        #endregion

        #region UPDATE

        public virtual async Task<T> UpdateAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Put, RequestFormat = DataFormat.Json };

            request.AddJsonBody(this);

            var data = await DataConnection.RequestAsync<T>(request, cancellationToken);

            this.UpdatedAt = (data as DataPacket<T>).UpdatedAt;

            return data;
        }

        #endregion

        #region DELETE

        public static async Task<bool> DeleteAsync(int? id, CancellationToken cancellationToken = default) => await DeleteAsync((int)id, cancellationToken);

        public static async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(id);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Delete, RequestFormat = DataFormat.Json };

            try
            {
                await DataConnection.RequestAsync<T>(request, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = this.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetSingularRoute(ID);

            RestRequest request = new RestRequest{ Resource = url, Method = Method.Delete, RequestFormat = DataFormat.Json }   ;

            try
            {
                await DataConnection.RequestAsync<T>(request, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
