using System.Web;
using Newtonsoft.Json;
using static System.Web.Http.GlobalConfiguration;

namespace SecuroteckWebApplication
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Configure(WebApiConfig.Register);
            Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                ReferenceLoopHandling.Ignore;

            Configuration.Formatters.Remove(Configuration.Formatters
                .XmlFormatter);
        }
    }
}
