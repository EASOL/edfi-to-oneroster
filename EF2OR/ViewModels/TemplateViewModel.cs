using System;

namespace EF2OR.ViewModels
{
    public class TemplateViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VendorName { get; set; }
        public string OneRosterVersion { get; set; }
        public string AccessUrl { get; set; }
        public string AccessToken { get; set; }
        public int NumberOfDownloads { get; set; }
        public string LastAccess { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedDateString { get; set; }
        public string LastModifiedDate { get; set; }
    }
}