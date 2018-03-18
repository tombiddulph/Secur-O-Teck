using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{
    public class TalkBackController : ApiController
    {
        [ActionName("Hello")]
        public HttpResponseMessage Get()
        {

            UserRepository repository = new UserRepository(null);

            string userName = "";
            Guid apikey = Guid.Empty;
            var test = repository.CheckUser(x => x.UserName == userName && x.ApiKey == apikey.ToString());
        
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
