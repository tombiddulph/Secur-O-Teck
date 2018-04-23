using System.Security.Cryptography;
using System.Web.Http;
using SecuroteckWebApplication.Config;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.DataAccess;
using SecuroteckWebApplication.Models;
using Unity;
using Unity.Injection;
using Unity.WebApi;

namespace SecuroteckWebApplication
{
    public static class WebApiConfig
    {


        public static void Register(HttpConfiguration config)
        {


            var containter = new UnityContainer();
            var rsa = new RSACryptoServiceProvider
            {
                PersistKeyInCsp = false,
                KeySize = 2048
            };
            rsa.FromXmlString(rsa.ToXmlString(true));
            containter.RegisterInstance(rsa);

            containter.RegisterType<IUserRepository>(new InjectionFactory(x => new UserRepository(new UserContext())));
            config.DependencyResolver = new UnityDependencyResolver(containter);
            
            GlobalConfiguration.Configuration.MessageHandlers.Add(new DelegatingHandlerProxy<ApiAuthorisationHandler>(containter));

 

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "TalkbackApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }





    }
}
