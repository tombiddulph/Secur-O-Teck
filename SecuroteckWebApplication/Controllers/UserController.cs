﻿using System;
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

        /// <summary>
        /// Checks the database for to see if a user with the specified username has already been created
        /// </summary>
        /// <param name="userName">The name of the user to check</param>
        [ActionName("New"), HttpGet]
        public HttpResponseMessage Get([FromUri]string userName)
        {

            if (string.IsNullOrEmpty(userName))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }

            if (!_userRepository.CheckUser(x => x.UserName == userName))
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    "False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                "True - User Does Exist! Did you mean to do a POST to create a new user?");
        }



        /// <summary>
        /// Creates a new user from the given username
        /// </summary>
        /// <param name="userName">The name of the user to create</param>
        [ActionName("New"), HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] string userName)
        {

            if (string.IsNullOrEmpty(userName) || Request.Content.IsMimeMultipartContent("application/json"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    "Oops. Make sure your body contains a string with your username and your Content - Type is Content - Type:application / json");

            }

            var user = _userRepository.InsertUser(userName);
            await _userRepository.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK, user);
        }


        /// <summary>
        /// Deletes a user by username from the database 
        /// </summary>
        /// <param name="userName">The name of the user to delete</param>
        /// <returns>True if the user has been deleted, otherwise false</returns>
        [HttpDelete, ActionName("RemoveUser"), CustomAuthorise]
        public async Task<HttpResponseMessage> DeleteUser([FromUri] string userName)
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
                        await _userRepository.DeleteUser(user);
                        await _userRepository.SaveChanges();
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, user != null);
        }

        protected override void Dispose(bool disposing)
        {

            _userRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}
