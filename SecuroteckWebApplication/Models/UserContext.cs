﻿using System.Data.Entity;

namespace SecuroteckWebApplication.Models
{
    public class UserContext : DbContext
    {
        public UserContext() : base()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, Migrations.Configuration>());
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}