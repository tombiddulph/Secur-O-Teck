namespace SecuroteckWebApplication.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Userclasscreated : DbMigration
    {
        public override void Up()
        {
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
            DropTable("dbo.Users");
        }
    }
}
