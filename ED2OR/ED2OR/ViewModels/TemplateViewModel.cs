using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED2OR.ViewModels
{
    public class TemplateViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VendorName { get; set; }
        public string AccessUrl { get; set; }
        public string AccessToken { get; set; }
        public int NumberOfDownloads { get; set; }
        public DateTime LastAccess { get; set; }
    }
}