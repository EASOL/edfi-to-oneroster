namespace EF2OR.ViewModels
{
    public class CsvOrgs
    {
        public string sourcedId { get; set; }
        public string status { get; set; }
        public string dateLastModified { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string identifier { get; set; }
        public string metadata__classification { get; set; } //double underscore replaced with dot for csv output
        public string metadata__gender { get; set; } //double underscore replaced with dot for csv output
        public string metadata__boarding { get; set; } //double underscore replaced with dot for csv output
        public string parentSourcedId { get; set; }

        [CsvIgnoreField]
        public string SchoolId { get; set; }
    }
}