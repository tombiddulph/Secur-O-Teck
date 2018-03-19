using System;
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
        [ResponseType(typeof(int[]))]
        public HttpResponseMessage Get([FromUri] int[] integers)
        {

            int[] result;

            if (integers.Length > 1)
            {
                result = integers.OrderBy(x => x).ToArray();
            }
            else
            {
                result = new int[0];
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }



    }


}
