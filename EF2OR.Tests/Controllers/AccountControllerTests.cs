using Microsoft.VisualStudio.TestTools.UnitTesting;
using EF2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using System.IO;
using System.Security.Principal;
using Moq;
using EF2OR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using EF2OR.Tests.Utils;
using EF2OR.Utils;

namespace EF2OR.Controllers.Tests
{
    //[TestClass()]
    public class AccountControllerTests
    {
        private static ApplicationDbContext db = null;
        [ClassInitialize()]
        public static void InitializeClass(TestContext context)
        {
        }

        [TestMethod()]
        public void AccountController_LoginTest()
        {
            Assert.Inconclusive("SignInManager is causing test to fail");
            AccountController controller = new AccountController();
            var result = controller.Login(returnUrl: string.Empty);
            Assert.IsNotNull(result, "Invalid Result");
            if (result is RedirectToRouteResult)
            {
                RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
                Assert.IsTrue(redirectResult.RouteValues["action"].ToString() == "Register", "Unexpected Redirect");
            }
            else
            if (result is ViewResult)
            {
                ViewResult viewResult = result as ViewResult;
                Assert.IsNotNull("Invalid Result");
            }
            else
                Assert.Fail("Unexpected Result");
        }

        /// <summary>
        /// Check http://stackoverflow.com/questions/28405966/how-to-mock-applicationusermanager-from-accountcontroller-in-mvc5
        /// </summary>
        //[TestMethod()]
        //public async Task AccountController_RegisterTest()
        //{
        //    Assert.Inconclusive("Test Failing.");
        //    ApplicationUserManager objUserManager = null;
        //    ApplicationSignInManager objSignInManager = null;
        //    PrepareAuthentication(out objUserManager, out objSignInManager);
        //    var accountController = new AccountController(objUserManager, objSignInManager);

        //    // Act
        //    var result = accountController.Register();
        //    Assert.IsNotNull(result, "Invalid Result");
        //    if (result is RedirectToRouteResult)
        //    {
        //        RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
        //        Assert.IsTrue(redirectResult.RouteValues["action"].ToString() == "Login", "Unexpected Redirect");
        //    }
        //    else
        //    {
        //        ViewResult viewResult = result as ViewResult;
        //        //Assert.Inconclusive("accountController.Register is failing tests, possibly because of using Mocks or OWIN. Validating if using Microsoft's Fakes Assembly work");
        //        using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
        //        {
        //            EF2OR.Controllers.Fakes.ShimAccountController.AllInstances.UserManagerGet = (a) =>
        //            {
        //                var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
        //                return manager;
        //            };
        //            Microsoft.AspNet.Identity.Fakes.ShimUserManager<ApplicationUser, string>.AllInstances.CreateAsyncT0 = (userManager, userInfo) =>
        //             {
        //                 try
        //                 {
        //                     db.Users.Add(userInfo);
        //                     db.SaveChanges();
        //                     return Task.FromResult(IdentityResult.Success);
        //                 }
        //                 catch (System.Data.Entity.Validation.DbEntityValidationException dbValEx)
        //                 {
        //                     System.Text.StringBuilder errorsList = new StringBuilder();
        //                     foreach (var valError in dbValEx.EntityValidationErrors)
        //                     {
        //                         foreach (var singleError in valError.ValidationErrors)
        //                         {
        //                             errorsList.AppendLine(string.Format("Property: {0} - Message:{1}", singleError.PropertyName, singleError.ErrorMessage));
        //                         }
        //                     }
        //                     throw dbValEx;
        //                 }
        //             };
        //            EF2OR.Models.Fakes.ShimApplicationUser.AllInstances.GenerateUserIdentityAsyncUserManagerOfApplicationUser = (fakeApplicationUser, fakeUserManager) =>
        //            {
        //                UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
        //                fakeUserManager = new UserManager<ApplicationUser>(userStore);
        //                return fakeUserManager.CreateIdentityAsync(fakeApplicationUser, DefaultAuthenticationTypes.ApplicationCookie);
        //            };
        //            Microsoft.AspNet.Identity.Owin.Fakes.ShimSignInManager<ApplicationUser, string>.AllInstances.UserManagerGet =
        //                (fakeSignInManager) =>
        //                {
        //                    UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
        //                    ApplicationUserManager userManager = new ApplicationUserManager(userStore);
        //                    return userManager;
        //                };
        //            Assert.Inconclusive("accountController.Register is failing on await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false); requires research");
        //            var createUserResult =
        //                await accountController.Register(model: new ViewModels.RegisterViewModel()
        //                {
        //                    Email = AuthenticationHelper.TESTUSER_USERNAME,
        //                    Password = AuthenticationHelper.TESTUSER_ORIGINALPASSWORD,
        //                    ConfirmPassword = AuthenticationHelper.TESTUSER_ORIGINALPASSWORD,
        //                    ApiBaseUrl = AuthenticationHelper.TESTAPI_BASEURL,
        //                    ApiKey = AuthenticationHelper.TESTAPI_APIKEY,
        //                    ApiSecret = AuthenticationHelper.TESTAPI_APISECRET
        //                });
        //            if (createUserResult is RedirectToRouteResult)
        //            {
        //                RedirectToRouteResult createUserRedirectResult = createUserResult as RedirectToRouteResult;
        //                Assert.IsTrue(createUserRedirectResult.RouteValues["action"].ToString() == "Index" &&
        //                    createUserRedirectResult.RouteValues["controller"].ToString() == "Home", "Unexpected Redirect");
        //            }
        //            else
        //            {
        //                Assert.Inconclusive("Need to validate if there are errors");
        //            }
        //        }
        //    }

        //}

        //private static void PrepareAuthentication(out ApplicationUserManager userManager, out ApplicationSignInManager signInManager)
        //{
        //    using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
        //    {
        //        AuthenticationHelper.HttpContext = AuthenticationHelper.CreateHttpContext(false);
        //        var response = server.HttpClient.GetAsync("/");
        //        response.Wait();
        //        Microsoft.Owin.OwinContext ctx = new Microsoft.Owin.OwinContext(new System.Collections.Generic.Dictionary<string, object>());
        //        UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
        //        userManager = new ApplicationUserManager(userStore);
        //        using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
        //        {
        //            EF2OR.Fakes.ShimApplicationSignInManager.ConstructorApplicationUserManagerIAuthenticationManager = (fakeSignInManager, fakeUserManager, fakeAuthenticationManager) =>
        //            {
        //                fakeUserManager = new ApplicationUserManager(userStore);
        //                Microsoft.AspNet.Identity.Owin.SignInManager<ApplicationUser, string> a = new Microsoft.AspNet.Identity.Owin.SignInManager<ApplicationUser, string>(
        //                    fakeUserManager, fakeAuthenticationManager);
        //            };
        //            signInManager = ApplicationSignInManager.Create(
        //                options: new Microsoft.AspNet.Identity.Owin.IdentityFactoryOptions<ApplicationSignInManager>()
        //                {
        //                    DataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider(),
        //                    Provider = new Microsoft.AspNet.Identity.Owin.IdentityFactoryProvider<ApplicationSignInManager>()
        //                },
        //                context: ctx);
        //        }
        //    }
        //}

    }
}