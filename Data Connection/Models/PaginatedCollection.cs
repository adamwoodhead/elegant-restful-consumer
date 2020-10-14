using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataConnection.Models
{
    public class PaginatedCollection<T>
    {
        public int PageCount { get; private set; } = 0;

        private string BaseURL { get; set; }

        public List<Page<T>> Pages { get; private set; }

        private PaginatedCollection(string url) 
        {
            BaseURL = url;
        }

        // TODO Setup Cancellation Token
        public static async Task<PaginatedCollection<T>> InstantiateIndex(CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetPaginatedIndexRoute();

            PaginatedCollection<T> book = new PaginatedCollection<T>(url);

            Page<T> firstPage = new Page<T>(book.BaseURL, 1);

            await firstPage.GetDataOrInitializeAsync();

            book.PageCount = (int)firstPage.TotalPageCount;

            book.Pages = new List<Page<T>>(book.PageCount) { firstPage };

            if (book.PageCount > 1)
            {
                for (int i = 2; i <= book.PageCount; i++)
                {
                    book.Pages.Add(new Page<T>(book.BaseURL, i));
                }
            }

            return book;
        }

        // TODO Setup Cancellation Token
        public static async Task<PaginatedCollection<T>> InstantiateExtension(int? id, string extension, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetPaginatedRelationshipRoute(id, extension);

            PaginatedCollection<T> book = new PaginatedCollection<T>(url);

            Page<T> firstPage = new Page<T>(book.BaseURL, 1);

            await firstPage.GetDataOrInitializeAsync();

            book.PageCount = (int)firstPage.TotalPageCount;

            book.Pages = new List<Page<T>>(book.PageCount) { firstPage };

            if (book.PageCount > 1)
            {
                for (int i = 2; i <= book.PageCount; i++)
                {
                    book.Pages.Add(new Page<T>(book.BaseURL, i));
                }
            }

            return book;
        }

        // TODO Setup Cancellation Token
        public static async Task<PaginatedCollection<T>> InstantiateSearch(string haystack, string needle, CancellationToken cancellationToken = default)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.GetPaginatedSearchRoute(haystack, needle);

            PaginatedCollection<T> book = new PaginatedCollection<T>(url);

            Page<T> firstPage = new Page<T>(book.BaseURL, 1);

            await firstPage.GetDataOrInitializeAsync();

            book.PageCount = (int)firstPage.TotalPageCount;

            book.Pages = new List<Page<T>>(book.PageCount) { firstPage };

            if (book.PageCount > 1)
            {
                for (int i = 2; i <= book.PageCount; i++)
                {
                    book.Pages.Add(new Page<T>(book.BaseURL, i));
                }
            }

            return book;
        }
    }
}