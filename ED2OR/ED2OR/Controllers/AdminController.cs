using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ED2OR.Controllers
{
    public class PreConfigurationAttribute : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Utils.PreConfigurationHelper.IsInitialSetup())
            {
                filterContext.Result = new RedirectResult("/Admin/InitialSetup");
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
                using (Models.AdminDBModels.AdminDBEntities ctx = new Models.AdminDBModels.AdminDBEntities())
                {
                    var adminUser = ctx.AdminUsers.Where(p => p.Username == "admin").FirstOrDefault();
                    if (adminUser != null)
                    {
                        if (adminUser.Password == model.ApplicationAdminPassword)
                        {
                            string errors = string.Empty;
                            if (SaveConnectionString(model, out errors))
                            {
                                return RedirectToAction(actionName: "Index", controllerName: "Home");
                            }
                            else
                            {
                                ViewBag.Error = errors;
                                return View(model);
                            }
                        }
                        else
                        {
                            ViewBag.Error = "Invalid password for Admin User";
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
        }


        [HttpGet]
        [AllowAnonymous]
        // GET: Admin
        public ActionResult InitialSetup()
        {
            if (Utils.PreConfigurationHelper.IsInitialSetup())
            {
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendLine("Please set the initial configuration to access the database");
                string adminUserPassword = Guid.NewGuid().ToString();
                using (Models.AdminDBModels.AdminDBEntities ctx = new Models.AdminDBModels.AdminDBEntities())
                {
                    Models.AdminDBModels.AdminUser adminUser = ctx.AdminUsers.Where(p => p.Username == "admin").FirstOrDefault();
                    if (adminUser == null)
                    {
                        adminUser = new Models.AdminDBModels.AdminUser();
                        adminUser.Username = "admin";
                        ctx.AdminUsers.Add(adminUser);
                    }
                    adminUser.Password = adminUserPassword;
                    ctx.SaveChanges();
                }
                strMessage.AppendLine("Following is the password for the user 'admin' please keep it in a safe place, since it will not be shown again");
                strMessage.AppendLine(adminUserPassword);
                ViewBag.Message = strMessage.ToString();
            }
            ViewModels.InitialSetup model = new ViewModels.InitialSetup();
            return View(model);
        }
    }
}