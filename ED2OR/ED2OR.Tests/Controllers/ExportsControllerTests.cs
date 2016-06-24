using Microsoft.VisualStudio.TestTools.UnitTesting;
using ED2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using ED2OR.Tests.Utils;
using Moq;
using System.Web.Routing;

namespace ED2OR.Controllers.Tests
{
    [TestClass()]
    public class ExportsControllerTests
    {
        protected Mock<HttpContextBase> HttpContextBaseMock;
        protected Mock<HttpRequestBase> HttpRequestMock;
        protected Mock<HttpResponseBase> HttpResponseMock;

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
        }

        protected void SetupController(Controller controller)
        {
            var routes = new RouteCollection();
            controller.ControllerContext = new ControllerContext(HttpContextBaseMock.Object, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(HttpContextBaseMock.Object, new RouteData()), routes);
        }

        [TestMethod()]
        public async Task ExportsController_IndexTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
                System.Web.Fakes.ShimHttpContext.CurrentGet = () =>
                  {
                      return AuthenticationHelper.HttpContext;
                  };
                ExportsController controller = new ExportsController();
                SetupController(controller);
                ED2OR.Utils.Fakes.ShimApiCalls.UserIdGet = () =>
                {
                    return AuthenticationHelper.TestUser.Id;
                };
                var result = await controller.Index();
                Assert.IsNotNull(result, "Invalid result");
                Assert.IsInstanceOfType(result, typeof(ViewResult), "Unexpected result type");
                ViewResult viewResult = result as ViewResult;
                Assert.IsNotNull(viewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(viewResult.Model, typeof(ED2OR.ViewModels.ExportsViewModel), "Unexpected ViewModel type");
                ED2OR.ViewModels.ExportsViewModel viewModel = viewResult.Model as ED2OR.ViewModels.ExportsViewModel;
                Assert.IsNotNull(viewModel.SchoolsCriteriaSection, "Invalid CoursesCriteriaSection");
                Assert.IsNotNull(viewModel.SchoolYearsCriteriaSection, "Invalid SchoolYearsCriteriaSection");
                Assert.IsNotNull(viewModel.TermsCriteriaSection, "Invalid TermsCriteriaSection");
                Assert.IsNotNull(viewModel.SubjectsCriteriaSection, "Invalid SubjectsCriteriaSection");
                Assert.IsNotNull(viewModel.CoursesCriteriaSection, "Invalid CoursesCriteriaSection");
                Assert.IsNotNull(viewModel.TeachersCriteriaSection, "Invalid TeachersCriteriaSection");
                Assert.IsNotNull(viewModel.SectionsCriteriaSection, "Invalid SectionsCriteriaSection");
            }
        }
    }
}