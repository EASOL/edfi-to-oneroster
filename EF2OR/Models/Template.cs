using System.ComponentModel.DataAnnotations;

namespace EF2OR.Models
{
    public class Template
    {
        [Key]
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VendorName { get; set; }
        public string AccessUrl { get; set; }
        public string AccessToken { get; set; }
        public string Filters { get; set; }
        public string OneRosterVersion { get; set; }
        public string DownloadType { get; set; }
    }
}