using System.Data.Entity;

namespace SecuroteckWebApplication.Models
{
    public class UserContext : DbContext
    {
        public UserContext() : base()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, Migrations.Configuration>());


        }

        /// <summary>
        /// Gets or Sets the Users
        /// </summary>
        public DbSet<User> Users { get; set; }


        /// <summary>
        /// Gets or Sets the Logs
        /// </summary>
        public DbSet<Log> Logs { get; set; }

        /// <summary>
        /// Gets or sets the logs from users that have deleted themselves
        /// </summary>
        public DbSet<LogArchive> LogArchive { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);
        }
    }
}