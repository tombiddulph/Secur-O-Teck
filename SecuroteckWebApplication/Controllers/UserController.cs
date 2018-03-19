using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.DataAccess;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{
    public class UserController : ApiController
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }


        [ActionName("New")]
        public HttpResponseMessage Get([FromUri]string userName)
        {

            if (string.IsNullOrEmpty(userName))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }




            if (!_userRepository.CheckUser(x => x.UserName.Equals(userName)))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                "True - User Does Exist! Did you mean to do a POST to create a new user");
        }



        /// <summary>
        /// Creates a new user from the given username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [ActionName("New")]
        public async Task<HttpResponseMessage> Post([FromBody] string userName)
        {

            if (string.IsNullOrEmpty(userName) || Request.Content.IsMimeMultipartContent("application/json"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Oops. Make sure your body contains a string with your username and your Content - Type is Content - Type:application / json");

            }

            //TODO check for user existence before adding


            var user = _userRepository.InsertUser(userName);
            await _userRepository.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK, user.ApiKey);
        }


        /// <summary>
        /// Deletes a user by username from the database 
        /// </summary>
        /// <param name="userName">The name of the user to delete</param>
        /// <returns></returns>
        [CustomAuthorise]
        [ActionName("RemoveUser")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteUser([FromBody] string userName)
        {
            User user = null;
            IEnumerable<string> values;
            if (Request.Headers.TryGetValues("ApiKey", out values))
            {
                var apiKey = values.ToList().FirstOrDefault();

                Guid result;
                if (Guid.TryParse(apiKey, out result))
                {
                    user = _userRepository.GetUser(x => x.UserName == userName && x.ApiKey == apiKey);

                    if (user != null)
                    {
                        _userRepository.DeleteUser(user);
                        await _userRepository.SaveChanges();
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, user != null);
        }
    }
}
