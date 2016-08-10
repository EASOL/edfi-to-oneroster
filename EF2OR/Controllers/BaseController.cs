using System;
using System.Web.Mvc;
using EF2OR.Models;
using Microsoft.AspNet.Identity;
using EF2OR.ViewModels;

namespace EF2OR.Controllers
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
                return Utils.CommonUtils.UserProvider.UserName;
            }
        }

        protected string IpAddress
        {
            get
            {
                return Utils.CommonUtils.UserProvider.IpAddress;
            }
        }

        internal static bool IsValidConnectionString(string connectionString, out string errors)
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
            System.Data.SqlClient.SqlConnectionStringBuilder strCSB = BuildConnectionStringBuilder(model);
            try
            {
                bool isValidConnectionString = IsValidConnectionString(strCSB.ConnectionString, out errors);
                if (isValidConnectionString)
                {
                    var configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
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
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to setup database configuration. Error: " + ex.ToString();
                succeeded = false;
            }
            return succeeded;
        }

        internal static System.Data.SqlClient.SqlConnectionStringBuilder BuildConnectionStringBuilder(InitialSetup model)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder strCSB = new System.Data.SqlClient.SqlConnectionStringBuilder();
            strCSB.DataSource = model.DatabaseServer;
            strCSB.InitialCatalog = model.DatabaseName;
            if (!model.IntegratedSecuritySSPI)
            {
                strCSB.IntegratedSecurity = false;
                strCSB.UserID = model.DatabaseUserId;
                if (!string.IsNullOrWhiteSpace(model.DatabaseUserPassword))
                {
                    strCSB.Password = model.DatabaseUserPassword;
                }
            }
            else
            {
                strCSB.IntegratedSecurity = true;
            }
            if (!string.IsNullOrWhiteSpace(model.DatabaseApplicationName))
                strCSB.ApplicationName = model.DatabaseApplicationName;
            return strCSB;
        }
    }
}