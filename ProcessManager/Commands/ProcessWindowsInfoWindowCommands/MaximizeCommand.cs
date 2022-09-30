using ProcessManager.ViewModels;
using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class MaximizeCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public MaximizeCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.MaximizeWindow(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowMaximizeErrorMessage, Properties.Resources.WindowMaximizeErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}