using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace SecuroteckWebApplicationCore
{
    public class UserContext : DbContext
    {

        public UserContext()
        {
            //System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, Con>());
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<UserContext, DbLoggerCategory.Migrations.Configuration>());
        }
    }
}
