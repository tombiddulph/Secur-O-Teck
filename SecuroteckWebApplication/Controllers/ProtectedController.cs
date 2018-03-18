using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.Extensions;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{

    [CustomAuthorise]
    public class ProtectedController : ApiController
    {

        private readonly IUserRepository _userRepository;
        private readonly SHA1CryptoServiceProvider _sha1Crypto;
        private readonly SHA256CryptoServiceProvider _sha256Crypto;


        public ProtectedController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            this._sha1Crypto = new SHA1CryptoServiceProvider();
            this._sha256Crypto = new SHA256CryptoServiceProvider();
        }

        [ActionName("sha1")]

        public HttpResponseMessage Sha1([FromUri] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }
         
            

            var returnMessage = BitConverter.ToString(_sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))).Replace("-", string.Empty);


            return Request.CreateResponse(HttpStatusCode.OK, returnMessage);

        }

        [ActionName("sha256")]

        public HttpResponseMessage Sha256([FromUri] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }



            var returnMessage = BitConverter.ToString(_sha256Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))).Replace("-", string.Empty);


            return Request.CreateResponse(HttpStatusCode.OK, returnMessage);

        }


        [ActionName("Hello")]
        public HttpResponseMessage Get()
        {

            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var user = _userRepository.GetUser(x => x.ApiKey == key);

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK, $"Hello {user.UserName}");
        }
    }
}
