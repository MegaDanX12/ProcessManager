using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class RefreshWindowsDataCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public RefreshWindowsDataCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            VM.UpdateWindowsData();
        }

        public event EventHandler CanExecuteChanged;
    }
}