using Microsoft.VisualStudio.TestTools.UnitTesting;
using EF2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using EF2OR.Tests.Utils;
using Moq;
using System.Web.Routing;
using EF2OR.ViewModels;
using EF2OR.Utils;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace EF2OR.Controllers.Tests
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
            EF2OR.Tests.Utils.ProvidersHelper.InitializeProviders();
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
            Assert.Inconclusive("Unit test has infinite looping");
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
            ExportsController exportsController = new ExportsController();
            SetupController(exportsController);
            ExportsViewModel defaultExportsViewModel = await GetDefaultExportsViewModel();
            var previewResult = await exportsController.Preview(
                schoolIds: defaultExportsViewModel.SelectedSchools.Split(',').ToList(),
                //schoolYears: null,
                //terms: null,
                //subjects: null,
                //courses: defaultExportsViewModel.SelectedCourses.Split(',').ToList(),
                teachers: null,
                sections: null,
                oneRosterVersion: null
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

        [TestMethod]
        public async Task ExportsController_SaveTemplate()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
            ExportsController controller = new ExportsController();
            SetupController(controller);

            //ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
            DateTime currentTime = DateTime.UtcNow;
            ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
            defaultExportViewModel.NewTemplateName = string.Format("[UNIT TEST TEMPLATE]-{0}", currentTime);
            defaultExportViewModel.NewTemplateVendorName = string.Format("[UNIT TEST VENDOR]-{0}", currentTime);
            var result = controller.SaveTemplate(defaultExportViewModel);
            Assert.IsNotNull(result, "Invalid result");
            if (result is RedirectToRouteResult)
            {
                RedirectToRouteResult redirecResult = result as RedirectToRouteResult;
                Assert.IsTrue(redirecResult.RouteValues["action"].ToString() == "Index" && redirecResult.RouteValues["controller"].ToString() == "Templates");
            }
            else
            {
                Assert.Fail("Unexpected result");
            }
            var templateToDelete = AuthenticationHelper.DB.Templates.Where(p => p.VendorName ==
            defaultExportViewModel.NewTemplateVendorName && p.TemplateName == defaultExportViewModel.NewTemplateName).FirstOrDefault();
            if (templateToDelete != null)
            {
                AuthenticationHelper.DB.Templates.Remove(templateToDelete);
                if (
                AuthenticationHelper.DB.SaveChanges() == 0)
                    Assert.Fail("Unable to delete template: " + defaultExportViewModel.NewTemplateName);
            }
        }

        [TestMethod()]
        public async Task ExportsController_DownloadCsvTest()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
            ExportsController controller = new ExportsController();
            SetupController(controller);

            //ExportsViewModel defaultExportViewModel = await GetDefaultExportsViewModel();
            ExportsViewModel defaultExportViewModel = new ExportsViewModel();
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
                    { "orgs.csv", "users.csv", "courses.csv", "classes.csv", "enrollments.csv", "academicSessions.csv", "demographics.csv" };
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

        private static void ValidateField(string fieldName, string[] validValues, List<string> lstInvalidInfo,
            int iRow, CsvHelper.CsvReader csvreader)
        {
            string fieldValue = string.Empty;
            bool hasValue = csvreader.TryGetField<string>(fieldName, out fieldValue);
            if (!hasValue)
            {
                lstInvalidInfo.Add(string.Format("Invalid {0} for Row: ", fieldName) + iRow);
            }
            else
            {
                if (validValues != null && !validValues.Contains(fieldValue))
                {
                    lstInvalidInfo.Add(string.Format("Invalid {0} for Row: ", fieldName) + iRow + " " + fieldValue);
                }
            }
        }

        /// <summary>
        /// For Validation rules check https://docs.google.com/spreadsheets/d/1X-gX06CaeDwDn5-Q-pqVAVahgJRtQqFOx5jj3FtIHKs/edit#gid=0
        /// </summary>
        /// <param name="singleEntry"></param>
        private static void ValidateFileContents(ZipArchiveEntry singleEntry)
        {
            List<string> lstOrgFileMissingHeaders = new List<string>();
            List<string> lstOrgFileInvalidInfo = new List<string>();


            List<string> lstUsersFileMissingHeaders = new List<string>();
            List<string> lstUsersFileInvalidInfo = new List<string>();

            List<string> lstCoursesFileMissingHeaders = new List<string>();
            List<string> lstCoursesFileInvalidInfo = new List<string>();

            List<string> lstClassesFileMissingHeaders = new List<string>();
            List<string> lstClassesFileInvalidInfo = new List<string>();

            List<string> lstEnrollmentsFileMissingHeaders = new List<string>();
            List<string> lstEnrollmentsFileInvalidInfo = new List<string>();

            List<string> lstAcademicSessionsFileMissingHeaders = new List<string>();
            List<string> lstAcademicSessionsFileInvalidInfo = new List<string>();

            using (var singleFileStream = singleEntry.Open())
            {
                using (System.IO.StreamReader strReader = new System.IO.StreamReader(singleFileStream))
                {
                    using (CsvHelper.CsvReader csvreader = new CsvHelper.CsvReader(strReader))
                    {
                        switch (singleEntry.Name)
                        {
                            case "orgs.csv":
                                ValidateOrgsCSVFile(lstOrgFileMissingHeaders, lstOrgFileInvalidInfo, csvreader);
                                break;
                            case "users.csv":
                                ValidateUsersCSVFile(lstUsersFileMissingHeaders, lstUsersFileInvalidInfo, csvreader);
                                break;
                            case "courses.csv":
                                ValidateCoursesCSVFile(lstCoursesFileMissingHeaders, lstCoursesFileInvalidInfo, csvreader);
                                break;
                            case "classes.csv":
                                ValidateClassesCSVFile(lstClassesFileMissingHeaders, lstClassesFileInvalidInfo, csvreader);
                                break;
                            case "enrollments.csv":
                                ValidateEnrollmentsCSVFile(lstEnrollmentsFileMissingHeaders, lstEnrollmentsFileInvalidInfo, csvreader);
                                break;
                            case "academicSessions.csv":
                                ValidateAcademicSessionsCSVFile(lstAcademicSessionsFileMissingHeaders, lstAcademicSessionsFileInvalidInfo, csvreader);
                                break;
                        }
                    }
                    strReader.Close();
                }
                singleFileStream.Close();
            }
            if (lstOrgFileMissingHeaders.Count > 0 || lstOrgFileInvalidInfo.Count > 0 ||
                lstUsersFileMissingHeaders.Count > 0 || lstUsersFileInvalidInfo.Count > 0 ||
                lstCoursesFileMissingHeaders.Count > 0 || lstCoursesFileInvalidInfo.Count > 0 ||
                lstClassesFileMissingHeaders.Count > 0 || lstClassesFileInvalidInfo.Count > 0 ||
                lstEnrollmentsFileMissingHeaders.Count > 0 || lstEnrollmentsFileInvalidInfo.Count > 0 ||
                lstAcademicSessionsFileMissingHeaders.Count > 0 || lstAcademicSessionsFileInvalidInfo.Count > 0)
            {
                StringBuilder strErrors = new StringBuilder();
                if (lstOrgFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for orgs.csv: ");
                    strErrors.AppendLine(string.Join(",", lstOrgFileInvalidInfo.ToArray()));
                }
                if (lstOrgFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for orgs.csv: ");
                    strErrors.AppendLine(string.Join(",", lstOrgFileMissingHeaders.ToArray()));
                }

                if (lstUsersFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for users.csv: ");
                    strErrors.AppendLine(string.Join(",", lstUsersFileInvalidInfo.ToArray()));
                }
                if (lstUsersFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for users.csv: ");
                    strErrors.AppendLine(string.Join(",", lstUsersFileInvalidInfo.ToArray()));
                }

                if (lstCoursesFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for courses.csv: ");
                    strErrors.AppendLine(string.Join(",", lstCoursesFileInvalidInfo.ToArray()));
                }
                if (lstCoursesFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for courses.csv: ");
                    strErrors.AppendLine(string.Join(",", lstCoursesFileMissingHeaders.ToArray()));
                }

                if (lstClassesFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for classes.csv: ");
                    strErrors.AppendLine(string.Join(",", lstClassesFileInvalidInfo.ToArray()));
                }
                if (lstClassesFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for classes.csv: ");
                    strErrors.AppendLine(string.Join(",", lstClassesFileMissingHeaders.ToArray()));
                }

                if (lstEnrollmentsFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for enrollments.csv: ");
                    strErrors.AppendLine(string.Join(",", lstEnrollmentsFileInvalidInfo.ToArray()));
                }
                if (lstEnrollmentsFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for enrollments.csv: ");
                    strErrors.AppendLine(string.Join(",", lstEnrollmentsFileMissingHeaders.ToArray()));
                }

                if (lstAcademicSessionsFileInvalidInfo.Count > 0)
                {
                    strErrors.AppendLine("Invalid info for academicsessions.csv: ");
                    strErrors.AppendLine(string.Join(",", lstAcademicSessionsFileInvalidInfo.ToArray()));
                }
                if (lstAcademicSessionsFileMissingHeaders.Count > 0)
                {
                    strErrors.AppendLine("Missing headers for academicsessions.csv: ");
                    strErrors.AppendLine(string.Join(",", lstAcademicSessionsFileMissingHeaders.ToArray()));
                }

                string errorsText = strErrors.ToString();
                Assert.Fail(errorsText);
            }
        }

        private static void ValidateOrgsCSVFile(List<string> lstOrgFileMissingHeaders, List<string> lstOrgFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
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

                ValidateField("sourcedId", null, lstOrgFileInvalidInfo, iRow,
                    csvreader);
                //ValidateField("status", new string[] { "active", "inactive", "tobedeleted" }, lstOrgFileInvalidInfo, iRow,
                //    csvreader);
                //string dateLastModified = string.Empty;
                //bool hasDateLastModified = csvreader.TryGetField<string>("dateLastModified", out dateLastModified);
                //if (String.IsNullOrWhiteSpace(dateLastModified))
                //{
                //    lstOrgFileInvalidInfo.Add("Empty dateLastModified for Row: " + iRow + ". Format must be YYYY-MM-DD");
                //}
                //else
                //{
                //    if (hasDateLastModified && dateLastModified.Length == 8)
                //    {
                //        DateTime resultDate;
                //        bool validDate = DateTime.TryParseExact(dateLastModified, "YYYY-MM-DD", null, System.Globalization.DateTimeStyles.None,
                //            out resultDate);
                //        if (!validDate)
                //        {
                //            lstOrgFileInvalidInfo.Add("Invalid dateLastModified for Row: " + iRow + ". Format must be YYYY-MM-DD");
                //        }
                //    }
                //    else
                //    {
                //        lstOrgFileInvalidInfo.Add("Invalid format for date: " + dateLastModified);
                //    }
                //}
                ValidateField("name", null, lstOrgFileInvalidInfo, iRow,
                    csvreader);
                ValidateField("type", new string[] { "school", "local", "state", "national" }, lstOrgFileInvalidInfo, iRow,
                    csvreader);
                ValidateField("identifier", null, lstOrgFileInvalidInfo, iRow,
                    csvreader);
                //ValidateField("metadata.classification", new string[] { "charter", "private", "public" }, lstOrgFileInvalidInfo, iRow, csvreader);
                //ValidateField("metadata.gender", null, lstOrgFileInvalidInfo, iRow, csvreader);
                //ValidateField("metadata.boarding", null, lstOrgFileInvalidInfo, iRow, csvreader);
                ValidateField("parentSourcedId", null, lstOrgFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        private static void ValidateUsersCSVFile(List<string> lstFileMissingHeaders, List<string> lstFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
            if (csvreader.ReadHeader())
            {
                if (!csvreader.FieldHeaders.Contains("sourcedId"))
                {
                    lstFileMissingHeaders.Add("sourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("status"))
                {
                    lstFileMissingHeaders.Add("status");
                }
                if (!csvreader.FieldHeaders.Contains("dateLastModified"))
                {
                    lstFileMissingHeaders.Add("dateLastModified");
                }
                if (!csvreader.FieldHeaders.Contains("orgSourcedIds"))
                {
                    lstFileMissingHeaders.Add("orgSourcedIds");
                }
                if (!csvreader.FieldHeaders.Contains("role"))
                {
                    lstFileMissingHeaders.Add("role");
                }
                if (!csvreader.FieldHeaders.Contains("username"))
                {
                    lstFileMissingHeaders.Add("username");
                }
                if (!csvreader.FieldHeaders.Contains("userId"))
                {
                    lstFileMissingHeaders.Add("userId");
                }
                if (!csvreader.FieldHeaders.Contains("givenName"))
                {
                    lstFileMissingHeaders.Add("givenName");
                }
                if (!csvreader.FieldHeaders.Contains("familyName"))
                {
                    lstFileMissingHeaders.Add("familyName");
                }
                if (!csvreader.FieldHeaders.Contains("identifier"))
                {
                    lstFileMissingHeaders.Add("identifier");
                }
                if (!csvreader.FieldHeaders.Contains("email"))
                {
                    lstFileMissingHeaders.Add("email");
                }
                if (!csvreader.FieldHeaders.Contains("sms"))
                {
                    lstFileMissingHeaders.Add("sms");
                }
                if (!csvreader.FieldHeaders.Contains("phone"))
                {
                    lstFileMissingHeaders.Add("phone");
                }
                if (!csvreader.FieldHeaders.Contains("agents"))
                {
                    lstFileMissingHeaders.Add("agents");
                }
            }
            int iRow = 1;
            while (csvreader.Read())
            {

                ValidateField("sourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                //Disabling status validation temporarily, files do not have the column with data
                //ValidateField("status", new string[] { "active", "inactive", "tobedeleted" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("orgSourcedIds", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("role", new string[] { "teacher", "student", "parent", "guardian", "relative", "aide", "administrator" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("username", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("userId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("givenName", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("familyName", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("identifier", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("email", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("sms", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("phone", null, lstFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        private static void ValidateCoursesCSVFile(List<string> lstFileMissingHeaders, List<string> lstFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
            if (csvreader.ReadHeader())
            {
                if (!csvreader.FieldHeaders.Contains("sourcedId"))
                {
                    lstFileMissingHeaders.Add("sourcedId");
                }
                //if (!csvreader.FieldHeaders.Contains("status"))
                //{
                //    lstFileMissingHeaders.Add("status");
                //}
                if (!csvreader.FieldHeaders.Contains("schoolYearId"))
                {
                    lstFileMissingHeaders.Add("schoolYearId");
                }
                if (!csvreader.FieldHeaders.Contains("title"))
                {
                    lstFileMissingHeaders.Add("title");
                }
                if (!csvreader.FieldHeaders.Contains("courseCode"))
                {
                    lstFileMissingHeaders.Add("courseCode");
                }
                if (!csvreader.FieldHeaders.Contains("subjects"))
                {
                    lstFileMissingHeaders.Add("subjects");
                }
            }
            int iRow = 1;
            while (csvreader.Read())
            {

                ValidateField("sourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                //ValidateField("status", new string[] { "active", "inactive", "tobedeleted" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("schoolYearId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("title", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("courseCode", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("orgSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("subjects", null, lstFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        private static void ValidateClassesCSVFile(List<string> lstFileMissingHeaders, List<string> lstFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
            if (csvreader.ReadHeader())
            {
                if (!csvreader.FieldHeaders.Contains("sourcedId"))
                {
                    lstFileMissingHeaders.Add("sourcedId");
                }
                //if (!csvreader.FieldHeaders.Contains("status"))
                //{
                //    lstFileMissingHeaders.Add("status");
                //}
                if (!csvreader.FieldHeaders.Contains("title"))
                {
                    lstFileMissingHeaders.Add("title");
                }
                if (!csvreader.FieldHeaders.Contains("courseSourcedId"))
                {
                    lstFileMissingHeaders.Add("courseSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("classCode"))
                {
                    lstFileMissingHeaders.Add("classCode");
                }
                if (!csvreader.FieldHeaders.Contains("classType"))
                {
                    lstFileMissingHeaders.Add("classType");
                }
                if (!csvreader.FieldHeaders.Contains("schoolSourcedId"))
                {
                    lstFileMissingHeaders.Add("schoolSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("termSourcedId"))
                {
                    lstFileMissingHeaders.Add("termSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("subjects"))
                {
                    lstFileMissingHeaders.Add("subjects");
                }
            }
            int iRow = 1;
            while (csvreader.Read())
            {
                ValidateField("sourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                //ValidateField("status", new string[] { "active", "inactive", "tobedeleted" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("title", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("courseSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("classCode", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("classType", new string[] { "homeroom", "scheduled" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("schoolSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("termSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("subjects", null, lstFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        private static void ValidateEnrollmentsCSVFile(List<string> lstFileMissingHeaders, List<string> lstFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
            if (csvreader.ReadHeader())
            {
                if (!csvreader.FieldHeaders.Contains("sourcedId"))
                {
                    lstFileMissingHeaders.Add("sourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("classSourcedId"))
                {
                    lstFileMissingHeaders.Add("classSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("schoolSourcedId"))
                {
                    lstFileMissingHeaders.Add("schoolSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("userSourcedId"))
                {
                    lstFileMissingHeaders.Add("userSourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("role"))
                {
                    lstFileMissingHeaders.Add("role");
                }
            }
            int iRow = 1;
            while (csvreader.Read())
            {
                ValidateField("sourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("classSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("schoolSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("userSourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("role", new string[] { "student" , "teacher" , "parent" , "guardian" , "relative" ,
                    "aide", "administrator" }, lstFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        private static void ValidateAcademicSessionsCSVFile(List<string> lstFileMissingHeaders, List<string> lstFileInvalidInfo, CsvHelper.CsvReader csvreader)
        {
            if (csvreader.ReadHeader())
            {
                if (!csvreader.FieldHeaders.Contains("sourcedId"))
                {
                    lstFileMissingHeaders.Add("sourcedId");
                }
                if (!csvreader.FieldHeaders.Contains("title"))
                {
                    lstFileMissingHeaders.Add("title");
                }
                if (!csvreader.FieldHeaders.Contains("type"))
                {
                    lstFileMissingHeaders.Add("type");
                }
                if (!csvreader.FieldHeaders.Contains("startDate"))
                {
                    lstFileMissingHeaders.Add("startDate");
                }
                if (!csvreader.FieldHeaders.Contains("endDate"))
                {
                    lstFileMissingHeaders.Add("endDate");
                }
            }
            int iRow = 1;
            while (csvreader.Read())
            {
                ValidateField("sourcedId", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("title", null, lstFileInvalidInfo, iRow, csvreader);
                //ValidateField("type", new string[] {"term", "gradingPeriod", "schoolYear", "semester" }, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("startDate", null, lstFileInvalidInfo, iRow, csvreader);
                ValidateField("endDate", null, lstFileInvalidInfo, iRow, csvreader);
                iRow++;
            }
        }

        [TestMethod()]
        public async Task ExportsController_IndexTest()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
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
            Assert.IsTrue(modelResult.SchoolsCriteriaSection.FilterCheckboxes != null &&
                modelResult.SchoolsCriteriaSection.FilterCheckboxes.Count > 0, "No data for Schools Criteria");
        }

        private async Task<ExportsViewModel> GetDefaultExportsViewModel()
        {
            ExportsViewModel result = new ExportsViewModel();
            await ApiCalls.PopulateFilterSection1(result);
            FilterInputs filters = new FilterInputs();
            var schools = await ApiCalls.GetSchools();
            result.SelectedSchools = string.Join(",", schools.Select(p => p.SchoolId));
            return result;
        }

        private FilterInputs GenerateTestFilterInputs(ExportsViewModel viewModel)
        {
            var inputs = new FilterInputs
            {
                Schools = viewModel.SchoolsCriteriaSection.FilterCheckboxes.Select(p => p.SchoolId).Distinct().ToList(),
                //Subjects = viewModel.SubjectsCriteriaSection.FilterCheckboxes.Select(p => p.Subject).Distinct().ToList(),
                //Courses = viewModel.CoursesCriteriaSection.FilterCheckboxes.Select(p => p.Course).Distinct().ToList(),
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
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
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
            var partialResult = await controller.GetTeachersPartial(schoolIds: schoolIds,
                boxesAlreadyChecked: null, getMore: false);
            Assert.IsNotNull(partialResult, "Invalid result");
            Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Invalid result type");
            PartialViewResult partialViewResult = partialResult as PartialViewResult;
            Assert.IsNotNull(partialViewResult.Model, "Invalid model");
            Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
            ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
            Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
        }

        [TestMethod()]
        public async Task ExportsController_GetSectionsPartialTest()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
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
            var partialResult = await controller.GetSectionsPartial(schoolIds: schoolIds, boxesAlreadyChecked: null, getMore: false);
            Assert.IsNotNull(partialResult, "Invalid result");
            Assert.IsInstanceOfType(partialResult, typeof(PartialViewResult), "Unexpected result type");
            PartialViewResult partialViewResult = partialResult as PartialViewResult;
            Assert.IsNotNull(partialViewResult.Model, "Invalid model");
            Assert.IsInstanceOfType(partialViewResult.Model, typeof(ApiCriteriaSection), "Unexpected model type");
            ApiCriteriaSection apiCriteriaResult = partialViewResult.Model as ApiCriteriaSection;
            Assert.IsTrue(apiCriteriaResult.FilterCheckboxes.Count > 0, "No data found");
        }

        [TestMethod()]
        public async Task ExportsController_GetCoursesPartialTest()
        {
            AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(true);
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
            var partialResult = await controller.GetPreviewCoursesJsonString(0);
            Assert.IsNotNull(partialResult, "Invalid result");
            Assert.IsInstanceOfType(partialResult, typeof(JsonResult), "Unexpected result type");
            JsonResult partialViewResult = partialResult as JsonResult;
            Assert.IsNotNull(partialViewResult.Data, "Invalid model");
            Assert.IsInstanceOfType(partialViewResult.Data, typeof(ViewModels.DataPreviewPagedJsonModel), "Unexpected model type");
            ViewModels.DataPreviewPagedJsonModel apiCriteriaResult = partialViewResult.Data as ViewModels.DataPreviewPagedJsonModel;
            Assert.IsTrue(apiCriteriaResult.TotalPages > 0, "No data found");
        }

    }
}