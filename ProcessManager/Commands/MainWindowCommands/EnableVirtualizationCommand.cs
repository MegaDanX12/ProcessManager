using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class EnableVirtualizationCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public EnableVirtualizationCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.EnableVirtualization(Parameter as ProcessInfo))
            {
                _ = MessageBox.Show(Properties.Resources.EnableVirtualizationErrorMessage, Properties.Resources.EnableVirtualizationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}