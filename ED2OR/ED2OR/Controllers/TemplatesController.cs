using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;

namespace ED2OR.Controllers
{
    public class TemplatesController : BaseController
    {
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
                            NumberOfDownloads = 3,
                            LastAccess = DateTime.Now
                        }).ToList();
            return View(model);
        }

        public ActionResult AssignToken(int templateId)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            template.AccessToken = Guid.NewGuid().ToString();
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int templateId, string templateName, string vendorName)
        {
            var template = db.Templates.FirstOrDefault(x => x.TemplateId == templateId);
            template.TemplateName = templateName;
            template.VendorName = vendorName;

            return RedirectToAction("Index");
        }


    }
}