namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TemplateFilters : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Templates", "Filters", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Templates", "Filters");
        }
    }
}
