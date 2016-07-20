namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AcademicSessionTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AcademicSessionTypes",
                c => new
                    {
                        AcademicSessionTypeId = c.Int(nullable: false, identity: true),
                        TermDescriptor = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.AcademicSessionTypeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AcademicSessionTypes");
        }
    }
}
