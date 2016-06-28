namespace ED2OR.Enums
{
    public class ApiEndPoints
    {
        public const string ApiPrefix = "api/v2.0/2016/";

        public const string OauthAuthorize = "oauth/authorize";
        public const string OauthGetToken = "oauth/token";
        public const string Schools = "enrollment/schools";
        public const string Subjects = "enrollment/sections"; //acedemicSubjectDescriptor
        public const string Courses = "enrollment/sections"; //courseOfferingReference.localCourseCode
        //public const string Sections = "enrollment/schools/{0}/sections"; //uniqueSectionCode
        public const string Sections = "enrollment/sections"; //uniqueSectionCode
        public const string Teachers = "enrollment/sectionEnrollments"; //staff.firstName + staff.lastSurname
        public const string SchoolYears = "enrollment/sections"; //sessionReference.schoolYear
        public const string Terms = "enrollment/sections"; //sessionReference.termDescriptor

        public const string CsvOrgs = "enrollment/schools";
        //public const string CsvUsersStudents = "enrollment/students";
        //public const string CsvUsersStaff = "enrollment/staffs";
        public const string CsvUsers = "enrollment/sectionEnrollments";
        public const string CsvCourses = "enrollment/sectionEnrollments";
        public const string CsvClasses = "enrollment/sectionEnrollments";
        public const string CsvEnrollments = "enrollment/sectionEnrollments";
        public const string CsvAcademicSessions = "enrollment/sectionEnrollments";

    }
}