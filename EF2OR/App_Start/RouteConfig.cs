﻿using System.Web.Mvc;
using System.Web.Routing;

namespace EF2OR
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            "ExportRoute",
            "Export/{id}",
            new { controller = "Export", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Templates", action = "Index", id = UrlParameter.Optional });
        }
    }
}
