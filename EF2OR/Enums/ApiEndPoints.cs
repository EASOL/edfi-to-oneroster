namespace EF2OR.Enums
{
    public class ApiEndPoints
    {
        public const string OauthAuthorize = "oauth/authorize";
        public const string OauthGetToken = "oauth/token";
        public const string Schools = "enrollment/schools";
        public const string Subjects = "enrollment/sections";
        public const string Courses = "enrollment/sections";
        public const string Sections = "enrollment/sections";
        public const string SectionsWithSchoolId = "enrollment/schools/{0}/sections";
        public const string Staff = "enrollment/staffs";
        public const string StaffWithSchoolId = "enrollment/schools/{0}/staffs";
        public const string SchoolYears = "enrollment/sections";
        public const string Terms = "enrollment/sections";
        public const string TermDescriptors = "termDescriptors";

        public const string CsvOrgs = "enrollment/schools";
        public const string CsvUsersStudents = "enrollment/students";
        public const string CsvUsersStaff = "enrollment/staffs";
        public const string CsvUsers = "enrollment/sections";
        public const string CsvCourses = "enrollment/sections";
        public const string CsvClasses = "enrollment/sections";
        public const string CsvEnrollments = "enrollment/sections";
        public const string CsvAcademicSessions = "enrollment/sections";
    }
}