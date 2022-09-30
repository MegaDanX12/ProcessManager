using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class SuspendThreadCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public SuspendThreadCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.SuspendThread(Parameter as ThreadInfo))
            {
                _ = MessageBox.Show(Properties.Resources.SuspendThreadErrorMessageText, Properties.Resources.SuspendThreadErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}