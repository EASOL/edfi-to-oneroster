namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Templates : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Templates",
                c => new
                    {
                        TemplateId = c.Int(nullable: false, identity: true),
                        TemplateName = c.String(),
                        VendorName = c.String(),
                        AccessUrl = c.String(),
                        AccessToken = c.String(),
                    })
                .PrimaryKey(t => t.TemplateId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Templates");
        }
    }
}
