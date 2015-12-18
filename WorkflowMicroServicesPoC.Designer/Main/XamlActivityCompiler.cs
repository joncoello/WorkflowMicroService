using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorkflowMicroServicesPoC.Designer.Main
{
    /// <summary>
    /// Compile a xaml activity into a dll for the control toolbox
    /// </summary>
    class XamlActivityCompiler
    {
        public CompilerResults Compile(string xaml, string description, string activityName, string fileName)
        {
            string source;
            using (var sr = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.txt")))
            {
                xaml = xaml != null ? xaml.Replace('"', char.Parse("'")) : null;
                description = description != null ? description.Replace('"', char.Parse("'")) : null;
                activityName = activityName != null ? activityName.Replace('"', char.Parse("'")) : null;

                source = sr.ReadToEnd();
                source = source.Replace("{0}", xaml);
                source = source.Replace("{1}", description);
                source = source.Replace("{2}", activityName);

            }

            fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Compiled", "customactivity." + Path.GetFileNameWithoutExtension(fileName) + ".dll");

            var codeDomProvider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters
            {
                OutputAssembly = fileName,
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = false
            };

            compilerParams.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkflowMicroServicesPoC.Interfaces.dll"));
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            compilerParams.ReferencedAssemblies.Add(@"WPF\WindowsBase.dll");
            compilerParams.ReferencedAssemblies.Add("System.Activities.dll");
            compilerParams.ReferencedAssemblies.Add("System.Activities.Core.Presentation.dll");
            compilerParams.ReferencedAssemblies.Add("System.Activities.Presentation.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xaml.dll");

            return codeDomProvider.CompileAssemblyFromSource(compilerParams, source);

        }

    }

}
