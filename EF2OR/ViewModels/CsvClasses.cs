using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class CsvClasses
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
        public string title { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string grade { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string courseSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string classCode { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string classType { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string location { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string schoolSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string termSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string subjects { get; set; }
        [OR11IncludeField]
        public string subjectCodes { get; set; }
        [OR11IncludeField]
        public string Periods { get; set; }


        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public IEnumerable<string> Teachers { get; set; }
    }
}