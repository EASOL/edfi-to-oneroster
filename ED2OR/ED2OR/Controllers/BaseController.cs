using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.Models;
using Microsoft.AspNet.Identity;
using ED2OR.ViewModels;
using ED2OR.Utils;

namespace ED2OR.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext db = new ApplicationDbContext();

        protected string UserId
        {
            get
            {
                return User.Identity.GetUserId();
            }
        }

    }
}