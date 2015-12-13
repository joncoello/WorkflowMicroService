using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Castle.Windsor;
using System.Linq;

namespace WorkflowMicroServicesPoC
{
    internal class WindsorResolver : IDependencyResolver
    {
        private WindsorContainer container;

        public WindsorResolver(WindsorContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return container.Resolve(serviceType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var returnValue = new List<object>();
            try
            {
                var services = container.ResolveAll(serviceType);
                foreach (object service in services)
                {
                    returnValue.Add(service);
                }
            }
            catch (Exception){}
            return returnValue;
        }

        public IDependencyScope BeginScope()
        {
            var child = new WindsorContainer();
            container.AddChildContainer(child);
            return new WindsorResolver(child);
            //return this;
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}