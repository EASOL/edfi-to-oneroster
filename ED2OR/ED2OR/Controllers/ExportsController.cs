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
            var subjects = await ApiCalls.GetSubjects();

            

            //if (schools != null)
            //{

            //}
            //var schoolsCbs = (from s in schools
            //                select new ExportsCheckbox
            //                {
            //                    Id = s.id,
            //                    SchoolId = s.schoolId,
            //                    Text = s.nameOfInstitution,
            //                    Visible = true
            //                }).ToList();


            model.CriteriaSections = new List<ApiCriteriaSection>
            {
                new ApiCriteriaSection
                {
                    FilterCheckboxes = schools,
                    SectionName = "Schools",
                    Level = 1
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = subjects,
                    SectionName = "Subjects",
                    Level = 2
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Courses",
                    Level = 3
                },
                new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
                    SectionName = "Sections",
                    Level = 3
                }
                ,new ApiCriteriaSection
                {
                    FilterCheckboxes = GenerateCbList(),
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

        [HttpPost]
        public async Task<JsonResult> GetSubjectsCheckboxes(List<string> schoolIds)
        {
            var subjects = await ApiCalls.GetSubjects();
            var filteredSubjects = subjects.Where(x => schoolIds.Contains(x.SchoolId)).Select(x => new ExportsCheckbox
            {
                Id = x.Id,
                Text = x.Text,
                SchoolId = x.SchoolId
            }).ToList();

            return Json(filteredSubjects, JsonRequestBehavior.AllowGet);
        }

        //private void FilterSubjects(List<ExportsCheckbox> schools, List<ExportsCheckbox> subjects)
        //{
        //    var selectedSchools = schools.Where(x => x.Selected).Select(x => x.SchoolId).ToList();
        //    subjects.RemoveAll(x => selectedSchools.Contains(x.SchoolId) == false);
        //}

        public List<ExportsCheckbox> GenerateCbList()
        {
            return new List<ExportsCheckbox>
                {
                    new ExportsCheckbox
                    {
                        Id = "a",
                        Selected = true,
                        Text = "abc"
                    },
                     new ExportsCheckbox
                    {
                        Id = "b",
                        Selected = false,
                        Text = "def"
                    },
                      new ExportsCheckbox
                    {
                        Id = "c",
                        Selected = true,
                        Text = "ghi"
                    }
                };
        }

        public async Task<FileResult> GetZipFile()
        {
            var csvFilesDirectory = "~/CsvFiles";
            var csvDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(csvFilesDirectory);

            var directoryGuid = Guid.NewGuid().ToString();
            var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
            var tempDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(tempDirectory);

            Directory.CreateDirectory(tempDirectoryFullName);
            WriteObjectToCsv(await ApiCalls.GetCsvOrgs(), tempDirectoryFullName, "ORGS.CSV");
            WriteObjectToCsv(await ApiCalls.GetCsvUsers(), tempDirectoryFullName, "USERS.CSV");
            WriteObjectToCsv(await ApiCalls.GetCsvCourses(), tempDirectoryFullName, "COURSES.CSV");
            WriteObjectToCsv(await ApiCalls.GetCsvClasses(), tempDirectoryFullName, "CLASSES.CSV");
            WriteObjectToCsv(await ApiCalls.GetCsvEnrollments(), tempDirectoryFullName, "ENROLLMENTS.CSV");
            WriteObjectToCsv(await ApiCalls.GetCsvAcademicSessions(), tempDirectoryFullName, "ACADEMICSESSIONS.CSV");

            var zipPath = Path.Combine(csvDirectoryFullName, directoryGuid + ".zip");
            ZipFile.CreateFromDirectory(tempDirectoryFullName, zipPath, CompressionLevel.Fastest, true);


            var bytes = System.IO.File.ReadAllBytes(zipPath); //if this eats memory there are other options http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
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