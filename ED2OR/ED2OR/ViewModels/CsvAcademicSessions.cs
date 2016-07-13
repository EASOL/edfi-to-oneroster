using System.Collections.Generic;

namespace ED2OR.ViewModels
{
    public class CsvAcademicSessions
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string parentSourcedId { get; set; }

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