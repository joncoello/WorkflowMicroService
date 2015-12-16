using Microsoft.Win32;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Activities.Presentation.Services;
using System.Text;
using System.Xaml;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace WorkflowMicroServicesPoC.Designer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WorkflowDesigner wd;

        public MainWindow()
        {
            InitializeComponent();

            // Register the metadata
            RegisterMetadata();

            // Add the WFF Designer
            AddDesigner(true);

            AddToolBox();

            AddPropertyInspector();

        }

        private void AddDesigner(bool loadBlank)
        {
            //Create an instance of WorkflowDesigner class.
            this.wd = new WorkflowDesigner();

            //Place the designer canvas in the middle column of the grid.
            Grid.SetColumn(this.wd.View, 2);

            //Load a new Sequence as default.
            if (loadBlank)
            {

                ActivityBuilder activityBuilderType = new ActivityBuilder();
                activityBuilderType.Name = "Activity Builder";
                activityBuilderType.Implementation = new Flowchart()
                {
                    DisplayName = "Default Template"
                };

                this.wd.Load(activityBuilderType);
            }

            //Add the designer canvas to the grid.bo
            grid1.Children.Add(this.wd.View);
        }

        private void RegisterMetadata()
        {
            DesignerMetadata dm = new DesignerMetadata();
            dm.Register();
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

        private ListBox GetCustomToolboxControl()
        {

            var listBox = new ListBox();

            string filename = @"C:\Users\jon.coello\Desktop\test.xaml";
            var activity1 = ActivityXamlServices.Load(filename);
            var activity2 = new Sequence();


            listBox.Items.Add(CreateToolBoxItem("Test", activity1));
            listBox.Items.Add(CreateToolBoxItem("Sequence", activity2));

            return listBox;

        }

        private Label CreateToolBoxItem(string text, Activity activity)
        {
            var label = new Label() { Content = text };
            label.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    if (activity.GetType() == typeof(DynamicActivity))
                    {

                        string filename = @"C:\Users\jon.coello\Desktop\test.xaml";
                        var activity1 = ActivityXamlServices.Load(filename);
                        ModelItem mi = wd.Context.Services.GetService<ModelTreeManager>().CreateModelItem(null, activity1);
                        DataObject data = new DataObject(DragDropHelper.ModelItemDataFormat, mi);
                        DragDrop.DoDragDrop((Label)s, data, DragDropEffects.Copy);

                    }
                    else
                    {

                        //Activity activity = (Activity)(s as Label).Tag;
                        DataObject data = new DataObject(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat, activity.GetType().AssemblyQualifiedName);
                        DragDrop.DoDragDrop(this/*ToolboxControlIWrote?*/, data, DragDropEffects.All);

                    }

                }
            };
            return label;
        }

        private void AddToolBox()
        {
            var tc = GetToolboxControl();
            //var tc = GetCustomToolboxControl();
            Grid.SetColumn(tc, 1);

            AddCustomToolboxItems(tc);

            grid1.Children.Add(tc);
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

        public IDictionary<string, Type> GetAllCompiledActivities(string[] files)
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

        private void AddPropertyInspector()
        {
            Grid.SetColumn(wd.PropertyInspectorView, 3);
            grid1.Children.Add(wd.PropertyInspectorView);
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {

            var sfd = new SaveFileDialog();
            sfd.Filter = "Xaml File | .xaml";
            sfd.ShowDialog();
            string fileName = sfd.FileName;

            var def = wd.Context.Services.GetService<ModelService>().Root.GetCurrentValue();
            var sb = new StringBuilder();
            var xamlWriter = ActivityXamlServices.CreateBuilderWriter(new IgnorableXamlXmlWriter(new StringWriter(sb), new XamlSchemaContext()));
            XamlServices.Save(xamlWriter, def);

            var xaml = sb.ToString();
            var description = Path.GetFileNameWithoutExtension(fileName);
            //var fileName = Path.Combine(Path.GetTempPath(), string.Format("{0}.dll", Guid.NewGuid().ToString()));

            var result = this.CompileAssembly(xaml, description, description, fileName);

            //foreach (CompilerError error in result.Errors)
            //{
            //    System.Windows.Forms.MessageBox.Show(error.ErrorText);
            //}

            wd.Save(sfd.FileName);

            using (var sw = new StreamWriter(sfd.FileName))
            {
                sw.Write(wd.Text);
            }

            System.Windows.Forms.MessageBox.Show("Done");
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

        private void cmdLoad_Click(object sender, RoutedEventArgs e)
        {
            AddDesigner(false);
            var ofd = new OpenFileDialog();
            ofd.Filter = "Xaml File | .xaml";
            ofd.ShowDialog();
            wd.Load(ofd.FileName);
        }

        private void cmdRun_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Xaml File | .xaml";
            ofd.ShowDialog();

            string fileName = ofd.FileName;

            //fileName = "customactivity." + Path.GetFileNameWithoutExtension(fileName) + ".dll";

            Process.Start("WorkflowMicroServicesPoC.EngineHost.exe", fileName);
        }

        private UnhandledExceptionAction WorkflowUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs arg)
        {
            MessageBox.Show(arg.UnhandledException.ToString());
            return UnhandledExceptionAction.Abort;
        }

        private void WorkflowCompleted(WorkflowApplicationCompletedEventArgs obj)
        {
            MessageBox.Show("Done");
        }

        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            AddDesigner(true);
        }
    }
}
