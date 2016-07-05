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
                            System.Data.SqlClient.SqlConnectionStringBuilder strCSB = new System.Data.SqlClient.SqlConnectionStringBuilder();
                            strCSB.DataSource = model.DatabaseServer;
                            strCSB.InitialCatalog = model.DatabaseName;
                            if (!model.IntegratedSecuritySSPI)
                            {
                                strCSB.IntegratedSecurity = false;
                                strCSB.UserID = model.DatabaseUserId;
                                strCSB.Password = model.DatabaseUserPassword;
                            }
                            else
                            {
                                strCSB.IntegratedSecurity = true;
                            }
                            strCSB.ApplicationName = model.DatabaseApplicationName;
                            try
                            {
                                using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(strCSB.ConnectionString))
                                {
                                    con.Open();
                                    con.Close();
                                    var configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/");
                                    configuration.ConnectionStrings.ConnectionStrings.Remove("DefaultConnection");
                                    configuration.ConnectionStrings.ConnectionStrings.Add(
                                        new System.Configuration.ConnectionStringSettings("DefaultConnection", strCSB.ConnectionString, "System.Data.SqlClient"));
                                    configuration.Save(System.Configuration.ConfigurationSaveMode.Modified);
                                }
                                return RedirectToAction(actionName: "Index", controllerName: "Home");
                            }
                            catch (Exception ex)
                            {
                                ViewBag.Error = "Unable to setup database configuration. Error: " + ex.ToString();
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