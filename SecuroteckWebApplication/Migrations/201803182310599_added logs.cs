namespace SecuroteckWebApplication.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addedlogs : DbMigration
    {
        public override void Up()
        {
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "User_ApiKey", "dbo.Users");
            DropIndex("dbo.Logs", new[] { "User_ApiKey" });
            DropTable("dbo.Logs");
        }
    }
}
