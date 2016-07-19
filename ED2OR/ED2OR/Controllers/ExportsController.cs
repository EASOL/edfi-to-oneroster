﻿using System;
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

            var orgColumnNames = typeof(CsvOrgs).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            var usersColumnNames = typeof(CsvUsers).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            var coursesColumnNames = typeof(CsvCourses).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            var classesColumnNames = typeof(CsvClasses).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            var enrollmentsColumnNames = typeof(CsvEnrollments).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));
            var academicsessionsColumnNames = typeof(CsvAcademicSessions).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(CsvIgnoreFieldAttribute))).Select(x => x.Name.Replace("__", "."));

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

            var allBoxes = await ApiCalls.GetSubjects();
            model.FilterCheckboxes = GetFilteredCheckboxes(allBoxes, schoolIds, schoolYears, terms, boxesAlreadyChecked);
            
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

            var allBoxes = await ApiCalls.GetCourses();
            model.FilterCheckboxes = GetFilteredCheckboxes(allBoxes, schoolIds, schoolYears, terms, boxesAlreadyChecked);

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

            var allBoxes = await ApiCalls.GetTeachers();
            model.FilterCheckboxes = GetFilteredCheckboxes(allBoxes, schoolIds, schoolYears, terms, boxesAlreadyChecked);

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

            var allBoxes = await ApiCalls.GetSections();
            model.FilterCheckboxes = GetFilteredCheckboxes(allBoxes, schoolIds, schoolYears, terms, boxesAlreadyChecked);

            return PartialView("_CriteriaSection", model);
        }

        private List<ExportsCheckbox> GetFilteredCheckboxes(List<ExportsCheckbox> allBoxes,
           List<string> schoolIds,
           List<string> schoolYears,
           List<string> terms,
           List<string> boxesAlreadyChecked)
        {
            bool allSchools = schoolIds == null || schoolIds.Count() == 0;
            bool allSchoolYears = schoolYears == null || schoolYears.Count() == 0;
            bool allTerms = terms == null || terms.Count() == 0;

            var filteredBoxes = new List<ExportsCheckbox>();
            filteredBoxes.AddRange(allBoxes);

            if (!allSchools)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                schoolIds.Contains(x.SchoolId)).ToList();
            }

            if (!allSchoolYears)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                schoolYears.Contains(x.SchoolYear)).ToList();
            }

            if (!allTerms)
            {
                filteredBoxes = filteredBoxes.Where(x =>
                terms.Contains(x.Term)).ToList();
            }

            filteredBoxes = filteredBoxes.GroupBy(x => x.Text).Select(group => group.First()).ToList();

            CheckSelectedBoxes(filteredBoxes, boxesAlreadyChecked);

            return filteredBoxes;
        }

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
            var filtersJson = db.Templates.First(x => x.TemplateId == templateId).Filters;
            var filters = JsonConvert.DeserializeObject<FilterInputs>(filtersJson);

            CheckSelectedBoxes(model.SchoolsCriteriaSection.FilterCheckboxes, filters.Schools);
            CheckSelectedBoxes(model.SchoolYearsCriteriaSection.FilterCheckboxes, filters.SchoolYears);
            CheckSelectedBoxes(model.TermsCriteriaSection.FilterCheckboxes, filters.Terms);

            var allSubjects = await ApiCalls.GetSubjects();
            var subjects = GetFilteredCheckboxes(allSubjects, filters.Schools, filters.SchoolYears, filters.Terms, filters.Subjects);

            var allCourses = await ApiCalls.GetCourses();
            var courses = GetFilteredCheckboxes(allCourses, filters.Schools, filters.SchoolYears, filters.Terms, filters.Courses);

            var allTeachers = await ApiCalls.GetTeachers();
            var teachers = GetFilteredCheckboxes(allTeachers, filters.Schools, filters.SchoolYears, filters.Terms, filters.Teachers);

            var allSections = await ApiCalls.GetSections();
            var sections = GetFilteredCheckboxes(allSections, filters.Schools, filters.SchoolYears, filters.Terms, filters.Sections);

            model.SubjectsCriteriaSection.FilterCheckboxes = subjects;
            model.CoursesCriteriaSection.FilterCheckboxes = courses;
            model.TeachersCriteriaSection.FilterCheckboxes = teachers;
            model.SectionsCriteriaSection.FilterCheckboxes = sections;
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
        }
       
    }
}