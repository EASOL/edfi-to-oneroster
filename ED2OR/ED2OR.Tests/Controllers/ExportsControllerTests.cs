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
using ED2OR.ViewModels;
using ED2OR.Utils;

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
                System.Web.Fakes.ShimHttpServerUtility.AllInstances.MapPathString = (server, path) =>
                {
                    return System.IO.Path.Combine(Environment.CurrentDirectory, "FakeMappedPAth", path);
                };

                ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
                var downloadResult = await controller.Index(defaultExportViewModel, "Download");
                Assert.IsNotNull(downloadResult, "Invalid Download reuslt");
                Assert.IsInstanceOfType(downloadResult, typeof(FileContentResult), "Invalid File Result");
                FileContentResult fileResult = downloadResult as FileContentResult;
                Assert.IsTrue(fileResult.FileContents != null && fileResult.FileContents.Length > 0, "Invalid File Contents");
            }
        }

        private async Task<ExportsViewModel> GetDefaultExportsViewModel()
        {
            ExportsViewModel result = new ExportsViewModel();
            var courses = await ApiCalls.GetCourses();
            result.SelectedCourses = string.Join(",", courses);
            var schools = await ApiCalls.GetSchools();
            result.SelectedSchools = string.Join(",", schools.Select(p => p.SchoolId));
            return result;
        }

        private FilterInputs GenerateTestFilterInputs(ExportsViewModel viewModel)
        {
            var inputs = new FilterInputs
            {
                Schools = viewModel.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).Distinct().ToList(),
                SchoolYears = viewModel.SchoolYearsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolYear).Distinct().ToList(),
                Terms = viewModel.TermsCriteriaSection.FilterCheckboxes.Select(p => p.Term).Distinct().ToList(),
                Subjects = viewModel.SubjectsCriteriaSection.FilterCheckboxes.Select(p => p.Subject).Distinct().ToList(),
                Courses = viewModel.CoursesCriteriaSection.FilterCheckboxes.Select(p => p.Course).Distinct().ToList(),
                Teachers = viewModel.TeachersCriteriaSection.FilterCheckboxes.Select(p => p.Teacher).Distinct().ToList()
            };
            return inputs;
        }

        private void CompareColumns(IEnumerable<string> sourceTypeColumnNames, List<DataPreviewSection> dataPreviewSections, string sectionName)
        {

            var dataPreviewColumnNames = dataPreviewSections.Where(p => p.SectionName == sectionName).FirstOrDefault().ColumnNames;
            foreach (var singleSourceColumn in sourceTypeColumnNames)
            {
                Assert.IsTrue(dataPreviewColumnNames.Contains(singleSourceColumn),
                    string.Format("Column {0} does not exist in Data Preview for {1}", singleSourceColumn, sectionName));
            }
        }
    }
}