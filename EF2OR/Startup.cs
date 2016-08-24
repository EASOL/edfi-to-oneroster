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
            Utils.CommonUtils.PathProvider = new Providers.WebPathProvider();
            string strDatabaseConfigFile = Utils.CommonUtils.PathProvider.MapPath("database.config");
            if (!System.IO.File.Exists(strDatabaseConfigFile))
                System.IO.File.WriteAllBytes(strDatabaseConfigFile, Properties.Resources.database_config);
            Utils.CommonUtils.ApiResponseProvider = new Providers.ApiResponseProvider();
            Utils.CommonUtils.HttpContextProvider = new Providers.HttpContextProvider();
            Utils.CommonUtils.UserProvider = new Providers.WebUserProvider();
            //The following code will force Entity Framework Migrations on Application Start
            ConfigureMigrations(app);
        }

        private void ConfigureMigrations(IAppBuilder app)
        {
            try
            {
                var httpContext = System.Web.HttpContext.Current;
                if (httpContext != null)
                {
                    //We force migrations to run when a database has been set
                    if (!Utils.PreConfigurationHelper.IsInitialSetup(httpContext))
                    {
                        var configuration = new Configuration();
                        var migrator = new DbMigrator(configuration);
                        migrator.Update();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
