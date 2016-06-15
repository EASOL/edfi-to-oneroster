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
            var schools = await ApiCalls.GetSchools();
            var subjects = await ApiCalls.GetSubjects(); //this is never used here, but it's good to call it so it's loaded into the session

            model.CriteriaSections = new List<ApiCriteriaSection>
            {
                new ApiCriteriaSection
                {
                    FilterCheckboxes = schools,
                    SectionName = "Schools",
                    IsExpanded = true,
                    Level = 1
                },
                new ApiCriteriaSection
                {
                    SectionName = "Subjects",
                    Level = 2
                },
                new ApiCriteriaSection
                {
                    SectionName = "Courses",
                    Level = 3
                },
                new ApiCriteriaSection
                {
                    SectionName = "Sections",
                    Level = 3
                }
                ,new ApiCriteriaSection
                {
                    SectionName = "Teachers",
                    Level = 3
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(ExportsViewModel model)
        {
            var schoolsSection = model.CriteriaSections.Where(x => x.SectionName == "Schools").FirstOrDefault();
            var selectedSchools = schoolsSection.FilterCheckboxes.Where(x => x.Selected).Select(x => x.SchoolId).ToList();

            var subjects = await ApiCalls.GetSubjects();
            subjects.RemoveAll(x => selectedSchools.Contains(x.SchoolId) == false);
            var subjectsSection = model.CriteriaSections.Where(x => x.SectionName == "Subjects").FirstOrDefault();
            subjectsSection.FilterCheckboxes = subjects;
            //var selectedSchools = schools.Where(x => x.Selected).Select(x => x.SchoolId).ToList();
            return View(model);
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

        public async Task<ActionResult> GetSubjectsPartial(List<string> schoolIds, List<string> boxesAlreadyChecked)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Subjects";

            var model = new ApiCriteriaSection
            {
                SectionName = "Subjects",
                Level = 2,
                IsExpanded = true
            };

            if (schoolIds == null || schoolIds.Count() == 0)
            {
                return PartialView("_CriteriaSection", model);
            }

            var subjects = await ApiCalls.GetSubjects();
            var filteredSubjects = subjects.Where(x => schoolIds.Contains(x.SchoolId)).GroupBy(x => x.Text).Select(group => group.First()).ToList();

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = filteredSubjects.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = filteredSubjects;
            
            return PartialView("_CriteriaSection", model);
        }

        //private void FilterSubjects(List<ExportsCheckbox> schools, List<ExportsCheckbox> subjects)
        //{
        //    var selectedSchools = schools.Where(x => x.Selected).Select(x => x.SchoolId).ToList();
        //    subjects.RemoveAll(x => selectedSchools.Contains(x.SchoolId) == false);
        //}
        

        public async Task<FileResult> GetZipFile()
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
            ZipFile.CreateFromDirectory(tempDirectoryFullName, zipPath, CompressionLevel.Fastest, true);


            var bytes = System.IO.File.ReadAllBytes(zipPath); //if this eats memory there are other options: http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
            Directory.Delete(tempDirectoryFullName, true);
            System.IO.File.Delete(zipPath);
            return File(bytes, "application/zip");
        }

        public void WriteObjectToCsv<T>(List<T> inputList, string directoryPath, string fileName)
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

        public FileResult ObjectToCsv<T>(List<T> inputList, string fileName)
        {
            var csv = new StringBuilder();
            var columnNames = typeof(T).GetProperties().Select(x => x.Name);
            var headerLine = string.Join(",", columnNames);
            csv.AppendLine(headerLine);

            foreach (var rec in inputList)
            {
                var newLine = new List<string>();
                foreach (string prop in columnNames)
                {
                    newLine.Add(rec.GetType().GetProperty(prop).GetValue(rec, null)?.ToString() ?? "");
                }
                csv.AppendLine(string.Join(",", newLine));
            }
            return File(new UTF8Encoding().GetBytes(csv.ToString()), "text/csv", fileName);
        }

       
    }
}