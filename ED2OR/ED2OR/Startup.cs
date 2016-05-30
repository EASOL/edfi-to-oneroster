using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ED2OR.Startup))]
namespace ED2OR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
