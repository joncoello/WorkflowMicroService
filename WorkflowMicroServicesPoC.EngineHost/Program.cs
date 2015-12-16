using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WorkflowMicroServicesPoC.EngineHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"C:\Users\jon.coello\Desktop\test2.xaml";

            var waitHandle = new AutoResetEvent(false);

            var workflow = ActivityXamlServices.Load(fileName);
            var wa = new WorkflowApplication(workflow);
            wa.Completed = (e) =>
            {
                waitHandle.Set();
            };
            wa.Run();

            waitHandle.WaitOne();

            Console.WriteLine("Done");

            Console.ReadKey();

        }
    }
}
