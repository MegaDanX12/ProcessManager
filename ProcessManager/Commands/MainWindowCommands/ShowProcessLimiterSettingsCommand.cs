using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowProcessLimiterSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;

        public ShowProcessLimiterSettingsCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ProcessLimiterSettingsVM SettingsVM = new(VM);
            ProcessLimiterSettingsWindow Window = new(SettingsVM);
            _ = Window.ShowDialog();
        }
    }
}