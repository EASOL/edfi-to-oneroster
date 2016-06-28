using Microsoft.VisualStudio.TestTools.UnitTesting;
using ED2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ED2OR.Controllers.Tests
{
    [TestClass()]
    public class HomeControllerTests
    {
        [TestMethod()]
        public void HomeController_IndexTest()
        {
            HomeController homeController = new HomeController();
            ViewResult result = homeController.Index() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}