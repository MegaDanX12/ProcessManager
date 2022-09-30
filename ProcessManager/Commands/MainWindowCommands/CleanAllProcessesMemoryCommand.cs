using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class CleanAllProcessesMemoryCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;

        public CleanAllProcessesMemoryCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (!VM.EmptyAllProcessesWorkingSet())
            {
                _ = MessageBox.Show(Properties.Resources.AllProcessesEmptyWorkingSetErrorMessage, Properties.Resources.AllProcessesEmptyWorkingSetErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}