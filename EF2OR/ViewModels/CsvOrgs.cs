namespace EF2OR.ViewModels
{
    public class CsvOrgs
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
        public string name { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string type { get; set; }
        [OR10IncludeField]
        [OR11IncludeField]
        public string identifier { get; set; }
        [OR10IncludeField]
        public string metadata__classification { get; set; } //double underscore replaced with dot for csv output
        [OR10IncludeField]
        public string metadata__gender { get; set; } //double underscore replaced with dot for csv output
        [OR10IncludeField]
        public string metadata__boarding { get; set; } //double underscore replaced with dot for csv output
        [OR10IncludeField]
        [OR11IncludeField]
        public string parentSourcedId { get; set; }

        public string SchoolId { get; set; }
    }
}