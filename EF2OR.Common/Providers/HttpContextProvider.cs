using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Common.Providers
{
    public class HttpContextProvider : IHttpContextProvider
    {
        public System.Web.HttpContext Current
        {
            get
            {
                return System.Web.HttpContext.Current;
            }
        }
    }
}
