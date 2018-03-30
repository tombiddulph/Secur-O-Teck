using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SecuroteckWebApplicationCore.DataAccess;
using SecuroteckWebApplicationCore.Extensions;

namespace SecuroteckWebApplicationCore.Controllers
{

    //[CustomAuthorise]
    public class ProtectedController : Controller
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

        public IActionResult Sha1([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {

                return BadRequest("Bad Request");

            }
            //return Request.CreateResponse(HttpStatusCode.OK, _sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message)).ByteArrayToHexString(true));

            return Ok(_sha1Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))
                .ByteArrayToHexString(true));
        }

        [ActionName("sha256"), HttpGet]

        public IActionResult Sha256([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest( "Bad Request");
            }



            return Ok(_sha256Crypto.ComputeHash(Encoding.ASCII.GetBytes(message))
                .ByteArrayToHexString(true));


        }


        [ActionName("Hello")]
        public IActionResult Get()
        {

            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return BadRequest();
            }

            var user = _userRepository.GetUser(x => x.ApiKey == key);

            if (user == null)
            {
                return BadRequest();
            }

            return Ok( $"Hello {user.UserName}");
        }

        [ActionName("GetPublicKey")]
        public IActionResult GetPublicKey()
        {
            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return BadRequest();
            }





            return Ok(_rsaCrypto.ToXmlString(false));
        }

        [ActionName("Sign"), HttpGet]
        public IActionResult Sign([FromQuery] string message)
        {
            string key = Request.GetApiKey();

            if (string.IsNullOrEmpty(key))
            {
                return BadRequest();
            }




            var hash = _sha1Crypto.ComputeHash(Encoding.UTF8.GetBytes(message));


            var item = _rsaCrypto.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));





            return Ok(BitConverter.ToString(item));

        }

        [ActionName("AddFifty"), HttpGet]
        public IActionResult AddFifty([FromQuery]string encryptedInteger, [FromQuery] string encryptedsymkey, [FromQuery] string encryptedIV)
        {

            string apiKey = Request.GetApiKey();

            if (string.IsNullOrEmpty(encryptedInteger) || string.IsNullOrEmpty(encryptedsymkey) || string.IsNullOrEmpty(encryptedIV) || string.IsNullOrEmpty(apiKey))

            {
                return BadRequest();
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



            return Ok(BitConverter.ToString(resultBytes));
       
          
        }

        protected override void Dispose(bool disposing)
        {
            this._userRepository.Dispose();
            base.Dispose(disposing);
        }


        private static byte[] FromHexString(string input) => input.Split('-').Select(value => Convert.ToByte(value, 16)).ToArray();

    }
}
