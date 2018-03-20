using System;
using System.Security.Cryptography;
using System.Web.Http;
using SecuroteckWebApplication.Config;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.DataAccess;
using Unity;
using Unity.Lifetime;

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

            containter.RegisterType<IUserRepository, UserRepository>(new HierarchicalLifetimeManager());

            config.DependencyResolver = new UnityResolver(containter);

            // Web API configuration and services
            GlobalConfiguration.Configuration.MessageHandlers.Add(new ApiAuthorisationHandler(containter.Resolve(typeof(IUserRepository)) as IUserRepository));








            #region Task 7
            // Configuration for Task 9
            #endregion

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
