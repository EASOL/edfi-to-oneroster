using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED2OR.ViewModels
{
    public class LogsViewModel
    {
        public string LogsJson { get; set; }
    }

    public class TemplateLogViewmodel
    {
        public string TemplateName { get; set; }
        public string VendorName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime DateValue { get; set; }
        public string DateString { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
    }
}