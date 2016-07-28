using System;

namespace EF2OR.ViewModels
{
    public class LogsViewModel
    {
        public string LogsJson { get; set; }
        public int? TemplateId { get; set; }
        public string TemplateName { get; set; }
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
        public string SuccessString { get; set; }
        public string FailureReason { get; set; }
        public string IpAddress { get; set; }
        public string MostRecentOldValues { get; set; }
    }
}