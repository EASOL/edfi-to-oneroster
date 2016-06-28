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
using System.Dynamic;

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
        public async Task<ActionResult> Preview(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> subjects,
            List<string> courses,
            List<string> teachers,
            List<string> sections)
        {
            var model = new ExportsViewModel();
            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                SchoolYears = schoolYears,
                Terms = terms,
                Subjects = subjects,
                Courses = courses,
                Teachers = teachers,
                Sections = sections
            };

            model.JsonPreviews = await ApiCalls.GetJsonPreviews(inputs);

            var orgColumnNames = typeof(CsvOrgs).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
            var usersColumnNames = typeof(CsvUsers).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
            var coursesColumnNames = typeof(CsvCourses).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
            var classesColumnNames = typeof(CsvClasses).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
            var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
            var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);

            //var orgColumnNames = typeof(CsvOrgs).GetProperties().Select(x => x.Name);
            //var usersColumnNames = typeof(CsvUsers).GetProperties().Select(x => x.Name);
            //var coursesColumnNames = typeof(CsvCourses).GetProperties().Select(x => x.Name);
            //var classesColumnNames = typeof(CsvClasses).GetProperties().Select(x => x.Name);
            //var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Select(x => x.Name);
            //var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Select(x => x.Name);

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

            return PartialView("_DataPreview", model);
        }

        [HttpPost]
        public async Task<FileResult> DownloadCsv(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> subjects,
            List<string> courses,
            List<string> teachers,
            List<string> sections)
        {
            var model = new DataResults();
            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                SchoolYears = schoolYears,
                Terms = terms,
                Subjects = subjects,
                Courses = courses,
                Teachers = teachers,
                Sections = sections
            };
            model = await ApiCalls.GetDataResults(inputs);
                //schoolIds,
                //schoolYears,
                //terms,
                //subjects,
                //courses,
                //teachers,
                //sections);

            var csvFilesDirectory = "~/CsvFiles";
            var csvDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(csvFilesDirectory);

            var directoryGuid = Guid.NewGuid().ToString();
            var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
            var tempDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(tempDirectory);

            Directory.CreateDirectory(tempDirectoryFullName);
            WriteObjectToCsv(model.Orgs, tempDirectoryFullName, "orgs.csv");
            WriteObjectToCsv(model.Users, tempDirectoryFullName, "users.csv");
            WriteObjectToCsv(model.Courses, tempDirectoryFullName, "courses.csv");
            WriteObjectToCsv(model.Classes, tempDirectoryFullName, "classes.csv");
            WriteObjectToCsv(model.Enrollments, tempDirectoryFullName, "enrollments.csv");
            WriteObjectToCsv(model.AcademicSessions, tempDirectoryFullName, "academicsessions.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvOrgs(), tempDirectoryFullName, "orgs.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvUsers(), tempDirectoryFullName, "users.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvCourses(), tempDirectoryFullName, "courses.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvClasses(), tempDirectoryFullName, "classes.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvEnrollments(), tempDirectoryFullName, "enrollments.csv");
            //WriteObjectToCsv(await ApiCalls.GetCsvAcademicSessions(), tempDirectoryFullName, "academicsessions.csv");

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

        [HttpPost]
        public async Task<ActionResult> Index(ExportsViewModel model, string Command)
        {
            //if (Command == "Preview")
            //{
            //    await InitializeModel(model, true);

            //    model.JsonPreviews = await ApiCalls.GetJsonPreviews();

            //    var orgColumnNames = typeof(CsvOrgs).GetProperties().Select(x => x.Name);
            //    var usersColumnNames = typeof(CsvUsers).GetProperties().Select(x => x.Name);
            //    var coursesColumnNames = typeof(CsvCourses).GetProperties().Select(x => x.Name);
            //    var classesColumnNames = typeof(CsvClasses).GetProperties().Select(x => x.Name);
            //    var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Select(x => x.Name);
            //    var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Select(x => x.Name);

            //    model.DataPreviewSections = new List<DataPreviewSection>
            //    {
            //        new DataPreviewSection
            //        {
            //            SectionName = "orgs",
            //            ColumnNames = orgColumnNames
            //        },
            //        new DataPreviewSection
            //        {
            //            SectionName = "users",
            //            ColumnNames = usersColumnNames
            //        },
            //        new DataPreviewSection
            //        {
            //            SectionName = "courses",
            //            ColumnNames = coursesColumnNames
            //        },
            //        new DataPreviewSection
            //        {
            //            SectionName = "classes",
            //            ColumnNames = classesColumnNames
            //        },
            //        new DataPreviewSection
            //        {
            //            SectionName = "enrollments",
            //            ColumnNames = enrollmentsColumnNames
            //        },
            //        new DataPreviewSection
            //        {
            //            SectionName = "academicsessions",
            //            ColumnNames = academicsessionsColumnNames
            //        }
            //    };

            //    return View(model);
            //}
            //if (Command == "Download")
            //{
            var schools = model.SelectedSchools?.Split(',').ToList();
            var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            var terms = model.SelectedTerms?.Split(',').ToList();
            var subjects = model.SelectedSubjects?.Split(',').ToList();
            var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

            return await DownloadCsv(schools, schoolYears, terms, subjects, courses, teachers, sections);
            //return await GetZipFile();
            //}
            //else //"Save Template"
            //{
            //    return View();
            //}
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

            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;
        
            var subjects = await ApiCalls.GetSubjects();
            var filteredSubjects = new List<ExportsCheckbox>();
            filteredSubjects.AddRange(subjects);

            if (!allSchools)
            {
                filteredSubjects = filteredSubjects.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredSubjects = filteredSubjects.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredSubjects = filteredSubjects.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredSubjects = filteredSubjects.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = filteredSubjects.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = filteredSubjects;
            
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

            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;

            var courses = await ApiCalls.GetCourses();
            var filteredCourses = new List<ExportsCheckbox>();
            filteredCourses.AddRange(courses);

            if (!allSchools)
            {
                filteredCourses = filteredCourses.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredCourses = filteredCourses.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredCourses = filteredCourses.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredCourses = filteredCourses.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = filteredCourses.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = filteredCourses;

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetTeachersPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Teachers";

            var model = new ApiCriteriaSection
            {
                SectionName = "Teachers",
                IsExpanded = true
            };

            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;

            var teachers = await ApiCalls.GetTeachers();
            var filteredTeachers = new List<ExportsCheckbox>();
            filteredTeachers.AddRange(teachers);

            if (!allSchools)
            {
                filteredTeachers = filteredTeachers.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredTeachers = filteredTeachers.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredTeachers = filteredTeachers.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredTeachers = filteredTeachers.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = filteredTeachers.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = filteredTeachers;

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetSectionsPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Sections";

            var model = new ApiCriteriaSection
            {
                SectionName = "Sections",
                IsExpanded = true
            };

            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;

            var sections = await ApiCalls.GetSections();
            var filteredSections = new List<ExportsCheckbox>();
            filteredSections.AddRange(sections);

            if (!allSchools)
            {
                filteredSections = filteredSections.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredSections = filteredSections.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredSections = filteredSections.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredSections = filteredSections.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = filteredSections.Where(x => boxesAlreadyChecked.Contains(x.Text)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }

            model.FilterCheckboxes = filteredSections;

            return PartialView("_CriteriaSection", model);
        }

        private async Task InitializeModel(ExportsViewModel model, bool collapseAll = false)
        {
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

        //private async Task<FileResult> GetZipFile()
        //{
        //    var csvFilesDirectory = "~/CsvFiles";
        //    var csvDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(csvFilesDirectory);

        //    var directoryGuid = Guid.NewGuid().ToString();
        //    var tempDirectory = csvFilesDirectory + "/" + directoryGuid;
        //    var tempDirectoryFullName = System.Web.HttpContext.Current.Server.MapPath(tempDirectory);

        //    Directory.CreateDirectory(tempDirectoryFullName);
        //    WriteObjectToCsv(await ApiCalls.GetCsvOrgs(), tempDirectoryFullName, "orgs.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvUsers(), tempDirectoryFullName, "users.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvCourses(), tempDirectoryFullName, "courses.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvClasses(), tempDirectoryFullName, "classes.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvEnrollments(), tempDirectoryFullName, "enrollments.csv");
        //    WriteObjectToCsv(await ApiCalls.GetCsvAcademicSessions(), tempDirectoryFullName, "academicsessions.csv");

        //    var zipPath = Path.Combine(csvDirectoryFullName, directoryGuid + ".zip");

        //    var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\orgs.csv", "orgs.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\users.csv", "users.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\courses.csv", "courses.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\classes.csv", "classes.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\enrollments.csv", "enrollments.csv");
        //    zip.CreateEntryFromFile(tempDirectoryFullName + "\\academicsessions.csv", "academicsessions.csv");
        //    zip.Dispose();

        //    var bytes = System.IO.File.ReadAllBytes(zipPath); //if this eats memory there are other options: http://stackoverflow.com/questions/2041717/how-to-delete-file-after-download-with-asp-net-mvc
        //    Directory.Delete(tempDirectoryFullName, true);
        //    System.IO.File.Delete(zipPath);
        //    var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
        //    return File(bytes, "application/zip", downloadFileName);
        //}

        private void WriteObjectToCsv<T>(List<T> inputList, string directoryPath, string fileName)
        {
            var filePath = Path.Combine(directoryPath, fileName);

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                var columnNames = typeof(T).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name);
                var headerLine = string.Join(",", columnNames);
                sw.WriteLine(headerLine);

                foreach (var rec in inputList)
                {
                    var newLine = new List<string>();
                    foreach (string prop in columnNames)
                    {
                        var stringVal = rec.GetType().GetProperty(prop).GetValue(rec, null)?.ToString() ?? "";
                        newLine.Add("\"" + stringVal + "\"");
                    }
                    sw.WriteLine(string.Join(",", newLine));
                }
            }
        }
       
    }
}