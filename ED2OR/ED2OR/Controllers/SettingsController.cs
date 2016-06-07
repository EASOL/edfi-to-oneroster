using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ED2OR.Controllers
{
    public class SettingsController : BaseController
    {
        private ApplicationUserManager _userManager;

        public SettingsController()
        {
        }

        public SettingsController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            var user = UserManager.FindById(UserId);

            var model = new SettingsViewModel
            {
                ApiBaseUrl = user.ApiBaseUrl,
                ApiKey = user.ApiKey,
                ApiSecret = user.ApiSecret
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult TestConnection(string apiBaseUrl, string apiKey, string apiSecret)
        {
            var tokenResult = GetToken(apiBaseUrl, apiKey, apiSecret);
            return Json(tokenResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.OldPassword))
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "The new password and confirmation password do not match");
                    return View(model);
                }
                var result = UserManager.ChangePassword(UserId, model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.ToList()[0]);
                    return View(model);
                }
            }

            var user = db.Users.FirstOrDefault(x => x.Id == UserId);
            user.ApiBaseUrl = model.ApiBaseUrl;
            user.ApiKey = model.ApiKey;
            user.ApiSecret = model.ApiSecret;
            db.SaveChanges();

            model.OldPassword = "";
            model.NewPassword = "";
            model.ConfirmPassword = "";

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}