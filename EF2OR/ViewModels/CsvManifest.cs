namespace EF2OR.ViewModels
{
    public class CsvManifest
    {
        [OR11IncludeField]
        public string propertyName { get; set; }
        [OR11IncludeField]
        public string value { get; set; }
    }
}