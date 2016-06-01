using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ED2OR.Models
{
    public class Template
    {
        [Key]
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VendorName { get; set; }
        public string AccessUrl { get; set; }
        public string AccessToken { get; set; }
        //public DateTime CreatedTimeStamp { get; set; }
        //public DateTime LastUpdatedTimeStamp { get; set; }
    }
}