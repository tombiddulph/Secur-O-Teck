using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using SecuroteckWebApplication.DataAccess;
using SecuroteckWebApplication.Extensions;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers.Authorisation
{
    public class ApiAuthorisationHandler : DelegatingHandler
    {

        private readonly IUserRepository _userRepository;

        public ApiAuthorisationHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then authorise the principle on the current thread using a claim, claimidentity and claimsprinciple
            #endregion

            IEnumerable<string> values;
            if (request.Headers.TryGetValues("ApiKey", out values))
            {
                values = values.ToList();

                var item = values.ElementAt(0);

                Guid result;
                if (Guid.TryParse(item, out result))
                {
                    var user = _userRepository.GetUser(x => x.ApiKey == item);
                    if (user != null)
                    {

                        //user.Logs.Add(request.AuthorizationLog());


                        user.Logs.Add(request.AuthorizationLog());


                        request.AuthorizationLog(user);

                        _userRepository.SaveChanges();
                        ClaimsPrincipal current = new ClaimsPrincipal();
                        current.AddIdentity(new ClaimsIdentity(new[]
                            {
                                new Claim(nameof(User.UserName), user.UserName),
                                new Claim(nameof(User.ApiKey), user.ApiKey)
                            },
                            "ApiKey"));


                        Thread.CurrentPrincipal = current;

                    }


                }
            }


            return base.SendAsync(request, cancellationToken);
        }
    }
}