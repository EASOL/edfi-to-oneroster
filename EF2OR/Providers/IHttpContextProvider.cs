using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EF2OR.Providers
{
    internal interface IHttpContextProvider
    {
        HttpContext Current { get;}
    }
}
