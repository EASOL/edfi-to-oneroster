using System.Collections.Generic;

namespace ED2OR.ViewModels
{
    public class CsvCourses
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string schoolYearId { get; set; }
        public string metadata__duration { get; set; } //double underscore replaced with a dot for the csv
        public string title { get; set; }
        public string courseCode { get; set; }
        public string grade { get; set; }
        public string orgSourcedId { get; set; }
        public string subjects { get; set; }


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