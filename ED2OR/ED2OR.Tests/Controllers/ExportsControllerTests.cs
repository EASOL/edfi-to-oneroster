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

        [TestMethod]
        public async Task ExportsController_PreviewTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
                System.Web.Fakes.ShimHttpContext.CurrentGet = () =>
                {
                    return AuthenticationHelper.HttpContext;
                };
                ExportsController exportsController = new ExportsController();
                SetupController(exportsController);
                ED2OR.Utils.Fakes.ShimApiCalls.UserIdGet = () =>
                {
                    return AuthenticationHelper.TestUser.Id;
                };
                System.Web.Fakes.ShimHttpServerUtility.AllInstances.MapPathString = (server, path) =>
                {
                    return System.IO.Path.Combine(Environment.CurrentDirectory, "FakeMappedPAth", path);
                };
                ExportsViewModel defaultExportsViewModel = await GetDefaultExportsViewModel();
                var previewResult = await exportsController.Preview(
                    schoolIds: defaultExportsViewModel.SelectedSchools.Split(',').ToList(),
                    schoolYears: null,
                    terms: null,
                    subjects: null,
                    courses: defaultExportsViewModel.SelectedCourses.Split(',').ToList(),
                    teachers: null,
                    sections: null
                    );
                Assert.IsNotNull(previewResult, "Invalid result");
                Assert.IsInstanceOfType(previewResult, typeof(PartialViewResult), "Invalid result type");
                PartialViewResult partialViewResult = previewResult as PartialViewResult;
                Assert.IsNotNull(partialViewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(partialViewResult.Model, typeof(ExportsViewModel), "Unexpected type for model");
                ExportsViewModel partialResultModel = partialViewResult.Model as ExportsViewModel;
                Assert.IsTrue(partialResultModel.DataPreviewSections.Count > 0, "No Data Preview Sections");
                Assert.IsTrue(partialResultModel.JsonPreviews.Orgs.Count() > 0, "No Orgs for JsonPreview");
            }
        }


        [TestMethod()]
        public async Task ExportsController_DownloadCsvTest()
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
                var defaultResult = await controller.DownloadCsv(defaultExportViewModel);
                Assert.IsNotNull(defaultResult, "Invalid Result");
                Assert.IsInstanceOfType(defaultResult, typeof(FileContentResult), "Is not a FileResult");
                FileContentResult fileResult = defaultResult as FileContentResult;
                Assert.IsTrue(!string.IsNullOrWhiteSpace(fileResult.FileDownloadName), "Invalid File Name");
                Assert.IsTrue(fileResult.FileContents != null && fileResult.FileContents.Count() > 0, "Invalid File Contents");
            }
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
                var defaultResult = await controller.Index();
                Assert.IsNotNull(defaultResult, "invalid result");
                Assert.IsInstanceOfType(defaultResult, typeof(ViewResult), "Unexpected result type");
                ViewResult defaultViewResult = defaultResult as ViewResult;
                Assert.IsNotNull(defaultViewResult.Model, "Invalid model result");
                Assert.IsInstanceOfType(defaultViewResult.Model, typeof(ExportsViewModel), "Unexpected model type");
                ExportsViewModel modelResult = defaultViewResult.Model as ExportsViewModel;
                Assert.IsTrue(modelResult.SchoolsCriteriaSection.FilterCheckboxes != null &&
                    modelResult.SchoolsCriteriaSection.FilterCheckboxes.Count > 0, "No data for Schools Criteria");
                Assert.IsTrue(modelResult.SchoolYearsCriteriaSection.FilterCheckboxes != null &&
                    modelResult.SchoolYearsCriteriaSection.FilterCheckboxes.Count > 0, "No data for Schools Criteria");
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

        [TestMethod()]
        public async Task ExportsController_GetTeachersPartialTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ED2OR.Tests.Utils.FakesHelper.SetupFakes();
                ExportsController controller = new ExportsController();
                SetupController(controller);
                ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
                var defaultResult = await controller.Index();
                Assert.IsNotNull(defaultResult, "invalid result");
                Assert.IsInstanceOfType(defaultResult, typeof(ViewResult), "Unexpected result type");
                ViewResult defaultViewResult = defaultResult as ViewResult;
                Assert.IsNotNull(defaultViewResult.Model, "Invalid model result");
                Assert.IsInstanceOfType(defaultViewResult.Model, typeof(ExportsViewModel), "Unexpected model type");
                ExportsViewModel modelResult = defaultViewResult.Model as ExportsViewModel;
                var schoolIds = modelResult.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).ToList();
                var schoolYears = modelResult.SchoolYearsCriteriaSection.FilterCheckboxes.Select(p => p.Id).ToList();
                var partialResult = await controller.GetTeachersPartial(schoolIds: schoolIds, schoolYears: schoolYears, terms: null,
                    boxesAlreadyChecked: null);
                Assert.IsNotNull(partialResult, "Invalid result");
                Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Invalid result type");
                PartialViewResult partialViewResult = partialResult as PartialViewResult;
                Assert.IsNotNull(partialViewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
                ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
                Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
            }
        }

        [TestMethod()]
        public async Task ExportsController_GetSectionsPartialTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ED2OR.Tests.Utils.FakesHelper.SetupFakes();
                ExportsController controller = new ExportsController();
                SetupController(controller);
                ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
                var defaultResult = await controller.Index();
                Assert.IsNotNull(defaultResult, "invalid result");
                Assert.IsInstanceOfType(defaultResult, typeof(ViewResult), "Unexpected result type");
                ViewResult defaultViewResult = defaultResult as ViewResult;
                Assert.IsNotNull(defaultViewResult.Model, "Invalid model result");
                Assert.IsInstanceOfType(defaultViewResult.Model, typeof(ExportsViewModel), "Unexpected model type");
                ExportsViewModel modelResult = defaultViewResult.Model as ExportsViewModel;
                var schoolIds = modelResult.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).ToList();
                var schoolYears = modelResult.SchoolYearsCriteriaSection.FilterCheckboxes.Select(p => p.Id).ToList();
                var partialResult = await controller.GetSectionsPartial(schoolIds: schoolIds,
                    schoolYears: schoolYears, terms: null, boxesAlreadyChecked: null);
                Assert.IsNotNull(partialResult, "Invalid result");
                Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Unexpected result type");
                PartialViewResult partialViewResult = partialResult as PartialViewResult;
                Assert.IsNotNull(partialViewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
                ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
                Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
            }
        }

        [TestMethod()]
        public async Task ExportsController_GetCoursesPartialTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ED2OR.Tests.Utils.FakesHelper.SetupFakes();
                ExportsController controller = new ExportsController();
                SetupController(controller);
                ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
                var defaultResult = await controller.Index();
                Assert.IsNotNull(defaultResult, "invalid result");
                Assert.IsInstanceOfType(defaultResult, typeof(ViewResult), "Unexpected result type");
                ViewResult defaultViewResult = defaultResult as ViewResult;
                Assert.IsNotNull(defaultViewResult.Model, "Invalid model result");
                Assert.IsInstanceOfType(defaultViewResult.Model, typeof(ExportsViewModel), "Unexpected model type");
                ExportsViewModel modelResult = defaultViewResult.Model as ExportsViewModel;
                var schoolIds = modelResult.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).ToList();
                var schoolYears = modelResult.SchoolYearsCriteriaSection.FilterCheckboxes.Select(p => p.Id).ToList();
                var partialResult = await controller.GetCoursesPartial(schoolIds: schoolIds,
                    schoolYears: schoolYears, terms: null, boxesAlreadyChecked: null);
                Assert.IsNotNull(partialResult, "Invalid result");
                Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Unexpected result type");
                PartialViewResult partialViewResult = partialResult as PartialViewResult;
                Assert.IsNotNull(partialViewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
                ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
                Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
            }
        }

        [TestMethod()]
        public async Task ExportsController_GetSubjectsPartialTest()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                ED2OR.Tests.Utils.FakesHelper.SetupFakes();
                ExportsController controller = new ExportsController();
                SetupController(controller);
                ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
                var defaultResult = await controller.Index();
                Assert.IsNotNull(defaultResult, "invalid result");
                Assert.IsInstanceOfType(defaultResult, typeof(ViewResult), "Unexpected result type");
                ViewResult defaultViewResult = defaultResult as ViewResult;
                Assert.IsNotNull(defaultViewResult.Model, "Invalid model result");
                Assert.IsInstanceOfType(defaultViewResult.Model, typeof(ExportsViewModel), "Unexpected model type");
                ExportsViewModel modelResult = defaultViewResult.Model as ExportsViewModel;
                var schoolIds = modelResult.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).ToList();
                var schoolYears = modelResult.SchoolYearsCriteriaSection.FilterCheckboxes.Select(p => p.Id).ToList();
                var partialResult = await controller.GetSubjectsPartial(schoolIds: schoolIds,
                    schoolYears: schoolYears, terms: null, boxesAlreadyChecked: null);
                Assert.IsNotNull(partialResult, "Invalid result");
                Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Unexpected result type");
                PartialViewResult partialViewResult = partialResult as PartialViewResult;
                Assert.IsNotNull(partialViewResult.Model, "Invalid model");
                Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
                ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
                Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
            }
        }
    }
}