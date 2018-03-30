using Microsoft.EntityFrameworkCore;
using SecuroteckWebApplicationCore.Models;


namespace SecuroteckWebApplicationCore
{
    public class UserContext : DbContext
    {

        public UserContext()
        {
            //System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, Con>());
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, DbLoggerCategory.Migrations.Configuration>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogArchive> LogArchive { get; set; }
    }
}
