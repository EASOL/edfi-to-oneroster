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
using System.IO.Compression;

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
                List<string> lstInvalidFileNames = new List<string>();
                using (System.IO.MemoryStream memStream = new System.IO.MemoryStream(fileResult.FileContents))
                {
                    System.IO.Compression.ZipArchive zipArchive = new ZipArchive(memStream);
                    string[] validFileNames =
                        { "orgs.csv", "users.csv", "courses.csv", "classes.csv", "enrollments.csv", "academicSessions.csv" };
                    foreach (var singleEntry in zipArchive.Entries)
                    {
                        if (!validFileNames.Contains(singleEntry.Name))
                        {
                            lstInvalidFileNames.Add(singleEntry.Name);
                        }
                        //For Validation rules check https://docs.google.com/spreadsheets/d/1X-gX06CaeDwDn5-Q-pqVAVahgJRtQqFOx5jj3FtIHKs/edit#gid=0
                        ValidateFileContents(singleEntry);
                    }
                    memStream.Close();
                }
                if (lstInvalidFileNames.Count > 0)
                {
                    Assert.Fail("Invalid File Names: " +
                        String.Join(",", lstInvalidFileNames.ToArray()));
                }
                //var zipFile = System.IO.Compression.ZipFile
            }
        }

        private static void ValidateFileContents(ZipArchiveEntry singleEntry)
        {
            List<string> lstOrgFileMissingHeaders = new List<string>();
            List<string> lstOrgFileInvalidInfo = new List<string>();
            using (var singleFileStream = singleEntry.Open())
            {
                using (System.IO.StreamReader strReader = new System.IO.StreamReader(singleFileStream))
                {
                    using (CsvHelper.CsvReader csvreader = new CsvHelper.CsvReader(strReader))
                    {
                        switch (singleEntry.Name)
                        {
                            case "orgs.csv":
                                if (csvreader.ReadHeader())
                                {
                                    if (!csvreader.FieldHeaders.Contains("sourcedId"))
                                    {
                                        lstOrgFileMissingHeaders.Add("sourcedId");
                                    }
                                    if (!csvreader.FieldHeaders.Contains("status"))
                                    {
                                        lstOrgFileMissingHeaders.Add("status");
                                    }
                                    if (!csvreader.FieldHeaders.Contains("dateLastModified"))
                                    {
                                        lstOrgFileMissingHeaders.Add("dateLastModified");
                                    }
                                    if (!csvreader.FieldHeaders.Contains("name"))
                                    {
                                        lstOrgFileMissingHeaders.Add("name");
                                    }
                                }
                                int iRow = 1;
                                while (csvreader.Read())
                                {
                                    string sourceId = string.Empty;
                                    bool hasSourceId = csvreader.TryGetField<string>("sourcedId", out sourceId);
                                    if (!hasSourceId)
                                    {
                                        lstOrgFileInvalidInfo.Add("Unable to get a valid sourceId for Row " + iRow);
                                    }
                                    string status = string.Empty;
                                    bool hasStatus = csvreader.TryGetField<string>("status", out status);
                                    if (!hasStatus)
                                    {
                                        lstOrgFileInvalidInfo.Add("Unable to get a valid status for Row " + iRow);
                                    }
                                    else
                                    {
                                        string[] validStatuses = { "active", "inactive", "tobedeleted" };
                                        if (!validStatuses.Contains(status))
                                        {
                                            lstOrgFileInvalidInfo.Add("Invalid status for Row " + iRow + ": " + status);
                                        }
                                    }
                                    string dateLastModified = string.Empty;
                                    bool hasDateLastModified = csvreader.TryGetField<string>("dateLastModified", out dateLastModified);
                                    if (String.IsNullOrWhiteSpace(dateLastModified))
                                    {
                                        lstOrgFileInvalidInfo.Add("Empty dateLastModified for Row: " + iRow + ". Format must be YYYY-MM-DD");
                                    }
                                    else
                                    {
                                        if (hasDateLastModified && dateLastModified.Length == 8)
                                        {
                                            DateTime resultDate;
                                            bool validDate = DateTime.TryParseExact(dateLastModified, "YYYY-MM-DD", null, System.Globalization.DateTimeStyles.None,
                                                out resultDate);
                                            if (!validDate)
                                            {
                                                lstOrgFileInvalidInfo.Add("Invalid dateLastModified for Row: " + iRow + ". Format must be YYYY-MM-DD");
                                            }
                                        }
                                        else
                                        {
                                            lstOrgFileInvalidInfo.Add("Invalid format for date: " + dateLastModified);
                                        }
                                    }
                                    string name = string.Empty;
                                    bool hasName = csvreader.TryGetField<string>("name", out name);
                                    if (!hasName)
                                    {
                                        lstOrgFileInvalidInfo.Add("No name for Row: " + iRow);
                                    }
                                    string type = string.Empty;
                                    bool hasType = csvreader.TryGetField<string>("type", out type);
                                    if (!hasType)
                                    {
                                        lstOrgFileInvalidInfo.Add("No type for Row: " + iRow);
                                    }
                                    else
                                    {
                                        string[] validTypes = { "school", "local", "state", "national" };
                                        if (!validTypes.Contains(type))
                                        {
                                            lstOrgFileInvalidInfo.Add("Invalid type for Row: " + iRow);
                                        }
                                    }
                                    string identifier = string.Empty;
                                    bool hasIdentifier = csvreader.TryGetField<string>("identifier", out identifier);
                                    if (!hasIdentifier)
                                    {
                                        lstOrgFileInvalidInfo.Add("Invalid identifier for Row: " + iRow);
                                    }
                                    string metadata_classification = string.Empty;
                                    bool hasmetadata_classification = csvreader.TryGetField<string>("metadata.classification", out metadata_classification);
                                    if (!hasmetadata_classification)
                                    {
                                        lstOrgFileInvalidInfo.Add("Invalid metadata.classification for Row: " + iRow);
                                    }
                                    else
                                    {
                                        string[] validMetadataClassification = { "charter", "private", "public" };
                                    }
                                }
                                break;
                        }
                    }
                    strReader.Close();
                }
                singleFileStream.Close();
            }
            if (lstOrgFileMissingHeaders.Count > 0 || lstOrgFileInvalidInfo.Count > 0)
            {
                StringBuilder strErrors = new StringBuilder();
                strErrors.AppendLine("Missing headers for orgs.csv: ");
                if (lstOrgFileInvalidInfo.Count > 0)
                    strErrors.AppendLine(string.Join(",", lstOrgFileInvalidInfo.ToArray()));
                if (lstOrgFileMissingHeaders.Count > 0)
                    strErrors.AppendLine(string.Join(",", lstOrgFileMissingHeaders.ToArray()));
                string errorsText = strErrors.ToString();
                Assert.Fail(errorsText);
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