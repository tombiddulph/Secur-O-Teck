using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
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
            return integers.Length == 0
                ? Request.CreateResponse(HttpStatusCode.OK)
                : Request.CreateResponse(HttpStatusCode.OK, integers.OrderBy(x => x).ToArray());
        }

     

    }


}
