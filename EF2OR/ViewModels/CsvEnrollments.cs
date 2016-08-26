using System.Collections.Generic;

namespace EF2OR.ViewModels
{
    public class CsvEnrollments
    {
        [OR10IncludeField]
        [OR11IncludeField]
        public string sourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string classSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string schoolSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string userSourcedId { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string role { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string status { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string dateLastModified { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string primary { get; set; }
        [OR11IncludeField]
        public string beginDate { get; set; }
        [OR11IncludeField]
        public string endDate { get; set; }

        public string SchoolId { get; set; }
        public string SchoolYear { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string Course { get; set; }
        public string Section { get; set; }
        public IEnumerable<string> Teachers { get; set; }
    }
}