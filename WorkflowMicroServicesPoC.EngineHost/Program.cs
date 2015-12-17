using System;
using System.Activities;
using System.Activities.Presentation.View;
using System.Activities.XamlIntegration;
using System.IO;
using System.Text;
using System.Threading;
using System.Xaml;

namespace WorkflowMicroServicesPoC.EngineHost
{
    /// <summary>
    /// A host for running workflows to test the composite activties
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {

            string fileName = args[0];

            var waitHandle = new AutoResetEvent(false);

            var activty = LoadWorkflow(fileName);

            var wa = new WorkflowApplication(activty);
            wa.Completed = (e) =>
            {
                waitHandle.Set();
            };
            wa.Run();

            waitHandle.WaitOne();

            Console.WriteLine("Done");

            Console.ReadKey();

        }

        /// <summary>
        /// load worklfow definition
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static Activity LoadWorkflow(String fileName)
        {

            Activity workflow;

            String xamlData = string.Empty;
            using (var sr = new StreamReader(fileName))
            {
                xamlData = sr.ReadToEnd();
            }
            
            Byte[] byteArray = Encoding.ASCII.GetBytes(xamlData);
            MemoryStream memoryStream = new MemoryStream(byteArray);
            XamlXmlReaderSettings settings = GetXamlXmlReaderSettings();
            XamlReader reader = new XamlXmlReader(memoryStream, settings);
            workflow = ActivityXamlServices.Load(reader);

            return workflow;
        }

        /// <summary>
        /// used to ensure the presentation refs are resolved correctly
        /// </summary>
        /// <returns></returns>
        private static XamlXmlReaderSettings GetXamlXmlReaderSettings()
        {
            XamlXmlReaderSettings result = new XamlXmlReaderSettings();
            result.LocalAssembly = typeof(VirtualizedContainerService).Assembly;
            return result;
        }

    }
}
