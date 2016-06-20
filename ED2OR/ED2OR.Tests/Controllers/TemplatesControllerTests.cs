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
    public class TemplatesControllerTests
    {
        [TestMethod()]
        public void IndexTest()
        {
            TemplatesController controller = new TemplatesController();
            ViewResult result = controller.Index() as ViewResult;
            Assert.IsNotNull(result, "Invalid Result");
            Assert.IsNotNull(result.Model, "Invalid Model");
            List<ED2OR.ViewModels.TemplateViewModel> model = result.Model as List<ED2OR.ViewModels.TemplateViewModel>;
            Assert.IsNotNull(model, "Unable to cast model");
            if (model.Count > 0)
            {
                var firstItem = model.First();
                Assert.IsTrue(firstItem.NumberOfDownloads == ED2OR.Controllers.TemplatesController.NUMBER_OF_DOWNLOADS,"Incorrect Number of Downloads");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.AccessToken), "Empty Access Token");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.AccessUrl), "Empty Access Token");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.TemplateName), "Empty Template Name");
                Assert.IsTrue(!string.IsNullOrWhiteSpace(firstItem.VendorName), "Empty Vendor Name");
            }
        }
    }
}