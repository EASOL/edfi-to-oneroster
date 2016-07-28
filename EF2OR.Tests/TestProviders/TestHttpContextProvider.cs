using EF2OR.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Tests.TestProviders
{
    internal class TestHttpContextProvider : IHttpContextProvider
    {
        HttpContext IHttpContextProvider.Current
        {
            get
            {
                return EF2OR.Tests.Utils.AuthenticationHelper.HttpContext;
            }
        }
    }
}
