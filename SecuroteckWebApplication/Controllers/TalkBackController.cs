using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{

    public class TalkBackController : ApiController
    {

        [ActionName("Hello")]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World");
        }




        [ActionName("Sort")]
     
        public HttpResponseMessage Get([FromUri] string[] integers)
        {

            var result = new List<int>();
            foreach (var integer in integers)
            {
                int converted;
                if (int.TryParse(integer, out converted))
                {
                    result.Add(converted);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, result.OrderBy(x => x));
        }



    }


}
