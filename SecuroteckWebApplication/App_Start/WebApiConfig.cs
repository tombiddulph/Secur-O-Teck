using System;
using System.Security.Cryptography;
using System.Web.Http;
using SecuroteckWebApplication.Config;
using SecuroteckWebApplication.Controllers.Authorisation;
using SecuroteckWebApplication.Models;
using Unity;
using Unity.Lifetime;

namespace SecuroteckWebApplication
{
    public static class WebApiConfig
    {
        public static string RsaPublicKey;
        public static string RsaPrivateKey;

        public static void Register(HttpConfiguration config)
        {


            var containter = new UnityContainer();

            var rsa = new RSACryptoServiceProvider(GetCspParameters);
            GenerateKeys(rsa);
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

        private static CspParameters GetCspParameters => new CspParameters
        {
            ProviderType = 1,
            Flags = CspProviderFlags.NoPrompt,
            KeyNumber = (int)KeyNumber.Exchange

        };

        private static void GenerateKeys(RSACryptoServiceProvider rng)
        {


            try
            {
                RsaPublicKey = rng.ToXmlString(false);
                RsaPrivateKey = rng.ToXmlString(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


    }
}
