using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using Newtonsoft.Json;
using ED2OR.Enums;
using Newtonsoft.Json.Linq;

namespace ED2OR.Controllers
{
    public class LogsController : BaseController
    {
        public ActionResult Index(int? templateId = null)
        {
            var logs = (from a in db.AuditLogs
                       join t in db.Templates on a.TemplateId equals t.TemplateId
                       where templateId == null || t.TemplateId == templateId
                       select new TemplateLogViewmodel
                       {
                           TemplateName = t.TemplateName,
                           VendorName = t.VendorName,
                           Action = a.Type,
                           DateValue = a.DateTimeStamp,
                           DateString = a.DateTimeStamp.ToString(),
                           OldValues = a.OldValues,
                           NewValues = a.NewValues,
                           Success = a.Success,
                           FailureReason = a.FailureReason
                       }).OrderByDescending(x => x.DateValue).ToList();


            foreach (var log in logs)
            {
                if (log.Action == ActionTypes.DownloadCsvAdmin || log.Action == ActionTypes.DownloasCsvVendor)
                {
                    if (log.Success)
                    {
                        log.Description = "Sucess";
                    }
                    else
                    {
                        log.Description = "Failure: " + log.FailureReason;
                    }
                }
                else if (log.Action == ActionTypes.TemplateCreated)
                {
                    log.Description = "<b>Initial Values:</b><br /><ul>";
                    string lineItemFormat = "<li><b>{0}</b> created as <b>{1}</b></li>";
                    var newValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(log.NewValues);
                    foreach (KeyValuePair<string, object> entry in newValues)
                    {
                        var newValue = GetListItemHtml(entry.Key, entry.Value);
                        log.Description += String.Format(lineItemFormat, entry.Key, newValue);
                    }
                    log.Description += "</ul>";
                }
                else if (log.Action == ActionTypes.TemplateModified)
                {
                    string lineItemFormat = "<li><b>{0}</b> was changed from <b>{1}</b> to <b>{2}</b></li>";
                    var oldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(log.OldValues);
                    var newValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(log.NewValues);
                    log.Description = "<ul>";

                    foreach (KeyValuePair<string, object> entry in oldValues)
                    {
                        var oldValue = GetListItemHtml(entry.Key, entry.Value);
                        var newValue = GetListItemHtml(entry.Key, newValues[entry.Key]);
                        log.Description += String.Format(lineItemFormat, entry.Key, oldValue, newValue);
                    }
                    log.Description += "</ul>";
                }
                else if (log.Action == ActionTypes.TemplateDeleted)
                {
                    log.Description = "<b>Previous Values:</b><br /><ul>";
                    string lineItemFormat = "<li><b>{0}</b> was <b>{1}</b></li>";
                    var oldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(log.OldValues);
                    foreach (KeyValuePair<string, object> entry in oldValues)
                    {
                        var oldValue = GetListItemHtml(entry.Key, entry.Value);
                        log.Description += String.Format(lineItemFormat, entry.Key, oldValue);
                    }
                    log.Description += "</ul>";
                }
            }

            var model = new LogsViewModel
            {
                LogsJson = JsonConvert.SerializeObject(logs)
            };

            return View(model);
        }

        private string GetListItemHtml(string key, object value)
        {
            var returnText = "";
            if (key == "Filters")
            {
                returnText += "<ul>";
                var deserializedFilters = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                foreach (var filter in deserializedFilters)
                {
                    if (filter.Value != null)
                    {
                        returnText += "<li>" + filter.Key + ": ";
                        returnText += filter.Value + "</li>";
                    }
                }
                returnText += "</ul>";
            }
            else
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    returnText = value.ToString();
                }
                else
                {
                    returnText = "[no value]";
                }
            }
            return returnText;
        }
    }
}