using System;
using System.Linq;


using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecuroteckWebApplicationCore.DataAccess;
using SecuroteckWebApplicationCore.Models;

namespace SecuroteckWebApplicationCore.Controllers
{

     public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }


        [ActionName("New"), HttpGet]
        public IActionResult Get([FromQuery]string userName)
        {

            if (string.IsNullOrEmpty(userName))
            {
                return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }

            if (!_userRepository.CheckUser(x => x.UserName == userName))
            {
                return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
                    
            }

            return Ok("True - User Does Exist! Did you mean to do a POST to create a new user?");
        }



        /// <summary>
        /// Creates a new user from the given username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [ActionName("New"), HttpPost]
        public async Task<IActionResult> Post([FromBody] string userName)
        {

            if (string.IsNullOrEmpty(userName) || 
                Request.ContentType != "application/json")
            {
                return BadRequest(
                    "Oops. Make sure your body contains a string with your username and your Content - Type is Content - Type:application / json");

            }

            //TODO check for user existence before adding


            var user = _userRepository.InsertUser(userName);
            await _userRepository.SaveChanges();


            return Ok(user);
        }


        /// <summary>
        /// Deletes a user by username from the database 
        /// </summary>
        /// <param name="userName">The name of the user to delete</param>
        /// <returns></returns>
        //[CustomAuthorise]
        [ActionName("RemoveUser")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] string userName)
        {
            User user = null;
            
            
    
            if (Request.Headers.TryGetValue("ApiKey", out var values))
            {
                var apiKey = values.ToList().FirstOrDefault();

                Guid result;
                if (Guid.TryParse(apiKey, out result))
                {
                    user = _userRepository.GetUser(x => x.UserName == userName && x.ApiKey == apiKey);

                    if (user != null)
                    {
                        await _userRepository.DeleteUser(user);
                        await _userRepository.SaveChanges();
                    }
                }
            }


            
            return Ok(user != null);
        }

        protected override void Dispose(bool disposing)
        {

            _userRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}