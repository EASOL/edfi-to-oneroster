using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ED2OR.Models;
using ED2OR.Enums;
using Newtonsoft.Json;

namespace ED2OR.Utils
{
    public class LoggingMethods
    {
        public void LogAdminDownload(Template template, string userId, string ipAddress)
        {
            using (var db = new ApplicationDbContext())
            {
                var templateInfo = JsonConvert.SerializeObject(template);

                var log = new AuditLog
                {
                    Type = ActionTypes.DownloadCsvAdmin,
                    TemplateId = template.TemplateId,
                    Success = true,
                    DownloadInfo = templateInfo,
                    DateTimeStamp = DateTime.Now,
                    User = userId,
                    IpAddress = ipAddress
                };

                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
        }

        public void LogUserDownload(Template template, string ipAddress, bool success, string reason)
        {
            using (var db = new ApplicationDbContext())
            {
                var templateInfo = JsonConvert.SerializeObject(template);

                var log = new AuditLog
                {
                    Type = ActionTypes.DownloasCsvVendor,
                    TemplateId = template.TemplateId,
                    Success = success,
                    FailureReason = reason,
                    DownloadInfo = templateInfo,
                    DateTimeStamp = DateTime.Now,
                    IpAddress = ipAddress
                };

                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
        }
    }
}