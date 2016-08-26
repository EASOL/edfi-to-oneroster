using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class CsvCourses
    {
        [OR10IncludeField]
        [OR11IncludeField]
        public string sourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string status { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string dateLastModified { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string schoolYearId { get; set; }
        [OR10IncludeField]
        public string metadata__duration { get; set; } //double underscore replaced with a dot for the csv
        [OR10IncludeField]
        [OR11IncludeField]
        public string title { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string courseCode { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string grade { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string orgSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string subjects { get; set; }
        [OR11IncludeField]
        public string subjectCodes { get; set; }


        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public IEnumerable<string> Teachers { get; set; }
    }
}