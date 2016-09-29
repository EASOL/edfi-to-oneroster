using System.Linq;
using System.Web;
using System.Web.Mvc;
using EF2OR.ViewModels;
using EF2OR.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using EF2OR.Enums;
using EF2OR.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EF2OR.Controllers
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
                            TermDescriptor = term,
                            Type = ""
                        });
                }
            }
            catch
            {
                isError = true;
                errorMessage = "There was an error in getting the latest Term Descriptors.  Make sure the API Key and Secret are correct and test properly.  Also, the prefix must be correctly entered.";
            }

            var apiBaseUrl = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiBaseUrl)?.SettingValue;
            var apiKey = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiKey)?.SettingValue;
            var apiSecret = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiSecret)?.SettingValue;
            var apiPrefix = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.ApiPrefix)?.SettingValue;
            var orgsIdentifier = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == ApplicationSettingsTypes.OrgsIdentifier)?.SettingValue;

            var model = new SettingsViewModel
            {
                ApiBaseUrl = apiBaseUrl,
                ApiKey = apiKey,
                ApiSecret = apiSecret,
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
            ViewBag.AcademicSessionTypeValues = new List<SelectListItem>
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

            ViewBag.OneRosterVersions = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Text = OneRosterVersions.OR_1_0,
                        Value = OneRosterVersions.OR_1_0
                    },
                    new SelectListItem
                    {
                        Text = OneRosterVersions.OR_1_1,
                        Value = OneRosterVersions.OR_1_1
                    }
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult TestDatabaseConnection(ViewModels.InitialSetup DatabaseSettings)
        {
            try
            {
                var errorsList = PreValidateDatabaseSettings(DatabaseSettings);
                if (errorsList.Count > 0)
                {
                    return Json(new
                    {
                        IsSuccessful = false,
                        ErrorMessage = String.Join("\r\n", errorsList.ToArray())
                    });
                }
                var connectionStrigBuilder = BaseController.BuildConnectionStringBuilder(DatabaseSettings);
                string errors = string.Empty;
                bool isValidConnection = IsValidConnectionString(connectionStrigBuilder.ConnectionString, out errors);
                if (isValidConnection)
                    return Json(new { IsSuccessful = true });
                else
                    return Json(new { IsSuccessful = false, ErrorMessage = errors });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccessful = false, ErrorMessage = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult TestConnection(string apiBaseUrl, string apiKey, string apiSecret)
        {
            var tokenResult = Providers.ApiResponseProvider.GetToken(apiBaseUrl, apiKey, apiSecret);
            return Json(tokenResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Index(SettingsViewModel model)
        {
            LoadDropdownValues();

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

            var baseUrlChanged = InsertUpdateApplicationSetting(ApplicationSettingsTypes.ApiBaseUrl, model.ApiBaseUrl);
            var apiKeyChanged = InsertUpdateApplicationSetting(ApplicationSettingsTypes.ApiKey, model.ApiKey);
            var apiSecretChanged = InsertUpdateApplicationSetting(ApplicationSettingsTypes.ApiSecret, model.ApiSecret);
            InsertUpdateApplicationSetting(ApplicationSettingsTypes.ApiPrefix, model.ApiPrefix);
            InsertUpdateApplicationSetting(ApplicationSettingsTypes.OrgsIdentifier, model.OrgsIdentifier);
            InsertUpdateApplicationSetting(ApplicationSettingsTypes.DefaultOneRosterVersion, model.DefaultOneRosterVersion);

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
            try
            {
                db.SaveChanges(UserName, IpAddress);

                if (baseUrlChanged || apiKeyChanged || apiSecretChanged)
                {
                    Session.Clear();
                    CommonUtils.ExistingResponses.Clear();
                    Providers.ApiResponseProvider.GetToken(true);
                }

                ViewBag.SuccessMessage = "Settings successfully saved";
                var result = await Index();
                return result;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        private bool InsertUpdateApplicationSetting(string settingName, string settingValue)
        {
            bool valueChanged = false;
            var setting = db.ApplicationSettings.FirstOrDefault(x => x.SettingName == settingName);

            if (setting == null)
            {
                valueChanged = true;
                var newSetting = new ApplicationSetting
                {
                    SettingName = settingName,
                    SettingValue = settingValue
                };
                db.ApplicationSettings.Add(newSetting);
            }
            else
            {
                if (setting.SettingValue != settingValue)
                {
                    valueChanged = true;
                    setting.SettingValue = settingValue;
                }
            }
            return valueChanged;
        }

        [HttpPost]
        public ActionResult SaveDatabaseSettiings(SettingsViewModel model)
        {
            try
            {
                var errorsList = PreValidateDatabaseSettings(model.DatabaseSettings);
                if (errorsList.Count > 0)
                {
                    ViewBag.Error = String.Join("\r\n", errorsList.ToArray());
                    return View("Index", model);
                }
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
                        return RedirectToAction(actionName: "Index", controllerName: "Templates");
                    else
                    {
                        ViewBag.Error = errors;
                        return View("Index", model);
                    }
                }
                else
                {
                    ViewBag.Error = "Please verify database fields are correct and required fields are not empty";
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index", model);
            }
        }

        private List<string> PreValidateDatabaseSettings(InitialSetup model)
        {
            List<string> errors = new List<string>();
            if (model == null)
            {
                errors.Add("Database Settings cannot be empty");

            }
            if (string.IsNullOrWhiteSpace(model.DatabaseServer))
            {
                errors.Add("You must set a Database Server");
            }
            if (string.IsNullOrWhiteSpace(model.DatabaseName))
            {
                errors.Add("You must set a Database Name");
            }
            if (
                model.IntegratedSecuritySSPI == false && (
                string.IsNullOrWhiteSpace(model.DatabaseUserId) ||
                string.IsNullOrWhiteSpace(model.DatabaseUserPassword)))
            {
                errors.Add("You must set UserId and Password when not using Integrated Security");
            }
            return errors;
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