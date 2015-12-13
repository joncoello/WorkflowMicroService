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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            Grid.SetColumn(this.wd.View, 1);

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

            // Create a category.
            ToolboxCategory category = new ToolboxCategory("category1");

            // Create Toolbox items.
            ToolboxItemWrapper tool1 =
                new ToolboxItemWrapper("System.Activities.Statements.Assign",
                typeof(Assign).Assembly.FullName, null, "Assign");

            ToolboxItemWrapper tool2 = new ToolboxItemWrapper("System.Activities.Statements.Sequence",
                typeof(Sequence).Assembly.FullName, null, "Sequence");

            // Add the Toolbox items to the category.
            category.Add(tool1);
            category.Add(tool2);

            // Add the category to the ToolBox control.
            ctrl.Categories.Add(category);
            return ctrl;
        }

        private ListBox GetCustomToolboxControl()
        {

            var listBox = new ListBox();

            listBox.Items.Add(CreateToolBoxItem("Item 1"));
            listBox.Items.Add(CreateToolBoxItem("Item 2"));
            listBox.Items.Add(CreateToolBoxItem("Item 3"));
            
            return listBox;

        }

        private Label CreateToolBoxItem(string text)
        {
            var label = new Label() { Content = text };
            label.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DataObject data = new DataObject(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat, typeof(Sequence).AssemblyQualifiedName);
                    DragDrop.DoDragDrop(this/*ToolboxControlIWrote?*/, data, DragDropEffects.All);
                }
            };
            return label;
        }

        private void AddToolBox()
        {
            //var tc = GetToolboxControl();
            var tc = GetCustomToolboxControl();
            Grid.SetColumn(tc, 0);
            grid1.Children.Add(tc);
        }

        private void AddPropertyInspector()
        {
            Grid.SetColumn(wd.PropertyInspectorView, 2);
            grid1.Children.Add(wd.PropertyInspectorView);
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Xaml File | .xaml";
            sfd.ShowDialog();
            wd.Save(sfd.FileName);
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

            var writer = new StringWriter();
            var workflow = ActivityXamlServices.Load(fileName);
            var wa = new WorkflowApplication(workflow);
            wa.Extensions.Add(writer);
            //wa.Completed = WorkflowCompleted;
            //wa.OnUnhandledException = WorkflowUnhandledException;
            wa.Run();

        }

        private void cmdAddActivity_Click(object sender, RoutedEventArgs e)
        {

            //ToolboxCategory category = new ToolboxCategory("category2");

            //// Create Toolbox items.
            //ToolboxItemWrapper tool1 =
            //    new ToolboxItemWrapper("System.Activities.Statements.Assign",
            //    typeof(Assign).Assembly.FullName, null, "Assign");

            //// Add the Toolbox items to the category.
            //category.Add(tool1);

            //tc.Categories.Add(category);

        }

        private void cmdAddActivity_Drop(object sender, DragEventArgs e)
        {

        }

        private void cmdAddActivity_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void stkMain_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new DataObject(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat, typeof(Sequence).AssemblyQualifiedName);
                DragDrop.DoDragDrop(this/*ToolboxControlIWrote?*/, data, DragDropEffects.All);
            }
        }

    }
}
