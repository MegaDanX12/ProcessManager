using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class EmptyProcessWorkingSetCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public EmptyProcessWorkingSetCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.EmptyProcessWorkingSet(Parameter as ProcessInfo))
            {
                _ = MessageBox.Show(Properties.Resources.EmptyProcessWorkingSetErrorMessage, Properties.Resources.EmptyProcessWorkingSetErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}