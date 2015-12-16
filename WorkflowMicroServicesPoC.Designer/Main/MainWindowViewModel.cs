using Microsoft.CSharp;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xaml;
using WorkflowMicroServicesPoC.ActivityLibrary;

namespace WorkflowMicroServicesPoC.Designer
{
    public class MainWindowViewModel
    {
        public string Title { get; private set; }
        public ToolboxControl Toolbox { get; private set; }

        public MainWindowViewModel()
        {
            this.Title = "Activity Designer";

            AddToolBox();
        }

        #region toolbox

        private void LoadCustomActivities()
        {

            Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "customactivity.*.dll").ToList().ForEach(f => File.Delete(f));

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Activities");
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var wd = new WorkflowDesigner();
                wd.Load(file);

                var def = wd.Context.Services.GetService<ModelService>().Root.GetCurrentValue();

                (def as ActivityBuilder).Name = Path.GetFileNameWithoutExtension(file);
                var sb = new StringBuilder();
                var xamlWriter = ActivityXamlServices.CreateBuilderWriter(new IgnorableXamlXmlWriter(new StringWriter(sb), new XamlSchemaContext()));
                XamlServices.Save(xamlWriter, def);

                var xaml = sb.ToString();
                var description = Path.GetFileNameWithoutExtension(file);
                //var fileName = Path.Combine(Path.GetTempPath(), string.Format("{0}.dll", Guid.NewGuid().ToString()));

                var result = this.CompileAssembly(xaml, description, description, file);

