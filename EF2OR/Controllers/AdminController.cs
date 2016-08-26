using EF2OR.Utils;
using System.Text;
using System.Web.Mvc;

namespace EF2OR.Controllers
{
    public class PreConfigurationAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Utils.PreConfigurationHelper.IsInitialSetup(filterContext.HttpContext))
            {
                filterContext.Result = new RedirectToRouteResult("InitialSetupRoute", null);
            }
            else
                base.OnActionExecuting(filterContext);
        }
    }
    public class AdminController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        public ActionResult InitialSetup(ViewModels.InitialSetup model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                var adminUser = PreConfigurationHelper.GetLocalAdminInfo(HttpContext);
                if (adminUser != null)
                {

                    string errors = string.Empty;
                    if (SaveConnectionString(model, out errors))
                    {
                        adminUser.InitialDatabaseConfigured = true;
                        PreConfigurationHelper.SaveAdminUser(adminUser, HttpContext);
                        return RedirectToAction(actionName: "Login", controllerName: "Account");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(errors))
                        ViewBag.Error = errors;
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Error = "There is no admin user in the local database, please create it";
                    return View(model);
                }
            }
        }


        [HttpGet]
        [AllowAnonymous]
        // GET: Admin
        public ActionResult InitialSetup()
        {
            if (Utils.PreConfigurationHelper.IsInitialSetup(HttpContext))
            {
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendLine("Please set the initial configuration to access the database");
                var adminUser = PreConfigurationHelper.GetLocalAdminInfo(HttpContext);
                {
                    if (adminUser == null)
                    {
                        adminUser = new LocalAdminInfo();
                        adminUser.Username = "admin";
                    }
                    PreConfigurationHelper.SaveAdminUser(adminUser, HttpContext);
                }
                ViewBag.Message = strMessage.ToString();
            }
            else
            {
                if (!HttpContext.User.Identity
                    .IsAuthenticated)
                {
                    var routeUrl = Url.RouteUrl("InitialSetupRoute");
                    return RedirectToAction(controllerName: "Account", actionName: "Login", routeValues: new { ReturnUrl = routeUrl });
                }
            }
            ViewModels.InitialSetup model = new ViewModels.InitialSetup();
            model.IntegratedSecuritySSPI = true;
            return View(model);
        }
    }
}