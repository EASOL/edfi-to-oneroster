using ED2OR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ED2OR.Tests.Utils
{
    internal class AuthenticationHelper
    {
        internal const string TESTUSER_USERNAME = "testuser@learningtapestry.com";
        internal const string TESTUSER_ORIGINALPASSWORD = "Admin@2016";
        private static ED2OR.Models.ApplicationDbContext _db = null;
        private static UserStore<ED2OR.Models.ApplicationUser> _userStore = null;
        private static ApplicationUserManager _applicationUserManager = null;
        private static ED2OR.Models.ApplicationUser _testUser = null;

        static AuthenticationHelper()
        {
            //Initialize();
        }

        internal static void Initialize()
        {
            _db = new Models.ApplicationDbContext();
            _userStore = new UserStore<Models.ApplicationUser>(_db);
            _applicationUserManager = new ApplicationUserManager(_userStore);
            _testUser = DB.Users.Where(p => p.UserName == TESTUSER_USERNAME).FirstOrDefault();
            if (_testUser == null)
            {
                CreateTestUser();
            }
        }

        internal static ED2OR.Models.ApplicationUser TestUser
        {
            get
            {
                return _testUser;
            }
        }
        private static async void CreateTestUser()
        {
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                Microsoft.AspNet.Identity.Fakes.ShimUserManager<ApplicationUser, string>.AllInstances.CreateAsyncT0 = (userManager, userInfo) =>
            {
                try
                {
                    _db.Users.Add(userInfo);
                    _db.SaveChanges();
                    _testUser = userInfo;
                    return Task.FromResult(IdentityResult.Success);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbValEx)
                {
                    System.Text.StringBuilder errorsList = new StringBuilder();
                    foreach (var valError in dbValEx.EntityValidationErrors)
                    {
                        foreach (var singleError in valError.ValidationErrors)
                        {
                            errorsList.AppendLine(string.Format("Property: {0} - Message:{1}", singleError.PropertyName, singleError.ErrorMessage));
                        }
                    }
                    throw dbValEx;
                }
            };
                var user = new ApplicationUser
                {
                    UserName = TESTUSER_USERNAME,
                    Email = TESTUSER_USERNAME,
                    ApiBaseUrl = string.Empty,
                    ApiKey = string.Empty,
                    ApiSecret = string.Empty
                };
                var result = await AppUserManager.CreateAsync(user, TESTUSER_ORIGINALPASSWORD);
            };
        }

        internal static ApplicationUserManager AppUserManager
        {
            get
            {
                return _applicationUserManager;
            }
        }
        internal static UserStore<ED2OR.Models.ApplicationUser> UserManager
        {
            get
            {
                return _userStore;
            }
        }
        internal static ED2OR.Models.ApplicationDbContext DB
        {
            get
            {
                return _db;
            }
        }
    }
}
