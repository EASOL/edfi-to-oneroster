using System.Web;

namespace EF2OR.Common.Providers
{
    public interface IHttpContextProvider
    {
        HttpContext Current { get;}
    }
}