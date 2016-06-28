namespace ED2OR.ViewModels
{
    public class CsvCourses
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string schoolYearId { get; set; }
        public string title { get; set; }
        public string courseCode { get; set; }
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
    }
}