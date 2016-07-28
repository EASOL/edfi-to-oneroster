using Microsoft.VisualStudio.TestTools.UnitTesting;
using EF2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EF2OR.Tests.Utils;
using System.Web.Mvc;
using EF2OR.ViewModels;
using System.Web;
using Moq;

namespace EF2OR.Controllers.Tests
{
    [TestClass()]
    public class SettingsControllerTests
    {
        public Mock<HttpContextBase> HttpContextBaseMock { get; private set; }
        public Mock<HttpRequestBase> HttpRequestMock { get; private set; }
        public Mock<HttpResponseBase> HttpResponseMock { get; private set; }

        [ClassInitialize()]
        public static void InitializeClass(TestContext context)
        {
            AuthenticationHelper.Initialize();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            HttpContextBaseMock = new Mock<HttpContextBase>();
            HttpRequestMock = new Mock<HttpRequestBase>();
            HttpResponseMock = new Mock<HttpResponseBase>();
            HttpContextBaseMock.SetupGet(x => x.Request).Returns(HttpRequestMock.Object);
            HttpContextBaseMock.SetupGet(x => x.Response).Returns(HttpResponseMock.Object);
            SessionHelper.Initialize();
            EF2OR.Tests.Utils.ProvidersHelper.InitializeProviders();
        }

        [TestMethod()]
        public async Task SettingsController_IndexTest()
        {
                SettingsController controller = new SettingsController();
                var result = await controller.Index() as ViewResult;
                Assert.IsNotNull(result, "Invalid Result");
                Assert.IsNotNull(result.Model, "Null Model");
                EF2OR.ViewModels.SettingsViewModel settingsVM = new ViewModels.SettingsViewModel();
                settingsVM.ApiBaseUrl = AuthenticationHelper.TESTAPI_BASEURL;
                settingsVM.ApiKey = AuthenticationHelper.TESTAPI_APIKEY;
                settingsVM.ApiSecret = AuthenticationHelper.TESTAPI_APISECRET;
                settingsVM.ApiPrefix = AuthenticationHelper.TESTAPI_APIPREFIX;
                settingsVM.DatabaseSettings = new InitialSetup();
                var postResult = await controller.Index(settingsVM);
                Assert.IsTrue(controller.ModelState.IsValid, "Invalid model");
                Assert.IsNotNull(postResult, "Invalid Result");
        }

        [TestMethod()]
        public void SettingsController_TestConnectionTest()
        {
            SettingsController settingsController = new SettingsController();
            JsonResult result =
                settingsController.TestConnection(AuthenticationHelper.TESTAPI_BASEURL, AuthenticationHelper.TESTAPI_APIKEY,
                AuthenticationHelper.TESTAPI_APISECRET) as JsonResult;
            Assert.IsNotNull(result, "Invalid JsonResult");
            TokenViewModel tokenViewModel = result.Data as TokenViewModel;
            Assert.IsNotNull(tokenViewModel, "Unable to get TokenViewModel");
            Assert.IsTrue(tokenViewModel.IsSuccessful, string.Format("Error Testing Connection: {0}", tokenViewModel.ErrorMessage));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(tokenViewModel.Token), "Invalid Token");

        }
    }
}