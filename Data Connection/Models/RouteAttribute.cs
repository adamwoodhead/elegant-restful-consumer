using System;
using System.Collections.Generic;
using System.Text;

namespace DataConnection.Models
{
    public class RouteAttribute : Attribute
    {
        private string IndexRoute { get; set; }

        public string GetIndexRoute()
        {
            return $"{IndexRoute}/";
        }

        public string GetPaginatedIndexRoute()
        {
            return $"{IndexRoute}/?pagination=true";
        }

        public string GetSingularRoute(int? id)
        {
            return $"{IndexRoute}/{id}/";
        }

        public string GetRelationshipRoute(int? id, string relative)
        {
            if (id != null)
            {
                return $"{IndexRoute}/{id}/{relative}/";
            }
            else
            {
                return $"{IndexRoute}/{relative}/";
            }
        }

        public string GetPaginatedRelationshipRoute(int? id, string relative)
        {
            if (id != null)
            {
                return $"{IndexRoute}/{id}/{relative}/?pagination=true";
            }
            else
            {
                return $"{IndexRoute}/{relative}/?pagination=true";
            }
        }

        public string GetSearchRoute(string haystackField, string needleValue)
        {
            return $"{IndexRoute}/?search={needleValue}&in={haystackField}";
        }

        public string GetPaginatedSearchRoute(string haystackField, string needleValue)
        {
            return $"{IndexRoute}/?search={needleValue}&in={haystackField}&pagination=true";
        }

        public RouteAttribute(string baseRoute)
        {
            IndexRoute = baseRoute;
        }
    }
}