                foreach (CompilerError error in result.Errors)
                {
                    System.Windows.Forms.MessageBox.Show(error.ErrorText);
                }

            }


        }

        private CompilerResults CompileAssembly(string xaml, string description, string activityName, string fileName)
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

            fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customactivity." + Path.GetFileNameWithoutExtension(fileName) + ".dll");

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

        private void AddToolBox()
        {
            var tc = GetToolboxControl();

            AddCustomToolboxItems(tc);

            Toolbox = tc;
        }

        private ToolboxControl GetToolboxControl()
        {
            // Create the ToolBoxControl.
            ToolboxControl ctrl = new ToolboxControl();

            var categories = new List<ToolboxCategory>
            {
                new ToolboxCategory("ControlFlow")
                {
                    new ToolboxItemWrapper(typeof(DoWhile), "DoWhile", "DoWhile"),
                    new ToolboxItemWrapper(typeof(ForEach<>), "ForEach", "ForEach"),
                    new ToolboxItemWrapper(typeof(If), "If", "If"),
                    new ToolboxItemWrapper(typeof(Parallel), "Parallel", "Parallel"),
                    new ToolboxItemWrapper(typeof(ParallelForEach<>), "ParallelForEach", "ParallelForEach<T>"),
                    new ToolboxItemWrapper(typeof(Pick), "Pick", "Pick"),
                    new ToolboxItemWrapper(typeof(PickBranch), "PickBranch", "PickBranch"),
                    new ToolboxItemWrapper(typeof(Sequence), "Sequence", "Sequence"),
                    new ToolboxItemWrapper(typeof(Switch<>), "Switch", "Switch<T>"),
                    new ToolboxItemWrapper(typeof(While), "While", "While")
                },
                new ToolboxCategory("Flowchart")
                {
                    new ToolboxItemWrapper(typeof(Flowchart), "Flowchart", "Flowchart"),
                    new ToolboxItemWrapper(typeof(FlowSwitch<>), "FlowSwitch", "FlowSwitch<T>"),
                    new ToolboxItemWrapper(typeof(FlowDecision), "FlowDecision", "FlowDecision")
                },
                //new ToolboxCategory("Runtime")
                //{
                //    new ToolboxItemWrapper(typeof(Persist), "Persist", "Persist"),
                //    new ToolboxItemWrapper(typeof(TerminateWorkflow), "TerminateWorkflow", "TerminateWorkflow")
                //},
                new ToolboxCategory("Primitives")
                {
                    new ToolboxItemWrapper(typeof(Assign), "Assign", "Assign"),
                    new ToolboxItemWrapper(typeof(Assign<>), "Assign", "Assign<T>"),
                    new ToolboxItemWrapper(typeof(Delay), "Delay", "Delay"),
                    new ToolboxItemWrapper(typeof(InvokeMethod), "InvokeMethod", "InvokeMethod"),
                    new ToolboxItemWrapper(typeof(WriteLine), "WriteLine", "WriteLine")
                },
                //new ToolboxCategory("Transaction")
                //{
                //    new ToolboxItemWrapper(typeof(CancellationScope), "CancellationScope", "CancellationScope"),
                //    new ToolboxItemWrapper(typeof(CompensableActivity), "CompensableActivity", "CompensableActivity"),
                //    new ToolboxItemWrapper(typeof(Compensate), "Compensate", "Compensate"),
                //    new ToolboxItemWrapper(typeof(Confirm), "Confirm", "Confirm"),
                //    new ToolboxItemWrapper(typeof(TransactionScope), "TransactionScope", "TransactionScope")
                //},
                new ToolboxCategory("Collection")
                {
                    new ToolboxItemWrapper(typeof(AddToCollection<>), "AddToCollection", "AddToCollection<T>"),
                    new ToolboxItemWrapper(typeof(ClearCollection<>), "ClearCollection", "ClearCollection<T>"),
                    new ToolboxItemWrapper(typeof(ExistsInCollection<>), "ExistsInCollection", "ExistsInCollection<T>"),
                    new ToolboxItemWrapper(typeof(RemoveFromCollection<>), "RemoveFromCollection", "RemoveFromCollection<T>")
                },
                new ToolboxCategory("ErrorHandling")
                {
                    new ToolboxItemWrapper(typeof(Rethrow), "Rethrow", "Rethrow"),
                    new ToolboxItemWrapper(typeof(Throw), "Throw", "Throw"),
                    new ToolboxItemWrapper(typeof(TryCatch), "TryCatch", "TryCatch")
                },
                new ToolboxCategory("CCH")
                {
                    new ToolboxItemWrapper(typeof(CreateClient), "CreateClient", "CreateClient"),
                    new ToolboxItemWrapper(typeof(CreateTask), "CreateTask", "CreateTask")
                }
            };

            var activityTypes = new List<ToolboxItemWrapper>();
            categories.ForEach(cat => cat.Tools.ToList().ForEach(activityTypes.Add));
            ToolboxIconCreator.LoadToolboxIcons(activityTypes, this.GetIconReaders().ToList());
            categories.ForEach(cat => ctrl.Categories.Add(cat));

            return ctrl;
        }

        private IEnumerable<ResourceReader> GetIconReaders()
        {
            //if (this._resourceReaderList != null) return this._resourceReaderList;

            var resourceReaderList = new List<ResourceReader>();

            var requiredFileList = new List<string>
            {
                "Microsoft.VisualStudio.Activities"
            };

            foreach (var item in requiredFileList)
            {
                var file = string.Format("{0}.dll", item);
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                var resource = string.Format("{0}.Resources.resources", item);

                try
                {
                    var assembly = Assembly.LoadFile(path);

                    var stream = assembly.GetManifestResourceStream(resource);

                    if (stream == null)
                    {
                        var message = string.Format("Resource '{0}' in File '{1}' not found!", resource, path);
                        throw new InvalidOperationException(message);
                    }

                    resourceReaderList.Add(new ResourceReader(stream));
                }
                catch (FileNotFoundException fnfEx)
                {
                    var message = string.Format("File '{0}' not found!", path);
                    throw new FileNotFoundException(message, file, fnfEx);
                }
            }

            return resourceReaderList;
        }

        private void AddCustomToolboxItems(ToolboxControl tc)
        {
            var cat = new ToolboxCategory("Custom Activities");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customactivities");
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "customactivity.*.dll");

            foreach (var item in GetAllCompiledActivities(files))
            {
                cat.Tools.Add(new ToolboxItemWrapper(item.Value, item.Key));
            }
            tc.Categories.Add(cat);
        }

        private IDictionary<string, Type> GetAllCompiledActivities(string[] files)
        {
            var activities = new Dictionary<string, Type>();

            foreach (var item in files)
            {
                var fileName = Path.GetTempFileName();
                File.Copy(item, fileName, true);
                var asm = Assembly.LoadFrom(fileName);
                try
                {
                    var types = asm.GetTypes();

                    foreach (var type in types)
                    {
                        if (type != null ? typeof(IActivityTemplateFactory).IsAssignableFrom(type) : false)
                        {
                            string name = type.Assembly.GetName().Name.Replace("customactivity.", "");
                            var instance = ((IActivityTemplateFactory)Activator.CreateInstance(type));
                            activities.Add(name, type);
                        }
                    }
                }
                catch (Exception) { }

            }

            return activities;
        } 
        #endregion

    }
}
