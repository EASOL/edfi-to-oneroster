using System.Collections.Generic;

namespace ED2OR.ViewModels
{
    public class CsvEnrollments
    {
        public string sourcedId { get; set; }
        public string classSourcedId { get; set; }
        public string schoolSourcedId { get; set; }
        public string userSourcedId { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string primary { get; set; }

        [CsvIgnoreField]
        public string SchoolId { get; set; }
        [CsvIgnoreField]
        public string SchoolYear { get; set; }
        [CsvIgnoreField]
        public string Term { get; set; }
        [CsvIgnoreField]
        public string Subject { get; set; }
        [CsvIgnoreField]
        public string Course { get; set; }
        [CsvIgnoreField]
        public string Section { get; set; }
        [CsvIgnoreField]
        public IEnumerable<string> Teachers { get; set; }
    }
}