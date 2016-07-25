﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EF2OR.ViewModels;
using EF2OR.Models;
using EF2OR.Enums;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EF2OR.Utils;

namespace EF2OR.Controllers
{
    public class TemplatesController : BaseController
    {
        public ActionResult Index()
        {
            var downloadTypes = new List<string> { ActionTypes.DownloadCsvAdmin, ActionTypes.DownloasCsvVendor };
            var model = (from t in db.Templates
                         let logs = db.AuditLogs.Where(x => x.TemplateId == t.TemplateId)
                         let createdLog = logs.FirstOrDefault(x => x.Type == ActionTypes.TemplateCreated)
                         let createdTime = createdLog == null ? "" : createdLog.DateTimeStamp.ToString()

                         let modifiedLogs = logs.Where(x => x.Type == ActionTypes.TemplateModified)
                         let lastModified = modifiedLogs.Count() == 0 ? "" : modifiedLogs.Max(x => x.DateTimeStamp).ToString()

                         let downloads = db.AuditLogs.Where(x => x.TemplateId == t.TemplateId && downloadTypes.Contains(x.Type) && x.Success)
                         let numDownloads = downloads.Count()
                         let lastAccess = numDownloads == 0 ? "" : downloads.Max(x => x.DateTimeStamp).ToString()
                         select new TemplateViewModel
                        {
                            TemplateId = t.TemplateId,
                            TemplateName = t.TemplateName,
                            VendorName = t.VendorName,
                            AccessUrl = t.AccessUrl,
                            AccessToken = t.AccessToken,
                            NumberOfDownloads = numDownloads,
                            LastAccess = lastAccess,
                            LastModifiedDate = lastModified,
                            CreatedDate = createdTime
                         }).OrderByDescending(x => x.CreatedDate).ToList();
            return View(model);
        }

        public ActionResult AssignToken(int templateId)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            if (string.IsNullOrEmpty(template.AccessUrl))
            {
                template.AccessUrl = Guid.NewGuid().ToString();
            }
            template.AccessToken = Guid.NewGuid().ToString();
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int templateId, string templateName, string vendorName)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            template.TemplateName = templateName;
            template.VendorName = vendorName;
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index");
        }

        public ActionResult Clone(int templateId, string templateName, string vendorName)
        {
            var existingTemplate = db.Templates.First(x => x.TemplateId == templateId);
            var newTemplate = new Template
            {
                TemplateName = templateName,
                VendorName = vendorName,
                Filters = existingTemplate.Filters
            };
            db.Templates.Add(newTemplate);
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index", "Exports", new { templateId = newTemplate.TemplateId });
        }

        public ActionResult Delete(int templateId)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            db.Templates.Remove(template);
            db.SaveChanges(UserName, IpAddress);

            return RedirectToAction("Index");
        }
      
        public async Task<FileResult> Download(int templateId)
        {
            var template = db.Templates.First(x => x.TemplateId == templateId);
            var filters = JsonConvert.DeserializeObject<FilterInputs>(template.Filters);

            var csvUtils = new CsvMethods();
            var bytes = await csvUtils.GetZipFile(
                filters.Schools,
                filters.SchoolYears,
                filters.Terms,
                filters.Subjects,
                filters.Courses,
                filters.Teachers,
                filters.Sections);

            var logUtils = new LoggingMethods();
            string ip = Request.UserHostAddress;
            logUtils.LogAdminDownload(template, UserName, ip);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }
    }
}