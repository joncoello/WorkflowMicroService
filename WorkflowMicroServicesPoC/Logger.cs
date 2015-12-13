using System;
using WorkflowMicroServicesPoC.Interfaces;

namespace WorkflowMicroServicesPoC
{
    internal class Logger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}