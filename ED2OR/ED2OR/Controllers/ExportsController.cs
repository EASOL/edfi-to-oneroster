using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using ED2OR.Enums;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using ED2OR.Utils;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ED2OR.Controllers
{
    public class ExportsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = new ExportsViewModel();
            await InitializeModel(model);
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Index(ExportsViewModel model, string Command)
        {
            if (Command == "Preview")
            {
                await InitializeModel(model, true);

                model.JsonPreviews = await ApiCalls.GetJsonPreviews();

                var orgColumnNames = typeof(CsvOrgs).GetProperties().Select(x => x.Name);
                var usersColumnNames = typeof(CsvUsers).GetProperties().Select(x => x.Name);
                var coursesColumnNames = typeof(CsvCourses).GetProperties().Select(x => x.Name);
                var classesColumnNames = typeof(CsvClasses).GetProperties().Select(x => x.Name);
                var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Select(x => x.Name);
                var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Select(x => x.Name);

                model.DataPreviewSections = new List<DataPreviewSection>
                {
                    new DataPreviewSection
                    {
                        SectionName = "orgs",
                        ColumnNames = orgColumnNames
                    },
                    new DataPreviewSection
                    {
                        SectionName = "users",
                        ColumnNames = usersColumnNames
                    },
                    new DataPreviewSection
                    {
                        SectionName = "courses",
                        ColumnNames = coursesColumnNames
                    },
                    new DataPreviewSection
                    {
                        SectionName = "classes",
                        ColumnNames = classesColumnNames
                    },
                    new DataPreviewSection
                    {
                        SectionName = "enrollments",
                        ColumnNames = enrollmentsColumnNames
                    },
                    new DataPreviewSection
                    {
                        SectionName = "academicsessions",
                        ColumnNames = academicsessionsColumnNames
                    }
                };

                return View(model);
            }
            else if (Command == "Download")
            {
                return await GetZipFile();
            }
            else //"Save Template"
            {
                return View();
            }




            //var schoolsSection = model.CriteriaSections.Where(x => x.SectionName == "Schools").FirstOrDefault();
            //var selectedSchools = schoolsSection.FilterCheckboxes.Where(x => x.Selected).Select(x => x.SchoolId).ToList();

            //var subjects = await ApiCalls.GetSubjects();
            //subjects.RemoveAll(x => selectedSchools.Contains(x.SchoolId) == false);
            //var subjectsSection = model.CriteriaSections.Where(x => x.SectionName == "Subjects").FirstOrDefault();
            //subjectsSection.FilterCheckboxes = subjects;
        }

        //[HttpPost]
        //public async Task<JsonResult> GetSubjectsCheckboxes(List<string> schoolIds)
        //{
        //    if (schoolIds == null || schoolIds.Count() == 0)
        //    {
        //        return Json(new List<string>(), JsonRequestBehavior.AllowGet);
        //    }

        //    var subjects = await ApiCalls.GetSubjects();
        //    var filteredSubjects = subjects.Where(x => schoolIds.Contains(x.SchoolId)).GroupBy(x => x.Text).Select(group => group.First());
        //    return Json(filteredSubjects.Select(x => x.Text).ToList(), JsonRequestBehavior.AllowGet);
        //}

        public async Task<ActionResult> GetSubjectsPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Subjects";

            var model = new ApiCriteriaSection
            {
                SectionName = "Subjects",
                IsExpanded = true
            };

            if (schoolIds == null || schoolYears == null || terms == null || schoolIds.Count() == 0 || schoolYears.Count() == 0 || terms.Count() == 0)
            {
                return PartialView("_CriteriaSection", model);
            }

            var subjects = await ApiCalls.GetSubjects();
            var filteredSubjects = subjects.Where(x =>
                schoolIds.Contains(x.SchoolId) &&
                schoolYears.Contains(x.SchoolYear) &&
                terms.Contains(x.Term)).GroupBy(x => x.Text).Select(group => group.First());

            var subjectsListNewInstance = new List<ExportsCheckbox>(); //you need to make a new instance because when you check the boxes, it affects the session var
            subjectsListNewInstance.AddRange(filteredSubjects);

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = subjectsListNewInstance.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = subjectsListNewInstance;
            
            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetCoursesPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Courses";

            var model = new ApiCriteriaSection
            {
                SectionName = "Courses",
                IsExpanded = true
            };

            if (schoolIds == null || schoolYears == null || terms == null || schoolIds.Count() == 0 || schoolYears.Count() == 0 || terms.Count() == 0)
            {
                return PartialView("_CriteriaSection", model);
            }

            var courses = await ApiCalls.GetCourses();
            var filteredCourses = courses.Where(x =>
                schoolIds.Contains(x.SchoolId) &&
                schoolYears.Contains(x.SchoolYear) &&
                terms.Contains(x.Term)).GroupBy(x => x.Text).Select(group => group.First());

            var coursesListNewInstance = new List<ExportsCheckbox>(); //you need to make a new instance because when you check the boxes, it affects the session var
            coursesListNewInstance.AddRange(filteredCourses);

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = coursesListNewInstance.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = coursesListNewInstance;

            return PartialView("_CriteriaSection", model);
        }

        //private void FilterSubjects(List<ExportsCheckbox> schools, List<ExportsCheckbox> subjects)
        //{
        //    var selectedSchools = schools.Where(x => x.Selected).Select(x => x.SchoolId).ToList();
        //    subjects.RemoveAll(x => selectedSchools.Contains(x.SchoolId) == false);
        //}

        private async Task InitializeModel(ExportsViewModel model, bool collapseAll = false)
        {

            /*
            First Step - user has just opened the filter screen.
Second Step - user has selected one of the facets from the first step.
“Subjects” - /enrollment/sections endpoint. (acedemicSubjectDescriptor)
“Course” - /enrollment/sections endpoint  (courseOfferingReference.localCourseCode)
“Teachers” - /enrollment/sectionEnrollments (staff.firstName + staff.lastSurname)

Third Step - user has selected one of the facets from the second step.
Sections” - /enrollment/sections endpoint (uniqueSectionCode).
*/

            await ApiCalls.PopulateFilterSection1(model, collapseAll);

            model.SubjectsCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Subjects"
            };

            model.CoursesCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Courses"
            };

            model.TeachersCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Teachers"
            };

            model.SectionsCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Sections"
            };
        }

        private async Task<FileResult> GetZipFile()
        {
            var csvFilesDirectory = "~/CsvFiles";
            var csvDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(csvFilesDirectory);

            var directoryGuid = Guid.NewGuid().ToString();
            var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
            var tempDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(tempDirectory);

            Directory.CreateDirectory(tempDirectoryFullName);
            WriteObjectToCsv(await ApiCalls.GetCsvOrgs(), tempDirectoryFullName, "orgs.csv");
            WriteObjectToCsv(await ApiCalls.GetCsvUsers(), tempDirectoryFullName, "users.csv");
            WriteObjectToCsv(await ApiCalls.GetCsvCourses(), tempDirectoryFullName, "courses.csv");
            WriteObjectToCsv(await ApiCalls.GetCsvClasses(), tempDirectoryFullName, "classes.csv");
            WriteObjectToCsv(await ApiCalls.GetCsvEnrollments(), tempDirectoryFullName, "enrollments.csv");
            WriteObjectToCsv(await ApiCalls.GetCsvAcademicSessions(), tempDirectoryFullName, "academicsessions.csv");

            var zipPath = Path.Combine(csvDirectoryFullName, directoryGuid + ".zip");

            var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\orgs.csv", "orgs.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\users.csv", "users.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\courses.csv", "courses.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\classes.csv", "classes.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\enrollments.csv", "enrollments.csv");
            zip.CreateEntryFromFile(tempDirectoryFullName + "\\academicsessions.csv", "academicsessions.csv");
            zip.Dispose();

            var bytes = System.IO.File.ReadAllBytes(zipPath); //if this eats memory there are other options: http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
            Directory.Delete(tempDirectoryFullName, true);
            System.IO.File.Delete(zipPath);
            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }

        private void WriteObjectToCsv<T>(List<T> inputList, string directoryPath, string fileName)
        {
            var filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                var columnNames = typeof(T).GetProperties().Select(x => x.Name);
                var headerLine = string.Join(",", columnNames);
                sw.WriteLine(headerLine);

                foreach (var rec in inputList)
                {
                    var newLine = new List<string>();
                    foreach (string prop in columnNames)
                    {
                        newLine.Add(rec.GetType().GetProperty(prop).GetValue(rec, null)?.ToString() ?? "");
                    }
                    sw.WriteLine(string.Join(",", newLine));
                }
            }
        }
       
    }
}