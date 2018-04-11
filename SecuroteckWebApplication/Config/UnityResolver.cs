using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Exceptions;

namespace SecuroteckWebApplication.Config
{
    public class UnityResolver : IDependencyResolver
    {

        private readonly IUnityContainer _container;
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>
        {

        };


        public UnityResolver(IUnityContainer container)
        {
            _container = container;

        }





        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException e)
            {

                Console.WriteLine(e);
                return null;
                throw;
            }
        }

        public T GetService<T>()
        {
            try
            {
                return (T)_container.Resolve(typeof(T));
            }
            catch (ResolutionFailedException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (Exception e)
            {
                return Enumerable.Empty<object>();
                throw;
            }
        }

        public IDependencyScope BeginScope()
        {
            return new UnityResolver(_container.CreateChildContainer());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            _container.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
