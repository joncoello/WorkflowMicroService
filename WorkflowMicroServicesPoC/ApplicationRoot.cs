using System;
using System.Linq;
using System.ComponentModel.Composition.Hosting;
using WorkflowMicroServicesPoC.Interfaces;
using System.Reflection;
using System.Collections.Generic;

namespace WorkflowMicroServicesPoC
{
    internal class ApplicationRoot
    {
        private readonly ILogger _logger;

        public ApplicationRoot(ILogger logger)
        {
            _logger = logger;
            logger.Log("application root created");
        }

        internal IEnumerable<Assembly> Run()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            var plugins = container.GetExports<IPlugin>().ToList().Select(l=>l.Value).ToList();
            plugins.ForEach(p => Console.WriteLine(p.GetType().FullName));

            return plugins.Select(p => p.GetType().Assembly).Distinct();
            
        }

    }

}