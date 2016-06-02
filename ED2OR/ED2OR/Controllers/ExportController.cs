using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ED2OR.Controllers
{
    public class ExportController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index(string id)
        {
            return View();
        }
    }
}