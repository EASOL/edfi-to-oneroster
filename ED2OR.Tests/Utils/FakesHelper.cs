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
        }
    }
}
