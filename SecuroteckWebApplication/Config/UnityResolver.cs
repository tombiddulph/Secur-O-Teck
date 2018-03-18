﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using Unity;
using Unity.Exceptions;

namespace SecuroteckWebApplication.Config
{
    public class UnityResolver : IDependencyResolver
    {

        private readonly IUnityContainer _container;


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
            return new UnityResolver(this._container.CreateChildContainer());
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            this._container.Dispose();
        }
    }
}
