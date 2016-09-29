using System.Threading.Tasks;
using EF2OR.ViewModels;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using EF2OR.Entities.EdFiOdsApi;

namespace EF2OR.Providers
{
    public interface IApiResponseProvider
    {
        Task<IEdFiOdsData> GetApiData<T>(string apiEndpoint, bool forceNew = false, string fields = null, Dictionary<string,string> filters = null) where T : class, IEdFiOdsData, new();
        Task<JArray> GetPagedApiData(string apiEndpoint, int offset, string fields = null, Dictionary<string,string> filters = null);
        Task<JArray> GetCustomApiData(string customUrl);
        string GetApiPrefix();
    }
}