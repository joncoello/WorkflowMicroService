using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using WorkflowMicroServicesPoC.Interfaces;

namespace WorkflowMicroServicesPoC.Plugin1
{
    public class MyPluginsController : ApiController, IPlugin
    {
        public string Get()
        {
            return "test";
        }
    }
}
