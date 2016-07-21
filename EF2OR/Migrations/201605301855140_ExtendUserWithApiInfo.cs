namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendUserWithApiInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ApiBaseUrl", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApiKey", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApiSecret", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ApiSecret");
            DropColumn("dbo.AspNetUsers", "ApiKey");
            DropColumn("dbo.AspNetUsers", "ApiBaseUrl");
        }
    }
}
