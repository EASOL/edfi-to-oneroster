namespace ED2OR.ViewModels
{
    public class CsvUsers
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string orgSourcedIds { get; set; }
        public string role { get; set; }
        public string username { get; set; }
        public string userId { get; set; }
        public string givenName { get; set; }
        public string familyName { get; set; }
        public string identifier { get; set; }
        public string email { get; set; }
        public string sms { get; set; }
        public string phone { get; set; }

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