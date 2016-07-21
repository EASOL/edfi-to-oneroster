namespace EF2OR.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuditLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        AuditLogId = c.Int(nullable: false, identity: true),
                        User = c.String(),
                        IpAddress = c.String(),
                        DateTimeStamp = c.DateTime(nullable: false),
                        Type = c.String(),
                        Success = c.Boolean(nullable: false),
                        FailureReason = c.String(),
                        TemplateId = c.Int(nullable: false),
                        Fields = c.String(),
                        OldValues = c.String(),
                        NewValues = c.String(),
                        DownloadInfo = c.String(),
                    })
                .PrimaryKey(t => t.AuditLogId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AuditLogs");
        }
    }
}
