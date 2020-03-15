using ProcessActivityMonitoring.Helpers;
using ProcessActivityMonitoring.Models;
using ProcessActivityMonitoring.ViewModels;
using System;
using System.Windows.Controls;

namespace ProcessActivityMonitoring.View
{
    /// <summary>
    /// Interaction logic for ProcessActivityMonitoringView.xaml
    /// </summary>
    public partial class ProcessActivityMonitoringView : UserControl
    {
        public ProcessActivityMonitoringVM ViewModel { get; }

        #region .ctors

        public ProcessActivityMonitoringView() : this(new ProcessActivityMonitoringVM()) { }

        public ProcessActivityMonitoringView(ProcessManager model) : this(new ProcessActivityMonitoringVM(model)) { }

        public ProcessActivityMonitoringView(ProcessActivityMonitoringVM viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            Helper.SortDataGrid(mainDataGrid);
        }

        #endregion
    }
}
