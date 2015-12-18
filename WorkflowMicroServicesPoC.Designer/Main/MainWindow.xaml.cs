using Microsoft.Win32;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace WorkflowMicroServicesPoC.Designer
{
    /// <summary>
    /// Main Window for activity designer
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            this.DataContext = viewModel;

            // Register the metadata
            RegisterMetadata();

        }
        
        private void RegisterMetadata()
        {
            DesignerMetadata dm = new DesignerMetadata();
            dm.Register();
        }
        
        private void cmdRun_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RunClicked();
        }

        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.NewClicked();
        }

        private void cmdLoad_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadClicked();
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveClicked();
        }
    }
}
