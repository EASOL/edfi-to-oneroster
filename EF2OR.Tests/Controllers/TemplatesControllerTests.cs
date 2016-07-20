using Microsoft.VisualStudio.TestTools.UnitTesting;
using EF2OR.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EF2OR.Controllers.Tests
{
    [TestClass()]
    public class TemplatesControllerTests
    {
        [TestMethod()]
        public void TemplatesController_IndexTest()
        {
            TemplatesController controller = new TemplatesController();
            ViewResult result = controller.Index() as ViewResult;
            Assert.IsNotNull(result, "Invalid Result");
            Assert.IsNotNull(result.Model, "Invalid Model");
            List<EF2OR.ViewModels.TemplateViewModel> model = result.Model as List<EF2OR.ViewModels.TemplateViewModel>;
            Assert.IsNotNull(model, "Unable to cast model");
            if (model.Count > 0)
            {
                var firstItem = model.First();
                Assert.IsTrue(firstItem.NumberOfDownloads >0 ,"Incorrect Number of Downloads");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.AccessToken), "Empty Access Token");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.AccessUrl), "Empty Access Token");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.TemplateName), "Empty Template Name");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.VendorName), "Empty Vendor Name");
            }
        }
    }
}