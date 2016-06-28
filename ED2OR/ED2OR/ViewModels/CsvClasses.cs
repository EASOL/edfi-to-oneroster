namespace ED2OR.ViewModels
{
    public class CsvClasses
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string title { get; set; }
        public string courseSourcedId { get; set; }
        public string classCode { get; set; }
        public string classType { get; set; }
        public string schoolSourcedId { get; set; }
        public string termSourcedId { get; set; }
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