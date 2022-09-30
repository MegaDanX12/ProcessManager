using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.ViewModels
{
    public class ChildrenProcessesListVM
    {
        /// <summary>
        /// Informazioni sui processi.
        /// </summary>
        public ObservableCollection<ProcessInfo> ChildrenProcessesInfo { get; }

        public ChildrenProcessesListVM(List<ProcessInfo> ChildrenProcesses)
        {
            ChildrenProcessesInfo = new ObservableCollection<ProcessInfo>(ChildrenProcesses);
        }
    }
}