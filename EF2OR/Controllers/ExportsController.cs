﻿using System;
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
                filters.SchoolYears,
                filters.Terms,
                filters.Subjects,
                filters.Courses,
                filters.Teachers,
                filters.Sections,
                template.OneRosterVersion);
        }

        [HttpPost]
        public async Task<ActionResult> Preview(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> subjects,
            List<string> courses,
            List<string> teachers,
            List<string> sections,
            string oneRosterVersion)
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
            var bytes = await csvUtils.GetZipFile(schools, schoolYears, terms, subjects, courses, teachers, sections, model.OneRosterVersion);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }

        [HttpPost]
        public ActionResult UpdateTemplate(ExportsViewModel model)
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
                OneRosterVersion = model.OneRosterVersion,
                Filters = inputsJson
            };

            db.Templates.Add(template);
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index", "Templates");
        }

        public async Task<ActionResult> GetSubjectsPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Subjects";

            var model = await ApiCalls.GetSubjects(schoolIds, schoolYears, terms, getMore);
            CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetCoursesPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Courses";

            var model = await ApiCalls.GetCourses(schoolIds, schoolYears, terms, getMore);
            CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetTeachersPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Teachers";

            var model = await ApiCalls.GetTeachers(schoolIds, schoolYears, terms, getMore);
            CheckSelectedBoxes(model.FilterCheckboxes, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        public async Task<ActionResult> GetSectionsPartial(List<string> schoolIds,
            List<string> schoolYears,
            List<string> terms,
            List<string> boxesAlreadyChecked,
            bool getMore)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = "Sections";

            var model = await ApiCalls.GetSections(schoolIds, schoolYears, terms, getMore);
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
            //var template = db.Templates.First(x => x.TemplateId == templateId);

            //model.EditTemplateId = templateId;
            //model.EditTemplateName = template.TemplateName;
            //model.OneRosterVersion = template.OneRosterVersion;

            //var filters = JsonConvert.DeserializeObject<FilterInputs>(template.Filters);

            //CheckSelectedBoxes(model.SchoolsCriteriaSection.FilterCheckboxes, filters.Schools);
            //model.SchoolsCriteriaSection.IsExpanded = filters.Schools != null;

            //CheckSelectedBoxes(model.SchoolYearsCriteriaSection.FilterCheckboxes, filters.SchoolYears);
            //model.SchoolYearsCriteriaSection.IsExpanded = filters.SchoolYears != null;

            //CheckSelectedBoxes(model.TermsCriteriaSection.FilterCheckboxes, filters.Terms);
            //model.TermsCriteriaSection.IsExpanded = filters.Terms != null;

            //var allSubjects = await ApiCalls.GetSubjects();
            //var subjects = GetFilteredCheckboxes(allSubjects, filters.Schools, filters.SchoolYears, filters.Terms, filters.Subjects);
            //model.SubjectsCriteriaSection.FilterCheckboxes = subjects;
            //model.SubjectsCriteriaSection.IsExpanded = filters.Subjects != null;

            //var allCourses = await ApiCalls.GetCourses();
            //var courses = GetFilteredCheckboxes(allCourses, filters.Schools, filters.SchoolYears, filters.Terms, filters.Courses);
            //model.CoursesCriteriaSection.FilterCheckboxes = courses;
            //model.CoursesCriteriaSection.IsExpanded = filters.Courses != null;

            //var allTeachers = await ApiCalls.GetTeachers();
            //var teachers = GetFilteredCheckboxes(allTeachers, filters.Schools, filters.SchoolYears, filters.Terms, filters.Teachers);
            //model.TeachersCriteriaSection.FilterCheckboxes = teachers;
            //model.TeachersCriteriaSection.IsExpanded = filters.Teachers != null;

            //var allSections = await ApiCalls.GetSections();
            //var sections = GetFilteredCheckboxes(allSections, filters.Schools, filters.SchoolYears, filters.Terms, filters.Sections);
            //model.SectionsCriteriaSection.FilterCheckboxes = sections;
            //model.SectionsCriteriaSection.IsExpanded = filters.Sections != null;
        }

        private async Task InitializeModel(ExportsViewModel model)
        {
            await ApiCalls.PopulateFilterSection1(model);

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