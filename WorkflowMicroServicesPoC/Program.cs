using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using WorkflowMicroServicesPoC.Controllers;
using WorkflowMicroServicesPoC.Interfaces;

namespace WorkflowMicroServicesPoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            container.Install(FromAssembly.This());

            container.Register(AllTypes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());

            container.Register(Component.For<ApplicationRoot>());
            container.Register(Component.For<ILogger>().ImplementedBy<Logger>());
            
            var applicationRoot = container.Resolve<ApplicationRoot>();
            applicationRoot.Run();

            // web api self host
            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            //config.DependencyResolver = new WindsorResolver(container);

            config.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(container));

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
            
        }

    }

}
