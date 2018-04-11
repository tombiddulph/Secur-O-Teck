using System;
using System.Linq;
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


            //return Request.CreateResponse(HttpStatusCode.OK, _sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message)).ByteArrayToHexString(true));

            return Request.CreateOkStringResponse(_sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))
                .ByteArrayToHexString(true));
        }

        [ActionName("sha256"), HttpGet]

        public HttpResponseMessage Sha256([FromUri] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
            }



            return Request.CreateOkStringResponse(_sha256Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))
                .ByteArrayToHexString(true));


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





            return Request.CreateOkStringResponse(_rsaCrypto.ToXmlString(false));
        }

        [ActionName("Sign"), HttpGet]
        public HttpResponseMessage Sign([FromUri] string message)
        {
            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }




            var hash = _sha1Crypto.ComputeHash(Encoding.UTF8.GetBytes(message));


            var item = _rsaCrypto.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));





            return Request.CreateOkStringResponse(BitConverter.ToString(item));

        }

        [ActionName("AddFifty"), HttpGet]
        public HttpResponseMessage AddFifty([FromUri]string encryptedInteger, [FromUri] string encryptedsymkey, [FromUri] string encryptedIV)
        {

            string apiKey = Request.GetApiKey();

            if (string.IsNullOrEmpty(encryptedInteger) || string.IsNullOrEmpty(encryptedsymkey) || string.IsNullOrEmpty(encryptedIV) || string.IsNullOrEmpty(apiKey))

            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var encryptedBytes = FromHexString(encryptedInteger);
            var keyBytes = _rsaCrypto.Decrypt(FromHexString(encryptedsymkey), true);
            var ivBytes = _rsaCrypto.Decrypt(FromHexString(encryptedIV), true);


            var value = (BitConverter.ToInt32(_rsaCrypto.Decrypt(encryptedBytes, true), 0) + 50);

            var aes = new AesManaged
            {
                Key = keyBytes,
                IV = ivBytes
            };
            var encryptor = aes.CreateEncryptor();

            var valueBytes = BitConverter.GetBytes(value);
            var resultBytes = encryptor.TransformFinalBlock(valueBytes, 0, valueBytes.Length);



            return Request.CreateOkStringResponse(BitConverter.ToString(resultBytes));
       
          
        }

        protected override void Dispose(bool disposing)
        {
            this._userRepository.Dispose();
            base.Dispose(disposing);
        }


        private static byte[] FromHexString(string input) => input.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();

    }
}
