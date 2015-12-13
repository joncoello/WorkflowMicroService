using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Web.Http;
using WorkflowMicroServicesPoC.Interfaces;
using WorkflowMicroServicesPoC.Models;

namespace WorkflowMicroServicesPoC.Controllers
{
    public class PluginsController : ApiController
    {

        public IEnumerable<Plugin> Get() {

            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var catalog = new DirectoryCatalog(directory);
            var container = new CompositionContainer(catalog);

            var lazyPlugins = container.GetExports<IPlugin>();

            return lazyPlugins.Select(l => 
                new Plugin()
                {
                    Name = l.Value.GetType().FullName
                }
            );
            
        }

    }
}
