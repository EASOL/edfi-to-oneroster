﻿using ED2OR.App_Start.CustomAttributes;
using System.Web;
using System.Web.Mvc;

namespace ED2OR
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}
