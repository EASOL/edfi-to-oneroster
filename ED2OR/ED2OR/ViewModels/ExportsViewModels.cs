using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED2OR.ViewModels
{
    public class ExportsViewModel
    {
        //public List<ExportsCheckbox> SchoolsCheckboxes { get; set; }
        //public List<ExportsCheckbox> SubjectsCheckboxes { get; set; }
        //public List<ExportsCheckbox> CoursesCheckboxes { get; set; }
        //public List<ExportsCheckbox> SectionsCheckboxes { get; set; }
        //public List<ExportsCheckbox> TeachersCheckboxes { get; set; }
        public List<ApiCriteriaSection> CriteriaSections { get; set; }
        public List<DataPreviewSection> DataPreviewSections { get; set; }
        public List<CsvOrgs> OrgsResult { get; set; }
        public List<CsvUsers> UsersResult { get; set; }
        public List<CsvCourses> CoursesResult { get; set; }
        public List<CsvClasses> ClassesResult { get; set; }
        public List<CsvEnrollments> EnrollmentsResult { get; set; }
        public List<CsvAcademicSessions> AcedemicSessionsResult { get; set; }
        //public ApiCriteriaSection SubjectsCriteriaSection { get; set; }
    }

    public class DataPreviewSection
    {
        public string SectionName { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
    }

    public class ApiCriteriaSection
    {
        public List<ExportsCheckbox> FilterCheckboxes { get; set; }
        public string SectionName { get; set; }
        public bool IsFilled { get; set; }
        public int Level { get; set; }
        public bool IsExpanded { get; set; }

    }

    public class ExportsCheckbox
    {
        public string Id { get; set; }
        public string SchoolId { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
        public bool Visible { get; set; }
    }
}