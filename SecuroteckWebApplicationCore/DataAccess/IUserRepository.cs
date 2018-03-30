using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecuroteckWebApplicationCore.Models;

namespace SecuroteckWebApplicationCore.DataAccess
{
    public interface IUserRepository : IDisposable
    {

        bool CheckUser(Func<User, bool> selector);
        IEnumerable<User> GetUsers();
        User GetUser(Func<User, bool> selector);
        User GetUserByUserName(string userName);
        User InsertUser(string userName);
        Task DeleteUser(User user);
        void UpdateUser(User user);
        Task SaveChanges();

    }
}
