using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Tests.Utils
{
    public class FakesHelper
    {
        internal static void SetupFakes()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
            System.Web.Fakes.ShimHttpContext.CurrentGet = () =>
            {
                return AuthenticationHelper.HttpContext;
            };
            EF2OR.Utils.Fakes.ShimApiCalls.UserIdGet = () =>
            {
                return AuthenticationHelper.TestUser.Id;
            };
            System.Web.Fakes.ShimHttpServerUtility.AllInstances.MapPathString = (server, path) =>
            {
                return System.IO.Path.Combine(Environment.CurrentDirectory, "FakeMappedPAth", path);
            };
            EF2OR.Controllers.Fakes.ShimBaseController.AllInstances.UserNameGet = (controller) =>
            {
                return AuthenticationHelper.TESTUSER_USERNAME;
            };
            EF2OR.Controllers.Fakes.ShimBaseController.AllInstances.IpAddressGet = (controller) =>
            {
                return "-1";
            };
            SetupFakeApiResponse();
        }

        internal static void SetupFakeApiResponse()
        {
            //EF2OR.Utils.Fakes.ShimApiCalls.GetApiResponseStringString = (apiEndpoint, fields) =>
            //{
            //    return null;
            //};
            EF2OR.Utils.Fakes.ShimApiCalls.GetApiResponseArrayStringBooleanString =
                async (apiEndpoint, forceNew, fields) =>
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
                };
        }
    }
}
