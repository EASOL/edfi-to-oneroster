using System;
using Microsoft.Owin;
using Owin;
using ED2OR.Migrations;
using System.Data.Entity.Migrations;

[assembly: OwinStartupAttribute(typeof(ED2OR.Startup))]
namespace ED2OR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //The following code will force Entity Framework Migrations on Application Start
            //ConfigureMigrations(app);
        }

        private void ConfigureMigrations(IAppBuilder app)
        {
            var configuration = new Configuration();
            var migrator = new DbMigrator(configuration);
            migrator.Update();
        }
    }
}
