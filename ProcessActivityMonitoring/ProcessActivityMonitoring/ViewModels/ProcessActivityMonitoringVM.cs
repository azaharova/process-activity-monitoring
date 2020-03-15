using ProcessActivityMonitoring.Helpers;
using ProcessActivityMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessActivityMonitoring.ViewModels
{
    public class ProcessActivityMonitoringVM : NotifyPropertyChangedHelper
    {
        #region Properties

        public ProcessManager Model { get; }

        public ObservableCollection<ProcessData> Processes
        {
            get { return Model.Processes; }
            set { Model.Processes = value; }
        }

        #endregion

        #region .ctors
        public ProcessActivityMonitoringVM() : this(new ProcessManager()) { }

        public ProcessActivityMonitoringVM(ProcessManager model)
        {
            Model = model;
            Model.PropertyChanged += (s, e) =>
            {
                NotifyPropertyChanged(e.PropertyName);
            };
        }
        #endregion
    }
}
