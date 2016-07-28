using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EF2OR.Providers;
using EF2OR.ViewModels;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF2OR.Tests.TestProviders
{
    public class TestApiResponseProvider : EF2OR.Providers.IApiResponseProvider
    {
        async Task<JArray> IApiResponseProvider.GetApiResponseArray(string apiEndpoint, bool forceNew, string fields)
        {
            JArray result = null;
            JArray apiResponse = null;
            switch (apiEndpoint)
            {
                case "enrollment/schools":
                    apiResponse =
                        JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Schools);
                    result = await Task.FromResult(apiResponse);
                    break;
                case "enrollment/staffs":
                    apiResponse =
                        JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Staffs);
                    result = await Task.FromResult(apiResponse);
                    break;
                case "enrollment/students":
                    apiResponse =
                        JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Students);
                    result = await Task.FromResult(apiResponse);
                    break;
                case "enrollment/sections":
                    apiResponse =
                        JArray.Parse(EF2OR.Tests.Properties.Resources.Enrollment_Sections);
                    result = await Task.FromResult(apiResponse);
                    break;
                default:
                    Assert.Fail(string.Format("Unexpected Endpoint: {0}", apiEndpoint));
                    break;
            }
            return result;
        }
    }
}
