using System.Threading.Tasks;
using EF2OR.ViewModels;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EF2OR.Providers
{
    public interface IApiResponseProvider
    {

        Task<JArray> GetApiResponseArray(string apiEndpoint, bool forceNew = false, string fields = null);
    }
}