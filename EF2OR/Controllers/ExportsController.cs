using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EF2OR.ViewModels;
using System.Threading.Tasks;
using EF2OR.Utils;
using Newtonsoft.Json;
using EF2OR.Models;
using EF2OR.Enums;

namespace EF2OR.Controllers
{
    public class ExportsController : BaseController
    {
        public async Task<ActionResult> Index(int? templateId = null)
        {
            var model = new ExportsViewModel();

            await InitializeModel(model);

            if (templateId != null)
            {
                await InitializeModelForEdit(model, templateId.Value);
            }

            return View(model);
        }

        public async Task<ActionResult> PreviewFromTemplate(int templateId)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            var filters = JsonConvert.DeserializeObject<FilterInputs>(template.Filters);

            return await Preview(
                filters.Schools,
                //filters.SchoolYears,
                //filters.Terms,
                //filters.Subjects,
                //filters.Courses,
                filters.Teachers,
                filters.Sections,
                template.OneRosterVersion);
        }

        [HttpPost]
        public async Task<ActionResult> Preview(List<string> schoolIds,
            //List<string> schoolYears,
            //List<string> terms,
            //List<string> subjects,
            //List<string> courses,
            List<string> teachers,
            List<string> sections,
            string oneRosterVersion)
        {
            var model = new ExportsViewModel();
            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                //SchoolYears = schoolYears,
                //Terms = terms,
                //Subjects = subjects,
                //Courses = courses,
                Teachers = teachers,
                Sections = sections
            };

            model.JsonPreviews = await ApiCalls.GetJsonPreviews(inputs, oneRosterVersion);

            IEnumerable<string> orgColumnNames;
            IEnumerable<string> usersColumnNames;
            IEnumerable<string> coursesColumnNames;
            IEnumerable<string> classesColumnNames;
            IEnumerable<string> enrollmentsColumnNames;
            IEnumerable<string> academicsessionsColumnNames;

            if (oneRosterVersion == OneRosterVersions.OR_1_0)
            {
                orgColumnNames = typeof(CsvOrgs).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                usersColumnNames = typeof(CsvUsers).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                coursesColumnNames = typeof(CsvCourses).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                classesColumnNames = typeof(CsvClasses).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR10IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            }
            else
            {
                orgColumnNames = typeof(CsvOrgs).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                usersColumnNames = typeof(CsvUsers).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                coursesColumnNames = typeof(CsvCourses).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                classesColumnNames = typeof(CsvClasses).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            }

            model.DataPreviewSections = new List<DataPreviewSection>
                {
                    new DataPreviewSection
                    {
                        SectionName = "orgs",
                        ColumnNames = orgColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.OrgsTotalPages
                    },
                    new DataPreviewSection
                    {
                        SectionName = "users",
                        ColumnNames = usersColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.UsersTotalPages
                    },
                    new DataPreviewSection
                    {
                        SectionName = "courses",
                        ColumnNames = coursesColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.CoursesTotalPages
                    },
                    new DataPreviewSection
                    {
                        SectionName = "classes",
                        ColumnNames = classesColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.ClassesTotalPages
                    },
                    new DataPreviewSection
                    {
                        SectionName = "enrollments",
                        ColumnNames = enrollmentsColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.EnrollmentsTotalPages
                    },
                    new DataPreviewSection
                    {
                        SectionName = "academicsessions",
                        ColumnNames = academicsessionsColumnNames,
                        CurrentPage = 1,
                        TotalPages = model.JsonPreviews.AcademicSessionsTotalPages
                    }
                };

            if (oneRosterVersion == OneRosterVersions.OR_1_1)
            {
                var manifestColumnNames = typeof(CsvManifest).GetProperties().Where(x => Attribute.IsDefined(x, typeof(OR11IncludeFieldAttribute))).Select(x => x.Name.Replace("__", "."));
                model.DataPreviewSections.Add(new DataPreviewSection
                {
                    SectionName = "manifest",
                    ColumnNames = manifestColumnNames
                });
            }

