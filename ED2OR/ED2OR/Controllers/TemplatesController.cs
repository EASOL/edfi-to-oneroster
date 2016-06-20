using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using ED2OR.Models;
using ED2OR.Enums;

namespace ED2OR.Controllers
{
    public class TemplatesController : BaseController
    {
        public const int NUMBER_OF_DOWNLOADS = 3;
        public ActionResult Index()
        {
            var model = (from t in db.Templates
                        select new TemplateViewModel
                        {
                            TemplateId = t.TemplateId,
                            TemplateName = t.TemplateName,
                            VendorName = t.VendorName,
                            AccessUrl = t.AccessUrl,
                            AccessToken = t.AccessToken,
                            NumberOfDownloads = NUMBER_OF_DOWNLOADS,
                            LastAccess = DateTime.Now
                        }).ToList();
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
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int templateId, string templateName, string vendorName)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            template.TemplateName = templateName;
            template.VendorName = vendorName;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Clone(int templateId, string templateName, string vendorName)
        {
            var template = new Template
            {
                TemplateName = templateName,
                VendorName = vendorName
            };
            db.Templates.Add(template);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int templateId)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            db.Templates.Remove(template);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}