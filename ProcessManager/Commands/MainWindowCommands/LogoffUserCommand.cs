using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class LogoffUserCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public LogoffUserCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.LogOffUser())
            {
                _ = MessageBox.Show(Properties.Resources.LogoffErrorTitle, Properties.Resources.LogoffUserErrorMessage, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}