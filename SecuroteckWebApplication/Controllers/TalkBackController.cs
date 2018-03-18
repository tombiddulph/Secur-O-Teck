using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

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
            if (integers.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            return Request.CreateResponse(HttpStatusCode.OK, integers.OrderBy(x => x).ToArray());
        }

    }
}
