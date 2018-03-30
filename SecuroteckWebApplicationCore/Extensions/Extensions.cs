using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecuroteckWebApplicationCore.Models;

namespace SecuroteckWebApplicationCore.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Gets and ApiKey from the headers of a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The ApiKey from the request or and empty string if the key doesn't exist</returns>
        public static string GetApiKey(this HttpRequest request)
        {

        
            if (request.Headers.TryGetValue("ApiKey", out var  values))
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

        public static IActionResult CreateOkStringResponse(this HttpRequest request, string data)
        {
            
           return new OkObjectResult(new StringContent(data, Encoding.UTF8, "application/json"));
        }

        public static Log AuthorizationLog(this HttpRequestMessage request)
        {
            return new Log
            {
                LogDateTime = DateTime.Now,
                LogString = $"User requested {request.RequestUri.PathAndQuery}"
            };
        }

        public static void AuthorizationLog(this HttpRequestMessage request, User user)
        {
            user.Logs.Add(new Log
            {
                LogDateTime = DateTime.Now,
                LogString = $"User requested {request.RequestUri.PathAndQuery}"
            });
        }
    }
}
