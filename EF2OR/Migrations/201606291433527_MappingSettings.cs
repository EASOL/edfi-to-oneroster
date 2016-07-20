namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MappingSettings : DbMigration
    {
        public override void Up()
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MappingSettings");
        }
    }
}
