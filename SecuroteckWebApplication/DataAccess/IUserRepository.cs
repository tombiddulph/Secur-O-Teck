using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.DataAccess
{
    public interface IUserRepository : IDisposable
    {

        bool CheckUser(Func<User, bool> selector);
        IEnumerable<User> GetUsers();
        User GetUser(Func<User, bool> selector);
        User GetUserByUserName(string userName);
        User InsertUser(string userName);
        IEnumerable<User> InsertUsers(IEnumerable<User> users);
        Task DeleteUser(User user);
        void UpdateUser(User user);
        Task SaveChanges();

    }
}
