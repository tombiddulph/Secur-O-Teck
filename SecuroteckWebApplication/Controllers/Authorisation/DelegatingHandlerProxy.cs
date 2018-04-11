using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SecuroteckWebApplication.Extensions;
using Unity;

namespace SecuroteckWebApplication.Controllers.Authorisation
{
    public class DelegatingHandlerProxy<T> : DelegatingHandler where T : DelegatingHandler
    {
        private readonly UnityContainer _containter;

        public DelegatingHandlerProxy(UnityContainer containter)
        {
            this._containter = containter;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)

        {

            Trace.WriteLine(await request.Content.ReadAsStringAsync());
            request.GetDependencyScope();
            var handler = this._containter.Resolve<T>();
            handler.InnerHandler = this.InnerHandler;

            var invoked = new HttpMessageInvoker(handler);


            var key = request.GetApiKey();
            if (!string.IsNullOrEmpty(key))
            {
                Trace.WriteLine(key);
            }

            var response = await invoked.SendAsync(request, cancellationToken);

            return response;
        }

    }
}
