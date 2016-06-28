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
                var defaultExportViewModel = viewResult.Model as ED2OR.ViewModels.ExportsViewModel;
                var resultPreviewCmd = await controller.Index(defaultExportViewModel, "Preview");
                Assert.IsNotNull(resultPreviewCmd, "Invalid Preview Post reuslt");
                Assert.IsInstanceOfType(resultPreviewCmd, typeof(ViewResult), "Unexpected result type");
                ViewResult previewCmdViewResult = resultPreviewCmd as ViewResult;
                Assert.IsInstanceOfType(previewCmdViewResult.Model, typeof(ExportsViewModel), "Invalid model");
                var previewCmdModel = previewCmdViewResult.Model as ExportsViewModel;
                Assert.IsNotNull(previewCmdModel.DataPreviewSections, "Invalid Data Preview Sections");


                var jsonPreviews = await Utils.ApiCalls.GetJsonPreviews();
                Assert.AreEqual(previewCmdModel.JsonPreviews.AcademicSessions, jsonPreviews.AcademicSessions, "JSON Preview AcademicSessions do not match");
                Assert.AreEqual(previewCmdModel.JsonPreviews.Classes, jsonPreviews.Classes, "JSON Preview Classes do not match");
                Assert.AreEqual(previewCmdModel.JsonPreviews.Courses, jsonPreviews.Courses, "JSON Preview Courses do not match");
                Assert.AreEqual(previewCmdModel.JsonPreviews.Enrollments, jsonPreviews.Enrollments, "JSON Preview  Enrollments do not match");
                Assert.AreEqual(previewCmdModel.JsonPreviews.Orgs, jsonPreviews.Orgs, "JSON Preview Orgs do not match");
                Assert.AreEqual(previewCmdModel.JsonPreviews.Users, jsonPreviews.Users, "JSON Preview Users do not match");

                var orgColumnNames = typeof(CsvOrgs).GetProperties().Select(x => x.Name);
                var usersColumnNames = typeof(CsvUsers).GetProperties().Select(x => x.Name);
                var coursesColumnNames = typeof(CsvCourses).GetProperties().Select(x => x.Name);
                var classesColumnNames = typeof(CsvClasses).GetProperties().Select(x => x.Name);
                var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Select(x => x.Name);
                var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Select(x => x.Name);

                CompareColumns(orgColumnNames, previewCmdModel.DataPreviewSections, "orgs");
                CompareColumns(usersColumnNames, previewCmdModel.DataPreviewSections, "users");
                CompareColumns(coursesColumnNames, previewCmdModel.DataPreviewSections, "courses");
                CompareColumns(classesColumnNames, previewCmdModel.DataPreviewSections, "classes");
                CompareColumns(enrollmentsColumnNames, previewCmdModel.DataPreviewSections, "enrollments");
                CompareColumns(academicsessionsColumnNames, previewCmdModel.DataPreviewSections, "academicsessions");

                System.Web.Fakes.ShimHttpServerUtility.AllInstances.MapPathString = (server, path) =>
                {
                    return System.IO.Path.Combine(Environment.CurrentDirectory, "FakeMappedPAth", path);
                };
                var downloadResult = await controller.Index(defaultExportViewModel, "Download");
                Assert.IsNotNull(downloadResult, "Invalid Download reuslt");
                Assert.IsInstanceOfType(downloadResult, typeof(FileContentResult), "Invalid File Result");
                FileContentResult fileResult = downloadResult as FileContentResult;
                Assert.IsTrue(fileResult.FileContents != null && fileResult.FileContents.Length > 0, "Invalid File Contents");
            }
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