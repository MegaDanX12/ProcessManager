using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class FindWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;

        public FindWindowCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MainWindow MainWindow = parameter as MainWindow;
            FindWindowInfoWindow Window = new();
            if (Window.ShowDialog().Value)
            {
                ProcessInfo Info = VM.ActiveProcessesInfo.FirstOrDefault(info => info.PID == Window.PID);
                if (Info is not null)
                {
                    VM.WindowOwner = Info;
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.WindowOwnerNotFoundErrorMessage, Properties.Resources.WindowOwnerNotFoundErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}