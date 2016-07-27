using EF2OR.App_Start.CustomAttributes;
using System.Web.Mvc;

namespace EF2OR
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}
