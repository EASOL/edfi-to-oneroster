using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Tests.Utils
{
    class ProvidersHelper
    {
        internal static void InitializeProviders()
        {
            EF2OR.Utils.CommonUtils.ApiResponseProvider = new EF2OR.Tests.TestProviders.TestApiResponseProvider();
            EF2OR.Utils.CommonUtils.PathProvider = new EF2OR.Tests.TestProviders.TestPathProvider();
            EF2OR.Utils.CommonUtils.HttpContextProvider = new EF2OR.Tests.TestProviders.TestHttpContextProvider();
            EF2OR.Utils.CommonUtils.UserProvider = new EF2OR.Tests.TestProviders.TestUserProvider();
        }
    }
}
