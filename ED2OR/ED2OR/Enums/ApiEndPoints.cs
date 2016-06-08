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
        public const string Sections = "enrollment/schools/{0}/sections"; //uniqueSectionCode
        //public const string Sections = "enrollment/sections"; //uniqueSectionCode
        public const string Teachers = "enrollment/sectionEnrollments"; //staff.firstName + staff.lastSurname
        public const string SchoolYear = "enrollment/sections"; //sessionReference.schoolYear
    }
}