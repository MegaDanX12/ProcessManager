using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class FindWindowAndTerminateOwnerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;

        public FindWindowAndTerminateOwnerCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            FindWindowInfoWindow Window = new();
            if (Window.ShowDialog().Value)
            {
                ProcessInfo Info = VM.ActiveProcessesInfo.FirstOrDefault(info => info.PID == Window.PID);
                if (Info is not null)
                {
                    if (!Info.TerminateProcess())
                    {
                        _ = MessageBox.Show(Properties.Resources.WindowOwnerTerminationErrorMessage, Properties.Resources.WindowOwnerTerminationErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.WindowOwnerNotFoundErrorMessage, Properties.Resources.WindowOwnerNotFoundErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}