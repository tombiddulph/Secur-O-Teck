using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SecuroteckWebApplication.Models
{
    public class User
    {
        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        #endregion


        public User()
        {

        }

        [Key]
        public string ApiKey { get; set; }

        public string UserName { get; set; }

        public virtual ICollection<Log> Logs { get; set; }
    }

    #region Task11?
    // TODO: You may find it useful to add code here for Log
    #endregion

    public class UserDatabaseAccess
    {

       
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 
        #endregion
    }



    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly UserContext _context;
        private bool _disposed;

        public UserRepository(UserContext context)
        {
            this._context = context;
            this._disposed = false;
        }


        public IEnumerable<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        public bool CheckUser(string username)
        {
            throw new NotImplementedException();
        }

        public User GetUser(Func<User, bool> selector)
        {
            return _context.Users.FirstOrDefault(selector);
        }

        public User GetUserByUserName(string userName)
        {
            return _context.Users.FirstOrDefault(x => x.UserName == userName);
        }



        public bool CheckUser(Func<User, bool> selector)
        {
            return _context.Users.FirstOrDefault(selector) != null;
        }

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

        public void DeleteUser(User user)
        {
            if (this.CheckUser(x => x == user))
            {
                this._context.Users.Remove(user);
            }
        }

        public void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            this._disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IUserRepository
    {

        bool CheckUser(Func<User, bool> selector);
        IEnumerable<User> GetUsers();
        bool CheckUser(string username);
        User GetUser(Func<User, bool> selector);
        User GetUserByUserName(string userName);
        User InsertUser(string userName);
        IEnumerable<User> InsertUsers(IEnumerable<User> users);
        void DeleteUser(User user);
        void UpdateUser(User user);
        Task SaveChanges();

    }


}