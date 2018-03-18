using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

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
    }
}
