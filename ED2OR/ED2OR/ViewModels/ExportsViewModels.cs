using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED2OR.ViewModels
{
    public class ExportsViewModel
    {
        //public List<ApiCriteriaSection> CriteriaSections { get; set; }
        public ApiCriteriaSection SchoolsCriteriaSection { get; set; }
        public ApiCriteriaSection SchoolYearsCriteriaSection { get; set; }
        public ApiCriteriaSection TermsCriteriaSection { get; set; }
        public ApiCriteriaSection SubjectsCriteriaSection { get; set; }
        public ApiCriteriaSection CoursesCriteriaSection { get; set; }
        public ApiCriteriaSection TeachersCriteriaSection { get; set; }
        public ApiCriteriaSection SectionsCriteriaSection { get; set; }
        public List<DataPreviewSection> DataPreviewSections { get; set; }
        public PreveiwJsonResults JsonPreviews { get; set; }
    }

    public class PreveiwJsonResults
    {
        public string Orgs { get; set; }
        public string Users { get; set; }
        public string Courses { get; set; }
        public string Classes { get; set; }
        public string Enrollments { get; set; }
        public string AcademicSessions { get; set; }
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
        //public bool IsFilled { get; set; }
        //public int Level { get; set; }
        public bool IsExpanded { get; set; }

    }

    public class ExportsCheckbox
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
        public bool Visible { get; set; }
        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
    }
}