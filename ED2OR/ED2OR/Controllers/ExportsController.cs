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
using Newtonsoft.Json;
using ED2OR.Models;

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

        public async Task<ActionResult> PreviewFromTemplate(int templateId)
        {
            var filtersJson = db.Templates.First(x => x.TemplateId == templateId).Filters;
            var filters = JsonConvert.DeserializeObject<FilterInputs>(filtersJson);

            return await Preview(
                filters.Schools,
                filters.SchoolYears,
                filters.Terms,
                filters.Subjects,
                filters.Courses,
                filters.Teachers,
                filters.Sections);
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
        public async Task<ActionResult> DownloadCsv(ExportsViewModel model)
        {
            var schools = model.SelectedSchools?.Split(',').ToList();
            var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            var terms = model.SelectedTerms?.Split(',').ToList();
            var subjects = model.SelectedSubjects?.Split(',').ToList();
            var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

            var csvUtils = new CsvMethods();
            var bytes = await csvUtils.GetZipFile(schools, schoolYears, terms, subjects, courses, teachers, sections);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }

        [HttpPost]
        public ActionResult SaveTemplate(ExportsViewModel model)
        {
            var schoolIds = model.SelectedSchools?.Split(',').ToList();
            var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            var terms = model.SelectedTerms?.Split(',').ToList();
            var subjects = model.SelectedSubjects?.Split(',').ToList();
            var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

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

            var inputsJson = JsonConvert.SerializeObject(inputs);

            var template = new Template
            {
                TemplateName = model.NewTemplateName,
                VendorName = model.NewTemplateVendorName,
                Filters = inputsJson
            };

            db.Templates.Add(template);
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index", "Templates");
        }

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

            filteredSubjects.ForEach(c => c.Selected = false); // make sure all are unchecked first
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

            filteredCourses.ForEach(c => c.Selected = false); // make sure all are unchecked first
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

            filteredTeachers.ForEach(c => c.Selected = false); // make sure all are unchecked first
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

            filteredSections.ForEach(c => c.Selected = false); // make sure all are unchecked first
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
       
    }
}