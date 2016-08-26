namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VersionOnePointOne : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Templates", "OneRosterVersion", c => c.String());
            AddColumn("dbo.Templates", "DownloadType", c => c.String());

            Sql("UPDATE Templates SET OneRosterVersion = '1.0'");
            Sql("INSERT INTO ApplicationSettings (SettingName, SettingValue) VALUES ('DefaultOneRosterVersion','1.0')");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Templates", "DownloadType");
            DropColumn("dbo.Templates", "OneRosterVersion");
        }
    }
}
