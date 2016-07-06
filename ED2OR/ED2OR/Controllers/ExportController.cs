using ED2OR.Utils;
using ED2OR.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ED2OR.Controllers
{
    public class ExportController : BaseController
    {
        [AllowAnonymous]
        public async Task<FileResult> Index(string id)  //Careful when changing this method definition.  There is a custom route for it in the RouteConfig.
        {
            string ip = Request.UserHostAddress;
            var logUtils = new LoggingMethods();
            var template = db.Templates.First(x => x.AccessUrl.Contains(id));

            if (Request.Headers["token"] == null) //http://forums.asp.net/t/1991328.aspx?Reading+HTTP+Header+in+MVC+5
            {
                logUtils.LogUserDownload(template, ip, false, "No Security Token Received");
                return null;
            }

            string token = Request.Headers["token"];

            if (template == null || template.AccessToken != token)
            {
                logUtils.LogUserDownload(template, ip, false, "Url not found and/or token doesnt match");
                return null;
            }

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

            logUtils.LogUserDownload(template, ip, true, null);

            var downloadFileName = "EdFiExport_" + string.Format("{0:MM_dd_yyyy}", DateTime.Now) + ".zip";
            return File(bytes, "application/zip", downloadFileName);
        }
    }
}