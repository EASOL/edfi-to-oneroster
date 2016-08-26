using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class CsvAcademicSessions
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
        public string type { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string startDate { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string endDate { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string parentSourcedId { get; set; }
        [OR11IncludeField]
        public string schoolYear { get; set; }

        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public IEnumerable<string> Teachers { get; set; }
    }
}