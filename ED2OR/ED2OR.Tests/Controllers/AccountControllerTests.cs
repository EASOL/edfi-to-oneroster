using Microsoft.VisualStudio.TestTools.UnitTesting;
using ED2OR.Controllers;
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
using ED2OR.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ED2OR.Controllers.Tests
{
    [TestClass()]
    public class AccountControllerTests
    {
        private const string TESTUSER_USERNAME = "testuser@learningtapestry.com";
        private const string TESTUSER_ORIGINALPASSWORD = "Admin@2016";
        private static ApplicationDbContext db = null;
        [ClassInitialize()]
        public static void InitializeClass(TestContext context)
        {
            db = new ApplicationDbContext();
            var testUser = db.Users.Where(p => p.UserName == TESTUSER_USERNAME).FirstOrDefault();
            if (testUser != null)
            {
                db.Users.Remove(testUser);
                db.SaveChanges();
            }
        }

        [TestMethod()]
        public void LoginTest()
        {
            AccountController controller = new AccountController();
            ViewResult result = controller.Login(returnUrl: string.Empty) as ViewResult;
            Assert.IsNotNull(result, "Invalid Result");
            Assert.Inconclusive("Need to validate 2 cases: when there are users and where there are no users->Redirect to Register");
        }

        /// <summary>
        /// Check http://stackoverflow.com/questions/28405966/how-to-mock-applicationusermanager-from-accountcontroller-in-mvc5
        /// </summary>
        [TestMethod()]
        public async Task Register()
        {
            var accountController = new AccountController();
            //var accountController = new AccountController(userManager.Object, signInManager.Object, authenticationManager.Object);

            // Act
            var result = accountController.Register();
            Assert.IsNotNull(result, "Invalid Result");
            if (result is RedirectToRouteResult)
            {
                RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
                Assert.IsTrue(redirectResult.RouteValues["action"].ToString() == "Login", "Unexpected Redirect");
            }
            else
            {
                ViewResult viewResult = result as ViewResult;
                //Assert.Inconclusive("accountController.Register is failing tests, possibly because of using Mocks or OWIN. Validating if using Microsoft's Fakes Assembly work");
                using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
                {
                    ED2OR.Controllers.Fakes.ShimAccountController.AllInstances.UserManagerGet = (a) =>
                    {
                        var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                        return manager;
                    };
                    Microsoft.AspNet.Identity.Fakes.ShimUserManager<ApplicationUser, string>.AllInstances.CreateAsyncT0 = (userManager, userInfo) => 
                     {
                         try
                         {
                             db.Users.Add(userInfo);
                             db.SaveChanges();
                             return Task.FromResult(IdentityResult.Success);
                         }
                         catch ( System.Data.Entity.Validation.DbEntityValidationException dbValEx)
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
                    var createUserResult = 
                        await accountController.Register(model: new ViewModels.RegisterViewModel()
                    {
                        Email = TESTUSER_USERNAME,
                        Password = TESTUSER_ORIGINALPASSWORD,
                        ConfirmPassword = TESTUSER_ORIGINALPASSWORD

                    });
                    if (createUserResult is RedirectToRouteResult)
                    {
                        RedirectToRouteResult createUserRedirectResult = createUserResult as RedirectToRouteResult;
                        Assert.IsTrue(createUserRedirectResult.RouteValues["action"].ToString() == "Index" &&
                            createUserRedirectResult.RouteValues["controller"].ToString() == "Home", "Unexpected Redirect");
                    }
                    else
                    {
                        Assert.Inconclusive("Need to validate if there are errors");
                    }
                }
            }

        }

        private static void PrepareAuthentication(out Mock<ApplicationUserManager> userManager, out Mock<ApplicationSignInManager> signInManager)
        {
            HttpContext.Current = CreateHttpContext(userLoggedIn: false);
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var authenticationManager = new Mock<IAuthenticationManager>();
            signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
        }

        /// <summary>
        /// Check http://stackoverflow.com/questions/28405966/how-to-mock-applicationusermanager-from-accountcontroller-in-mvc5
        /// </summary>
        /// <param name="userLoggedIn"></param>
        /// <returns></returns>
        private static HttpContext CreateHttpContext(bool userLoggedIn)
        {
            var httpContext = new HttpContext(
                new HttpRequest(string.Empty, "http://sample.com", string.Empty),
                new HttpResponse(new StringWriter())
            )
            {
                User = userLoggedIn
                    ? new GenericPrincipal(new GenericIdentity("userName"), new string[0])
                    : new GenericPrincipal(new GenericIdentity(string.Empty), new string[0])
            };

            return httpContext;
        }
    }
}