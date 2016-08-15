using System;
using Microsoft.Owin;
using Owin;
using EF2OR.Migrations;
using System.Data.Entity.Migrations;

[assembly: OwinStartupAttribute(typeof(EF2OR.Startup))]
namespace EF2OR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //The following code will force Entity Framework Migrations on Application Start
            ConfigureMigrations(app);
            Utils.CommonUtils.ApiResponseProvider = new Providers.ApiResponseProvider();
            Utils.CommonUtils.PathProvider = new Providers.WebPathProvider();
            Utils.CommonUtils.HttpContextProvider = new Providers.HttpContextProvider();
            Utils.CommonUtils.UserProvider = new Providers.WebUserProvider();
        }

        private void ConfigureMigrations(IAppBuilder app)
        {
            try
            {
                var configuration = new Configuration();
                var migrator = new DbMigrator(configuration);
                migrator.Update();
            }
            catch (Exception ex)
            {

            }
        }
    }
}