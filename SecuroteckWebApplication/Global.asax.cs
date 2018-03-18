using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using SecuroteckWebApplication.Models;
using Unity;
using Unity.Lifetime;

namespace SecuroteckWebApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

          

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