            return PartialView("_DataPreview", model);
        }

        public async Task<JsonResult> GetPreviewOrgsJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewOrgsJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPreviewUsersJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewUsersJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPreviewCoursesJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewCoursesJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPreviewClassesJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewClassesJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPreviewEnrollmentsJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewEnrollmentsJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPreviewAcademicSessionsJsonString(int pageNumber)
        {
            var model = await ApiCalls.GetPreviewAcademicSessionsJsonString(pageNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DownloadCsv(ExportsViewModel model)
        {
            var schools = model.SelectedSchools?.Split(',').ToList();
            //var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            //var terms = model.SelectedTerms?.Split(',').ToList();
            //var subjects = model.SelectedSubjects?.Split(',').ToList();
            //var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

            var csvUtils = new CsvMethods();
            //var bytes = await csvUtils.GetZipFile(schools, schoolYears, terms, subjects, courses, teachers, sections, model.OneRosterVersion);
            var bytes = await csvUtils.GetZipFile(schools, teachers, sections, model.OneRosterVersion);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }

        [HttpPost]
        public ActionResult UpdateTemplate(ExportsViewModel model)
        {
            var schoolIds = model.SelectedSchools?.Split(',').ToList();
            //var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            //var terms = model.SelectedTerms?.Split(',').ToList();
            //var subjects = model.SelectedSubjects?.Split(',').ToList();
            //var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                //SchoolYears = schoolYears,
                //Terms = terms,
                //Subjects = subjects,
                //Courses = courses,
                Teachers = teachers,
                Sections = sections
            };

            var inputsJson = JsonConvert.SerializeObject(inputs);

            var template = db.Templates.First(x => x.TemplateId == model.EditTemplateId);
            template.Filters = inputsJson;
            template.OneRosterVersion = model.OneRosterVersion;

            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index", "Templates");
        }

        [HttpPost]
        public ActionResult SaveTemplate(ExportsViewModel model)
        {
            var schoolIds = model.SelectedSchools?.Split(',').ToList();
            //var schoolYears = model.SelectedSchoolYears?.Split(',').ToList();
            //var terms = model.SelectedTerms?.Split(',').ToList();
            //var subjects = model.SelectedSubjects?.Split(',').ToList();
            //var courses = model.SelectedCourses?.Split(',').ToList();
            var teachers = model.SelectedTeachers?.Split(',').ToList();
            var sections = model.SelectedSections?.Split(',').ToList();

            var inputs = new FilterInputs
            {
                Schools = schoolIds,
                //SchoolYears = schoolYears,
                //Terms = terms,
                //Subjects = subjects,
                //Courses = courses,
                Teachers = teachers,
                Sections = sections
            };

            var inputsJson = JsonConvert.SerializeObject(inputs);

            var template = new Template
            {
                TemplateName = model.NewTemplateName,
                VendorName = model.NewTemplateVendorName,
                OneRosterVersion = model.OneRosterVersion,
                Filters = inputsJson
            };

            db.Templates.Add(template);
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index", "Templates");
        }

        //public async Task<ActionResult> GetSubjectsPartial(List<string> schoolIds,
        //    List<string> schoolYears,
        //    List<string> terms,
        //    List<string> boxesAlreadyChecked,
        //    bool getMore)
        //{
        //    ViewData.TemplateInfo.HtmlFieldPrefix = "Subjects";

        //    var model = await ApiCalls.GetSubjects(schoolIds, schoolYears, terms, getMore);
        //    CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

        //    return PartialView("_CriteriaSection", model);
        //}

        //public async Task<ActionResult> GetCoursesPartial(List<string> schoolIds,
        //    List<string> schoolYears,
        //    List<string> terms,
        //    List<string> boxesAlreadyChecked,
        //    bool getMore)
        //{
        //    ViewData.TemplateInfo.HtmlFieldPrefix = "Courses";

        //    var model = await ApiCalls.GetCourses(schoolIds, schoolYears, terms, getMore);
        //    CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

        //    return PartialView("_CriteriaSection", model);
        //}

        public async Task<ActionResult> GetTeachersPartial(List<string> schoolIds,
            //List<string> schoolYears,
            //List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Teachers";

            //var model = await ApiCalls.GetTeachers(schoolIds, schoolYears, terms, getMore);
            var model = await ApiCalls.GetTeachers(schoolIds, getMore);
            CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetSectionsPartial(List<string> schoolIds,
            //List<string> schoolYears,
            //List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Sections";

            //var model = await ApiCalls.GetSections(schoolIds, schoolYears, terms, getMore);
            var model = await ApiCalls.GetSections(schoolIds, getMore);
            CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        //private List<ExportsCheckbox> GetFilteredCheckboxes(List<ExportsCheckbox> allBoxes,
        //   List<string> schoolIds,
        //   List<string> schoolYears,
        //   List<string> terms,
        //   List<string> boxesAlreadyChecked)
        //{
        //    bool allSchools = schoolIds == null || schoolIds.Count() == 0;
        //    bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
        //    bool allTerms = terms == null || terms.Count() == 0;

        //    var filteredBoxes = new List<ExportsCheckbox>();
        //    filteredBoxes.AddRange(allBoxes);

        //    if (!allSchools)
        //    {
        //        filteredBoxes = filteredBoxes.Where(x =>
        //        schoolIds.Contains(x.SchoolId)).ToList();
        //    }

        //    if (!allSchoolYears)
        //    {
        //        filteredBoxes = filteredBoxes.Where(x =>
        //        schoolYears.Contains(x.SchoolYear)).ToList();
        //    }

        //    if (!allTerms)
        //    {
        //        filteredBoxes = filteredBoxes.Where(x =>
        //        terms.Contains(x.Term)).ToList();
        //    }

        //    filteredBoxes = filteredBoxes.GroupBy(x => x.Text).Select(group => group.First()).ToList();

        //    CheckSelectedBoxes(filteredBoxes, boxesAlreadyChecked);

        //    return filteredBoxes;
        //}

        private void CheckSelectedBoxes(List<ExportsCheckbox> boxes, List<string> boxesAlreadyChecked)
        {
            if (boxesAlreadyChecked != null && boxesAlreadyChecked.Count() > 0)
            {
                var boxesToCheck = boxes.Where(x => boxesAlreadyChecked.Contains(x.Id)).ToList();
                boxesToCheck.ForEach(c => c.Selected = true);
            }
        }

        private async Task InitializeModelForEdit(ExportsViewModel model, int templateId)
        {
            var template = db.Templates.First(x => x.TemplateId == templateId);

            model.EditTemplateId = templateId;
            model.EditTemplateName = template.TemplateName;
            model.OneRosterVersion = template.OneRosterVersion;

            var filters = JsonConvert.DeserializeObject<FilterInputs>(template.Filters);

            CheckSelectedBoxes(model.SchoolsCriteriaSection.FilterCheckboxes, filters.Schools);
            model.SchoolsCriteriaSection.IsExpanded = filters.Schools != null;

            if (filters.Teachers != null)
            {
                var teachersSection = await ApiCalls.GetTeachers(filters.Schools, false);
                var alreadyCheckedTeachers = await ApiCalls.GetSpecificTeachers(filters.Teachers);
                var duplicateTeachers = teachersSection.AllCheckboxes.Where(x => filters.Teachers.Contains(x.Id)).ToList();
                foreach (var box in duplicateTeachers)
                {
                    teachersSection.AllCheckboxes.Remove(box);
                }
                var duplicateTeachers2 = teachersSection.FilterCheckboxes.Where(x => filters.Teachers.Contains(x.Id)).ToList();
                foreach (var box in duplicateTeachers2)
                {
                    teachersSection.FilterCheckboxes.Remove(box);
                }
                teachersSection.AllCheckboxes.AddRange(alreadyCheckedTeachers);
                teachersSection.FilterCheckboxes.AddRange(alreadyCheckedTeachers);
                teachersSection.AllCheckboxes = teachersSection.AllCheckboxes.OrderByDescending(x => x.Selected).ToList();
                teachersSection.FilterCheckboxes = teachersSection.FilterCheckboxes.OrderByDescending(x => x.Selected).ToList();
                teachersSection.IsExpanded = alreadyCheckedTeachers != null && alreadyCheckedTeachers.Count() > 0;
                model.TeachersCriteriaSection = teachersSection;
            }

            if (filters.Schools != null)
            {
                var sectionsSection = await ApiCalls.GetSections(filters.Schools, false);
                if (filters.Sections != null)
                {
                    var alreadyCheckedSections = await ApiCalls.GetSpecificSections(filters.Sections);
                    var duplicateSections = sectionsSection.AllCheckboxes.Where(x => filters.Sections.Contains(x.Id)).ToList();
                    foreach (var box in duplicateSections)
                    {
                        sectionsSection.AllCheckboxes.Remove(box);
                    }
                    var duplicateSections2 = sectionsSection.FilterCheckboxes.Where(x => filters.Sections.Contains(x.Id)).ToList();
                    foreach (var box in duplicateSections2)
                    {
                        sectionsSection.FilterCheckboxes.Remove(box);
                    }
                    sectionsSection.AllCheckboxes.AddRange(alreadyCheckedSections);
                    sectionsSection.FilterCheckboxes.AddRange(alreadyCheckedSections);
                    sectionsSection.AllCheckboxes = sectionsSection.AllCheckboxes.OrderByDescending(x => x.Selected).ToList();
                    sectionsSection.FilterCheckboxes = sectionsSection.FilterCheckboxes.OrderByDescending(x => x.Selected).ToList();
                    sectionsSection.IsExpanded = alreadyCheckedSections != null && alreadyCheckedSections.Count() > 0;
                    model.SectionsCriteriaSection = sectionsSection;
                }
            }
        }

        private async Task InitializeModel(ExportsViewModel model)
        {
            await ApiCalls.PopulateFilterSection1(model);

            //model.SubjectsCriteriaSection = new ApiCriteriaSection
            //{
            //    SectionName = "Subjects"
            //};

            //model.CoursesCriteriaSection = new ApiCriteriaSection
            //{
            //    SectionName = "Courses"
            //};

            model.TeachersCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Teachers"
            };

            model.SectionsCriteriaSection = new ApiCriteriaSection
            {
                SectionName = "Sections"
            };

            using (var context = new ApplicationDbContext())
            {
                model.OneRosterVersion = context.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.DefaultOneRosterVersion)?.SettingValue;
            }

            ViewBag.OneRosterVersions = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Text = OneRosterVersions.OR_1_0,
                        Value = OneRosterVersions.OR_1_0
                    },
                    new SelectListItem
                    {
                        Text = OneRosterVersions.OR_1_1,
                        Value = OneRosterVersions.OR_1_1
                    }
            };

        }
    }
}