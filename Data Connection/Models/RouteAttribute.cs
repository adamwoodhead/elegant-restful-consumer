using System;
using System.Collections.Generic;
using System.Text;

namespace DataConnection.Models
{
    public class RouteAttribute : Attribute
    {
        public string IndexRoute { get; set; }

        public string GetSingularRoute(int? id)
        {
            return $"{IndexRoute}/{id}";
        }

        public string GetRelationshipRoute(int? id, string relative)
        {
            if (id != null)
            {
                return $"{IndexRoute}/{id}/{relative}";
            }
            else
            {
                return $"{IndexRoute}/{relative}";
            }
        }

        public RouteAttribute(string baseRoute)
        {
            IndexRoute = baseRoute;
        }
    }
}
