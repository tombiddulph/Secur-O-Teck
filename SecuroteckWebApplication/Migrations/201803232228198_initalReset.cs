namespace SecuroteckWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initalReset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogArchives",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        LogId = c.Guid(nullable: false),
                        LogString = c.String(nullable: false),
                        LogDateTime = c.DateTime(nullable: false),
                        Name = c.String(nullable: false),
                        ApiKey = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogId = c.Guid(nullable: false, identity: true),
                        LogString = c.String(),
                        LogDateTime = c.DateTime(nullable: false),
                        User_ApiKey = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Users", t => t.User_ApiKey)
                .Index(t => t.User_ApiKey);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        ApiKey = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.ApiKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "User_ApiKey", "dbo.Users");
            DropIndex("dbo.Logs", new[] { "User_ApiKey" });
            DropTable("dbo.Users");
            DropTable("dbo.Logs");
            DropTable("dbo.LogArchives");
        }
    }
}
