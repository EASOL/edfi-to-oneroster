using ED2OR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

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

        public static string TESTAPI_BASEURL { get; internal set; } = ConfigurationManager.AppSettings["TestApi_BaseUrl"];
        public static string TESTAPI_APIKEY { get; internal set; } = ConfigurationManager.AppSettings["TestApi_ApiKey"];
        public static string TESTAPI_APISECRET { get; internal set; } = ConfigurationManager.AppSettings["TestApi_ApiSecret"];

        public static HttpContext HttpContext
        {
            get;set;
        }

        /// <summary>
        /// Check http://stackoverflow.com/questions/28405966/how-to-mock-applicationusermanager-from-accountcontroller-in-mvc5
        /// Check http://stackoverflow.com/questions/9624242/setting-httpcontext-current-session-in-a-unit-test
        /// </summary>
        /// <param name="userLoggedIn"></param>
        /// <returns></returns>
        internal static HttpContext CreateHttpContext(bool userLoggedIn)
        {
            var httpContext = new HttpContext(
                new HttpRequest(string.Empty, "http://sample.com", string.Empty),
                new HttpResponse(new StringWriter())
            )
            {
                User = userLoggedIn
                    ? new GenericPrincipal(new GenericIdentity("userName"), new string[0])
                    : new GenericPrincipal(new GenericIdentity(string.Empty), new string[0]),
            };

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                            new HttpStaticObjectsCollection(), 10, true,
                                            HttpCookieMode.AutoDetect,
                                            SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }

        public class HttpContextManager
        {
            private static HttpContextBase m_context;
            public static HttpContextBase Current
            {
                get
                {
                    if (m_context != null)
                        return m_context;

                    if (HttpContext.Current == null)
                        throw new InvalidOperationException("HttpContext not available");

                    return new HttpContextWrapper(HttpContext.Current);
                }
            }

            public static void SetCurrentContext(HttpContextBase context)
            {
                m_context = context;
            }

        }
    }
}
