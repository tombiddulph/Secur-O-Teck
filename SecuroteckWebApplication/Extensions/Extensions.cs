using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace SecuroteckWebApplication.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Gets and ApiKey from the headers of a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The ApiKey from the request or and empty string if the key doesn't exist</returns>
        public static string GetApiKey(this HttpRequestMessage request)
        {

            IEnumerable<string> values;
            if (request.Headers.TryGetValues("ApiKey", out values))
            {
                Guid id;
                var key = values.ToList().First();
                if (Guid.TryParse(key, out id))
                {
                    return key;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="remove">True to remove hyphens</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(this byte[] data, bool remove)
        {
            string result = BitConverter.ToString(data);

            if (remove)
            {
                result = result.Replace("-", string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(this byte[] data)
        {
            return data.ByteArrayToHexString(false);
        }

        public static HttpResponseMessage CreateOkStringResponse(this HttpRequestMessage request, string data)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(data, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
