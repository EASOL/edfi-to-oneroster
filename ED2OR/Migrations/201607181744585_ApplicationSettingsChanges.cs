namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationSettingsChanges : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationSettings",
                c => new
                    {
                        ApplicationSettingsId = c.Int(nullable: false, identity: true),
                        SettingName = c.String(),
                        SettingValue = c.String(),
                    })
                .PrimaryKey(t => t.ApplicationSettingsId);
            
            DropColumn("dbo.AspNetUsers", "ApiBaseUrl");
            DropColumn("dbo.AspNetUsers", "ApiKey");
            DropColumn("dbo.AspNetUsers", "ApiSecret");
            DropTable("dbo.MappingSettings");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MappingSettings",
                c => new
                    {
                        MappingSettingId = c.Int(nullable: false, identity: true),
                        SettingName = c.String(),
                        SettingValue = c.String(),
                    })
                .PrimaryKey(t => t.MappingSettingId);
            
            AddColumn("dbo.AspNetUsers", "ApiSecret", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApiKey", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApiBaseUrl", c => c.String());
            DropTable("dbo.ApplicationSettings");
        }
    }
}
