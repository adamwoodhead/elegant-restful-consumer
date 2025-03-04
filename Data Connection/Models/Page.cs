using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataConnection.Models
{
    [JsonObject]
    public class Page<T>
    {

        internal bool IsInitialized { get; set; }
        private string nonPaginatedURL;
        private int? currentPage;
        private int? totalItems;
        private int? itemsPerPage;
        private int? itemFrom;
        private int? itemTo;
        private string firstPageURL;
        private int? totalPageCount;
        private string lastPageURL;
        private string previousPageURL;
        private string nextPageURL;
        private List<T> data;

        // These properties are filled by the first request

        [JsonProperty("path")] //"path": "http://localhost:8000/api/stock-transactions",
        public string NonPaginatedURL
        {
            get => nonPaginatedURL;
            set => nonPaginatedURL = value;
        }

        [JsonProperty("current_page")] //"current_page": 3,
        public int? CurrentPage
        {
            get => currentPage;
            set => currentPage = value;
        }

        // The following must be gained via each lazy load

        [JsonProperty("total")] //"total": 2750,
        public int? TotalItems
        {
            get => totalItems;
            set => totalItems = value;
        }

        [JsonProperty("per_page")] //"per_page": 1000,
        public int? ItemsPerPage
        {
            get => itemsPerPage;
            set => itemsPerPage = value;
        }

        [JsonProperty("from")] //"from": 2001,
        public int? ItemFrom
        {
            get => itemFrom;
            set => itemFrom = value;
        }

        [JsonProperty("to")] //"to": 2750,
        public int? ItemTo
        {
            get => itemTo;
            set => itemTo = value;
        }

        [JsonProperty("first_page_url")] //"first_page_url": "http://localhost:8000/api/stock-transactions?page=1",
        public string FirstPageURL
        {
            get => firstPageURL;
            set => firstPageURL = value;
        }


        [JsonProperty("last_page")] //"last_page": 3,
        public int? TotalPageCount
        {
            get => totalPageCount;
            set => totalPageCount = value;
        }

        [JsonProperty("last_page_url")] //"last_page_url": "http://localhost:8000/api/stock-transactions?page=3",
        public string LastPageURL
        {
            get => lastPageURL;
            set => lastPageURL = value;
        }

        [JsonProperty("prev_page_url")] //"prev_page_url": "http://localhost:8000/api/stock-transactions?page=2",
        public string PreviousPageURL
        {
            get => previousPageURL;
            set => previousPageURL = value;
        }

        [JsonProperty("next_page_url")] //"next_page_url": null,
        public string NextPageURL
        {
            get => nextPageURL;
            set => nextPageURL = value;
        }

        [JsonProperty("data")] //"data": [],
        public List<T> Data
        {
            set => data = value;
            get
            {
                return data;
            }
        }

        public async Task<List<T>> GetDataOrInitializeAsync()
        {
            if (!IsInitialized)
            {
                await Initialize();
            }

            return Data;
        }

        internal Page(string baseUrl, int currentPage)
        {
            NonPaginatedURL = baseUrl;
            CurrentPage = currentPage;
        }

        [JsonConstructor]
        public Page() { }

        private async Task Initialize()
        {
            if (!IsInitialized)
            {
                string url = NonPaginatedURL.Contains("?") ? $"{NonPaginatedURL}&page={CurrentPage}" : $"{NonPaginatedURL}?page={CurrentPage}";

                RestRequest request = new RestRequest(url, Method.Get);

                Page<T> page = await DataConnection.RequestAsync<Page<T>>(request);

                totalItems = page.totalItems;
                itemsPerPage = page.itemsPerPage;
                itemFrom = page.itemFrom;
                itemTo = page.itemTo;
                firstPageURL = page.firstPageURL;
                totalPageCount = page.totalPageCount;
                lastPageURL = page.lastPageURL;
                previousPageURL = page.previousPageURL;
                nextPageURL = page.nextPageURL;
                data = page.Data;

                IsInitialized = true;
            }
        }
    }
}