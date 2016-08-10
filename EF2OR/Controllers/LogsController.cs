using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EF2OR.ViewModels;
using Newtonsoft.Json;
using EF2OR.Enums;

namespace EF2OR.Controllers
{
    public class LogsController : BaseController
    {
        public ActionResult Index(int? templateId = null)
        {
            var templateName = "";
            var logs = (from a in db.AuditLogs
                        from t in db.Templates.Where(x => x.TemplateId == a.TemplateId).DefaultIfEmpty()
                        let mostRecentOldValues = db.AuditLogs.Where(x => x.TemplateId == a.TemplateId).OrderByDescending(x => x.DateTimeStamp).FirstOrDefault().OldValues
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
                           SuccessString = a.Success ? "Yes" : "No",
                           FailureReason = a.FailureReason,
                           IpAddress = a.IpAddress,
                           MostRecentOldValues = mostRecentOldValues
                       }).OrderByDescending(x => x.DateValue).ToList();

            foreach (var log in logs)
            {
                if (string.IsNullOrEmpty(log.TemplateName))
                {
                    if (log.MostRecentOldValues != null)
                    {
                        var mostRecentOldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(log.MostRecentOldValues);
                        if (mostRecentOldValues.ContainsKey("TemplateName"))
                        {
                            log.TemplateName = mostRecentOldValues["TemplateName"].ToString();
                            log.VendorName = mostRecentOldValues["VendorName"].ToString();
                            templateName = mostRecentOldValues["TemplateName"].ToString(); //This is the template name for the model. It will always be the same if templateId is provided
                        }
                    }
                }
                else
                {
                    templateName = log.TemplateName; //This is the template name for the model. It will always be the same if templateId is provided
                }

                if (log.Action == ActionTypes.DownloadCsvAdmin || log.Action == ActionTypes.DownloasCsvVendor)
                {
                    if (!log.Success)
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
                else if (log.Action == ActionTypes.TemplateModified || log.Action == ActionTypes.SettingsModified)
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
                else if (log.Action == ActionTypes.LogIn || log.Action == ActionTypes.LogOut)
                {
                    if (!log.Success)
                    {
                        log.Description = "Failure: " + log.FailureReason;
                    }
                }
            }

            var model = new LogsViewModel
            {
                LogsJson = JsonConvert.SerializeObject(logs),
                TemplateId = templateId,
                TemplateName = templateName
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