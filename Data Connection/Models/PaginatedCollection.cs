using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Create a book of paginated data - Omit ?page=x from the url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<PaginatedCollection<T>> Instantiate(string extension, int? id, string searchTerm)
        {
            RouteAttribute routeAttribute = typeof(T).GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(RouteAttribute)) as RouteAttribute;

            string url = routeAttribute.IndexRoute;

            // Remove leading and trailing slashes
            url = url.TrimEnd('/');

            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Trim('/') ?? null;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim('/') ?? null;
            }

            if (!string.IsNullOrEmpty(extension) && !string.IsNullOrEmpty(searchTerm))
            {
                // Concatenate url & route extensions(s)
                url = $"{url}/by-{extension}/{id}/?search={searchTerm}";
            }
            else if (!string.IsNullOrEmpty(extension))
            {
                // Concatenate url & route extensions(s)
                url = $"{url}/by-{extension}/{id}";
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                // Concatenate url & route extensions(s)
                url = $"{url}/?search={searchTerm}";
            }

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