namespace ED2OR.ViewModels
{
    public class CsvOrgs
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string identifier { get; set; }
        public string parentSourcedId { get; set; }

        [CsvIgnoreField]
        public string SchoolId { get; set; }
    }
}