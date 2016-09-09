using EF2OR.Utils;
using EF2OR.ViewModels;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EF2OR.Controllers
{
    public class ExportController : BaseController
    {
        [AllowAnonymous]
        public async Task<ActionResult> Index(string id)  //Careful when changing this method definition.  There is a custom route for it in the RouteConfig.
        {
            var logUtils = new LoggingMethods();
            var template = db.Templates.First(x => x.AccessUrl.Contains(id));

            if (Request.Headers["token"] == null) //http://forums.asp.net/t/1991328.aspx?Reading+HTTP+Header+in+MVC+5
            {
                var errorMsg = "No Security Token Received";
                logUtils.LogUserDownload(template, IpAddress, false, errorMsg);
                return Json(new { ErrorMessage = errorMsg }, JsonRequestBehavior.AllowGet);
            }

            string token = Request.Headers["token"];

            if (template == null || template.AccessToken != token)
            {
                var errorMsg = "Url not found and/or token doesnt match";
                logUtils.LogUserDownload(template, IpAddress, false, errorMsg);
                return Json(new { ErrorMessage = errorMsg }, JsonRequestBehavior.AllowGet);
            }

            var filters = JsonConvert.DeserializeObject<FilterInputs>(template.Filters);
            var csvUtils = new CsvMethods();
            var bytes = await csvUtils.GetZipFile(
                filters.Schools,
                //filters.SchoolYears,
                //filters.Terms,
                //filters.Subjects,
                //filters.Courses,
                filters.Teachers,
                filters.Sections,
                template.OneRosterVersion);

            logUtils.LogUserDownload(template, IpAddress, true, null);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }
    }
}