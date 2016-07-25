using System;
using EF2OR.Models;
using EF2OR.Enums;
using Newtonsoft.Json;

namespace EF2OR.Utils
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

        public void LogUserLogin(string userId, string ipAddress, bool success, string reason)
        {
            using (var db = new ApplicationDbContext())
            {
                var log = new AuditLog
                {
                    Type = ActionTypes.LogIn,
                    Success = success,
                    DateTimeStamp = DateTime.Now,
                    User = userId,
                    FailureReason = reason,
                    IpAddress = ipAddress
                };

                db.AuditLogs.Add(log);
                db.SaveChanges();
            }
        }

        public void LogUserLogout(string userId, string ipAddress)
        {
            using (var db = new ApplicationDbContext())
            {
                var log = new AuditLog
                {
                    Type = ActionTypes.LogOut,
                    Success = true,
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