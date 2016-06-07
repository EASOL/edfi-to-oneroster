using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ED2OR.Controllers
{
    public class ExportController : BaseController
    {
        [AllowAnonymous]
        public string Index(string id)  //Careful when changing this method definition.  There is a custom route for it in the RouteConfig.  This will actually return a FileResult in the form of a zip of CSVs
        {
            //string id = "1717b649-e11b-42d7-89df-b1d1ed633040"; //url
            //string token = "56693283-3243-42a0-a60e-32ec3ebb6b3e"; //this is taken in as a header
            if (Request.Headers["token"] == null) //http://forums.asp.net/t/1991328.aspx?Reading+HTTP+Header+in+MVC+5
            {
                return "No Security Token Received";
            }

            string token = Request.Headers["token"];

            var template = db.Templates.First(x => x.AccessUrl.Contains(id));
            if (template == null || template.AccessToken != token)
            {
                return "Url not found and/or token doesnt match";
            }

            GetFileResult(template.TemplateId);

            //write to audit log ip=string ip = Request.UserHostAddress;

            return "You did it!";
        }
    }
}