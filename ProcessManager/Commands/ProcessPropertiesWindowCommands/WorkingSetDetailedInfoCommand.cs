using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class WorkingSetDetailedInfoCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public WorkingSetDetailedInfoCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            WorkingSetDetailedInfoWindow Window = new WorkingSetDetailedInfoWindow(VM);
            Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}