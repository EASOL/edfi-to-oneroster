using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EF2OR.Tests
{
    /// <summary>
    /// Check https://blogs.msdn.microsoft.com/webdev/2013/11/26/unit-testing-owin-applications-using-testserver/
    /// </summary>
    public class OwinApplicationTests
    {
        [TestMethod]
        public async void OwinAppTest()
        {
            using (var server = TestServer.Create<MyStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/");

                //Execute necessary tests
                Assert.Equals("Hello world using OWIN TestServer", await response.Content.ReadAsStringAsync());
            }
        }
    }

    public class MyStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseErrorPage(); // See Microsoft.Owin.Diagnostics
            //app.UseWelcomePage("/Welcome"); // See Microsoft.Owin.Diagnostics 
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello world using OWIN TestServer");
            });
        }
    }
}
