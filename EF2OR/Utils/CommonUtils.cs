using EF2OR.Entities;
using EF2OR.Entities.EdFiOdsApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Utils
{
    public class CommonUtils
    {
        //public static IPathProvider PathProvider { get; set; }
        //public static IHttpContextProvider HttpContextProvider { get; set; }
        internal static Providers.IApiResponseProvider ApiResponseProvider { get; set; }
        internal static Dictionary<string, IEdFiOdsData> ExistingResponses { get; set; }
        internal static Providers.IPathProvider PathProvider { get; set; }
        internal static Providers.IHttpContextProvider HttpContextProvider { get; set; }
        internal static Providers.IUserProvider UserProvider { get; set; }
    }
}
