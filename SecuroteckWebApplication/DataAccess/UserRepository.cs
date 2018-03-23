using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new <see cref="UserRepository"/> instance
        /// </summary>
        /// <param name="context"></param>
        public UserRepository(UserContext context) => _context = context;


        /// <summary>
        /// Gets a list of all the users
        /// </summary>
        public IEnumerable<User> GetUsers() => _context.Users.ToList();

        /// <summary>
        /// Gets a user by user name and/or ApiKey
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>A user object, null if no matches found</returns>
        public User GetUser(Func<User, bool> selector) => _context.Users.FirstOrDefault(selector);

        public User GetUserByUserName(string userName) => _context.Users.FirstOrDefault(x => x.UserName == userName);


        /// <summary>
        /// Checks the database to see if a user with of given username and/or api key exists
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public bool CheckUser(Func<User, bool> selector) => _context.Users.Any(selector);


        /// <summary>
        /// Inserts a new user into the database
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User InsertUser(string userName)
        {
            var user = new User
            {
                UserName = userName,
                ApiKey = Guid.NewGuid().ToString()
            };

            _context.Users.Add(user);

            return user;
        }

        public IEnumerable<User> InsertUsers(IEnumerable<User> users)
        {
            throw new NotImplementedException();


        }

        public async Task DeleteUser(User user)
        {

            this._context.LogArchive.AddRange(user.Logs.Select(log => new LogArchive
            {
                LogString = log.LogString,
                LogId = log.LogId,
                LogDateTime = log.LogDateTime,
                Name = user.UserName,
                ApiKey = user.ApiKey
            }));


            _context.Logs.RemoveRange(user.Logs);
            _context.Users.Remove(user);
        }

        public void UpdateUser(User user) => _context.Users.AddOrUpdate(user);

        public async Task SaveChanges() => await _context.SaveChangesAsync();

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
