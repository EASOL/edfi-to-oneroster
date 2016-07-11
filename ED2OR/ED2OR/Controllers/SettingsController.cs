using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED2OR.ViewModels;
using ED2OR.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ED2OR.Enums;
using ED2OR.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<ActionResult> Index()
        {
            var isError = false;
            var errorMessage = "";

            var academicSessionTypes = db.AcademicSessionTypes.Select(x => new AcademicSessionTypeViewModel
            {
                Id = x.AcademicSessionTypeId,
                TermDescriptor = x.TermDescriptor,
                Type = x.Type
            }).ToList();

            try
            {
                var apiTermDescriptors = await ApiCalls.GetTermDescriptors(true);
                var exsitingTermDescirptors = db.AcademicSessionTypes.Select(x => x.TermDescriptor).ToList();
                var newTermDescriptors = apiTermDescriptors.Where(x => !exsitingTermDescirptors.Contains(x)).ToList();

                foreach (var term in newTermDescriptors)
                {
                    academicSessionTypes.Add(
                        new AcademicSessionTypeViewModel
                        {
                            TermDescriptor = term
                        });
                }
            }
            catch
            {
                isError = true;
                errorMessage = "There was an error in getting the latest Term Descriptors.  Make sure the API Key and Secret are correct and test properly.  Also, the prefix must be correctly entered.";
            }

            var user = UserManager.FindById(UserId);
            var apiPrefix = db.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.ApiPrefix)?.SettingValue;
            var orgsIdentifier = db.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.OrgsIdentifier)?.SettingValue;

            var model = new SettingsViewModel
            {
                ApiBaseUrl = user.ApiBaseUrl,
                ApiKey = user.ApiKey,
                ApiSecret = user.ApiSecret,
                OrgsIdentifier = orgsIdentifier,
                ApiPrefix = apiPrefix,
                AcademicSessionTypes = academicSessionTypes,
                IsError = isError,
                ErrorMessage = errorMessage
            };
            var connectionString =
                System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            System.Data.SqlClient.SqlConnectionStringBuilder conStringBuilder =
                new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            model.DatabaseSettings = new InitialSetup()
            {
                DatabaseApplicationName = conStringBuilder.ApplicationName,
                DatabaseServer = conStringBuilder.DataSource,
                DatabaseName = conStringBuilder.InitialCatalog,
                DatabaseUserPassword = conStringBuilder.Password,
                DatabaseUserId = conStringBuilder.UserID,
                IntegratedSecuritySSPI = conStringBuilder.IntegratedSecurity
            };

            LoadDropdownValues();

            return View(model);
        }

        private void LoadDropdownValues()
        {
            ViewBag.AcademicSessionTypes = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Text = "term",
                        Value = "term"
                    },
                    new SelectListItem
                    {
                        Text = "gradingPeriod",
                        Value = "gradingPeriod"
                    },
                    new SelectListItem
                    {
                        Text = "schoolYear",
                        Value = "schoolYear"
                    },
                    new SelectListItem
                    {
                        Text = "semester",
                        Value = "semester"
                    }
                };
        }

        [HttpPost]
        public ActionResult TestDatabaseConnection(ViewModels.InitialSetup DatabaseSettings)
        {
            var connectionStrigBuilder = BaseController.BuildConnectionStringBuilder(DatabaseSettings);
            string errors = string.Empty;
            bool isValidConnection = IsValidConnectionString(connectionStrigBuilder.ConnectionString, out errors);
            if (isValidConnection)
                return Json(new { IsSuccessful = true });
            else
                return Json(new { IsSuccessful = false, ErrorMessage = errors });
        }

        [HttpPost]
        public ActionResult TestConnection(string apiBaseUrl, string apiKey, string apiSecret)
        {
            var tokenResult = ApiCalls.GetToken(apiBaseUrl, apiKey, apiSecret);
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

            var apiPrefixSetting = db.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.ApiPrefix);
            var orgsIdentifierSetting = db.MappingSettings.FirstOrDefault(x => x.SettingName == MappingSettingNames.OrgsIdentifier);

            if (apiPrefixSetting == null)
            {
                var newSetting = new MappingSetting
                {
                    SettingName = MappingSettingNames.ApiPrefix,
                    SettingValue = model.ApiPrefix
                };
                db.MappingSettings.Add(newSetting);
            }
            else
            {
                apiPrefixSetting.SettingValue = model.ApiPrefix;
            }

            if (orgsIdentifierSetting == null)
            {
                var newSetting = new MappingSetting
                {
                    SettingName = MappingSettingNames.OrgsIdentifier,
                    SettingValue = model.OrgsIdentifier
                };
                db.MappingSettings.Add(newSetting);
            }
            else
            {
                orgsIdentifierSetting.SettingValue = model.OrgsIdentifier;
            }


            if (model.AcademicSessionTypes != null)
            {
                foreach (var t in model.AcademicSessionTypes)
                {
                    var dbEntry = db.AcademicSessionTypes.FirstOrDefault(x => x.AcademicSessionTypeId == t.Id);
                    if (dbEntry == null)
                    {
                        db.AcademicSessionTypes.Add(
                            new AcademicSessionType
                            {
                                TermDescriptor = t.TermDescriptor,
                                Type = t.Type
                            });
                    }
                    else
                    {
                        dbEntry.TermDescriptor = t.TermDescriptor;
                        dbEntry.Type = t.Type;
                    }
                }
            }

            db.SaveChanges();
            var connectionString =
                System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            System.Data.SqlClient.SqlConnectionStringBuilder conStringBuilder =
                new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            if (
                (conStringBuilder.ApplicationName != model.DatabaseSettings.DatabaseApplicationName ||
                conStringBuilder.DataSource != model.DatabaseSettings.DatabaseServer ||
                conStringBuilder.InitialCatalog != model.DatabaseSettings.DatabaseName ||
                conStringBuilder.UserID != model.DatabaseSettings.DatabaseUserId ||
                conStringBuilder.Password != model.DatabaseSettings.DatabaseUserPassword ||
                conStringBuilder.IntegratedSecurity != model.DatabaseSettings.IntegratedSecuritySSPI) &&
                (
                !string.IsNullOrWhiteSpace(model.DatabaseSettings.DatabaseServer) &&
                !string.IsNullOrWhiteSpace(model.DatabaseSettings.DatabaseName)))
            {
                string errors = string.Empty;
                bool succeedSaveConnectionString = SaveConnectionString(model.DatabaseSettings, out errors);
                if (succeedSaveConnectionString)
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                else
                {
                    ViewBag.Error = errors;
                    return View(model);
                }
            }
            return RedirectToAction("Index");
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