using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.DataAccess;
using SecuroteckWebApplication.Extensions;

namespace SecuroteckWebApplication.Controllers
{

    [CustomAuthorise]
    public class ProtectedController : ApiController
    {

        private readonly IUserRepository _userRepository;
        private readonly SHA1CryptoServiceProvider _sha1Crypto;
        private readonly SHA256CryptoServiceProvider _sha256Crypto;
        private readonly RSACryptoServiceProvider _rsaCrypto;

        public ProtectedController(RSACryptoServiceProvider rsaCrypto, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _sha1Crypto = new SHA1CryptoServiceProvider();
            _sha256Crypto = new SHA256CryptoServiceProvider();
            _rsaCrypto = rsaCrypto;
        }

        [ActionName("sha1"), HttpGet]

        public HttpResponseMessage Sha1([FromUri] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }



            var returnMessage = BitConverter.ToString(_sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))).Replace("-", string.Empty);


            return Request.CreateResponse(HttpStatusCode.OK, returnMessage);

        }

        [ActionName("sha256"), HttpGet]

        public HttpResponseMessage Sha256([FromUri] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }



            var returnMessage = _sha256Crypto.ComputeHash(Encoding.ASCII.GetBytes(message)).ByteArrayToHexString();


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

        [ActionName("GetPublicKey")]
        public HttpResponseMessage GetPublicKey()
        {
            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }


            return Request.CreateResponse(HttpStatusCode.OK, _rsaCrypto.ToXmlString(false));
        }

        [ActionName("Sign")]
        public HttpResponseMessage Sign([FromUri] string message)
        {
            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }



            var item = _rsaCrypto.SignHash(_sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message)), CryptoConfig.MapNameToOID(HashAlgorithmName.SHA1.Name));

            return Request.CreateResponse(HttpStatusCode.OK, item.ByteArrayToHexString());

        }

        [ActionName("AddFifty"), HttpGet]
        public HttpResponseMessage AddFifty([FromUri]string encrypted, [FromUri] string key, [FromUri] string initializationVector)
        {

            string apiKey = Request.GetApiKey();

            if (string.IsNullOrEmpty(encrypted) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(initializationVector) || string.IsNullOrEmpty(apiKey))

            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var encryptedBytes = Encoding.ASCII.GetBytes(encrypted);
            var keyBytes = Encoding.ASCII.GetBytes(key);
            var ivBytes = Encoding.ASCII.GetBytes(initializationVector);


            var test = _rsaCrypto.Decrypt(encryptedBytes, false);





            return null;
        }

        protected override void Dispose(bool disposing)
        {
            this._userRepository.Dispose();
            base.Dispose(disposing);
        }

    }
}
