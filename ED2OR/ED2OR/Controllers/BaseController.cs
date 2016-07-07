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

        protected string UserName
        {
            get
            {
                return User.Identity.Name;
            }
        }

        protected bool RequiresValidConnectionString()
        {
            bool requiresInitialSetup = false;
            using (Models.AdminDBModels.AdminDBEntities ctx = new Models.AdminDBModels.AdminDBEntities())
            {
                var configRow = ctx.ConfigurationInfoes.FirstOrDefault();
                if (configRow != null)
                {
                    requiresInitialSetup = configRow.RequiresInitialConnectionStringSetup;
                }
            }
            return requiresInitialSetup;
        }

        private static bool IsValidConnectionString(string connectionString, out string errors)
        {
            bool isValid = false;
            using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    con.Close();
                    isValid = true;
                    errors = string.Empty;
                }
                catch (Exception ex)
                {
                    isValid = false;
                    errors = ex.Message;
                }
            }
            return isValid;
        }

        protected bool SaveConnectionString(ViewModels.InitialSetup model, out string errors)
        {
            bool succeeded = false;
            errors = string.Empty;
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
                bool isValidConnectionString = IsValidConnectionString(strCSB.ConnectionString, out errors);
                if (isValidConnectionString)
                {
                    var configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/");
                    configuration.ConnectionStrings.ConnectionStrings.Remove("DefaultConnection");
                    configuration.ConnectionStrings.ConnectionStrings.Add(
                        new System.Configuration.ConnectionStringSettings("DefaultConnection", strCSB.ConnectionString, "System.Data.SqlClient"));
                    configuration.Save(System.Configuration.ConfigurationSaveMode.Modified);
                    succeeded = true;
                }
                else
                {
                    succeeded = false;
                }
                //return RedirectToAction(actionName: "Index", controllerName: "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to setup database configuration. Error: " + ex.ToString();
                //return View(model);
                succeeded = false;
            }
            return succeeded;
        }

    }
}