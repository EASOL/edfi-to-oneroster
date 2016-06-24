using Microsoft.VisualStudio.TestTools.UnitTesting;
using ED2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ED2OR.Tests.Utils;
using System.Web.Mvc;

namespace ED2OR.Controllers.Tests
{
    [TestClass()]
    public class SettingsControllerTests
    {
        [ClassInitialize()]
        public static void InitializeClass(TestContext context)
        {
            AuthenticationHelper.Initialize();
        }

        [TestMethod()]
        public async Task SettingsController_IndexTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                var userIdentity = await AuthenticationHelper.TestUser.GenerateUserIdentityAsync(AuthenticationHelper.AppUserManager);
                ED2OR.Controllers.Fakes.ShimBaseController.AllInstances.UserIdGet = (baseController) =>
                {
                    return AuthenticationHelper.TestUser.Id;
                };
                SettingsController controller = new SettingsController(AuthenticationHelper.AppUserManager);
                var result = controller.Index() as ViewResult;
                Assert.IsNotNull(result, "Invalid Result");
                Assert.IsNotNull(result.Model, "Null Model");
            }
        }
    }
}