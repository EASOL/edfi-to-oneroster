using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class ExportsViewModel
    {
        public ApiCriteriaSection SchoolsCriteriaSection { get; set; }
        //public ApiCriteriaSection SchoolYearsCriteriaSection { get; set; }
        //public ApiCriteriaSection TermsCriteriaSection { get; set; }
        //public ApiCriteriaSection SubjectsCriteriaSection { get; set; }
        //public ApiCriteriaSection CoursesCriteriaSection { get; set; }
        public ApiCriteriaSection TeachersCriteriaSection { get; set; }
        public ApiCriteriaSection SectionsCriteriaSection { get; set; }
        public List<DataPreviewSection> DataPreviewSections { get; set; }
        public PreveiwJsonResults JsonPreviews { get; set; }
        public string SelectedSchools { get; set; }
        public string SelectedSchoolYears { get; set; }
        public string SelectedTerms { get; set; }
        //public string SelectedSubjects { get; set; }
        //public string SelectedCourses { get; set; }
        public string SelectedTeachers { get; set; }
        public string SelectedSections { get; set; }
        public string NewTemplateName { get; set; }
        public string NewTemplateVendorName { get; set; }
        public string OneRosterVersion { get; set; }

        public int EditTemplateId { get; set; }
        public string EditTemplateName { get; set; }
    }

    public class PreveiwJsonResults
    {
        public string Orgs { get; set; }
        public string Users { get; set; }
        public string Courses { get; set; }
        public string Classes { get; set; }
        public string Enrollments { get; set; }
        public string AcademicSessions { get; set; }
        public string Manifest { get; set; }


        public int OrgsTotalPages { get; set; }
        public int UsersTotalPages { get; set; }
        public int CoursesTotalPages { get; set; }
        public int ClassesTotalPages { get; set; }
        public int EnrollmentsTotalPages { get; set; }
        public int AcademicSessionsTotalPages { get; set; }
    }

    public class DataPreviewSection
    {
        public string SectionName { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
    }

    public class DataPreviewPagedJsonModel
    {
        public string JsonData { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class DataResults
    {
        public List<CsvOrgs> Orgs { get; set; }
        public List<CsvUsers> Users { get; set; }
        public List<CsvCourses> Courses { get; set; }
        public List<CsvClasses> Classes { get; set; }
        public List<CsvEnrollments> Enrollments { get; set; }
        public List<CsvAcademicSessions> AcademicSessions { get; set; }
        public List<CsvDemographics> Demographics { get; set; }
        public List<CsvManifest> Manifest { get; set; }
    }

    public class FilterInputs
    {
        public List<string> Schools { get; set; }
        public List<string> SchoolYears { get; set; }
        public List<string> Terms { get; set; }
        //public List<string> Subjects { get; set; }
        //public List<string> Courses { get; set; }
        public List<string> Teachers { get; set; }
        public List<string> Sections { get; set; }
    }

    public class ApiCriteriaSection
    {
        public List<ExportsCheckbox> AllCheckboxes { get; set; }
        public List<ExportsCheckbox> FilterCheckboxes { get; set; }
        public string SectionName { get; set; }
        public bool IsExpanded { get; set; }
        public int CurrentOffset { get; set; }
        public string CurrentSchoolId { get; set; }
        public bool CurrentSchoolAllDataReceived { get; set; }
        public Dictionary<int, string> SchoolIds { get; set; }
        public int NumCheckBoxesToDisplay { get; set; }
        public bool AllDataReceived { get; set; }
        public bool CanGetMore { get; set; }
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
        //public string Subject { get; set; }
        //public string Course { get; set; }
        public string Teacher { get; set; }
    }
}