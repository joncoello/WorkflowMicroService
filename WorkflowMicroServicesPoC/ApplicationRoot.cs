using System;
using System.Linq;
using System.ComponentModel.Composition.Hosting;
using WorkflowMicroServicesPoC.Interfaces;

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

        internal void Run()
        {
            
            
        }

    }

}