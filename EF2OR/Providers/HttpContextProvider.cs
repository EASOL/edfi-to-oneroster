using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Providers
{
    internal class HttpContextProvider : IHttpContextProvider
    {
        HttpContext IHttpContextProvider.Current
        {
            get
            {
                return HttpContext.Current;
            }
        }
    }
}
