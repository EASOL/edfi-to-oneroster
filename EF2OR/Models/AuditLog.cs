using System;
using System.ComponentModel.DataAnnotations;

namespace EF2OR.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public string Type { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public int TemplateId { get; set; }
        public string Fields { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string DownloadInfo { get; set; }
    }
}