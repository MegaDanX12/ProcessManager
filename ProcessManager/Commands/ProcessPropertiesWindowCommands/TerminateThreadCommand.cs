using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class TerminateThreadCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public TerminateThreadCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.TerminateThread(Parameter as ThreadInfo))
            {
                _ = MessageBox.Show(Properties.Resources.TerminateThreadErrorMessageText, Properties.Resources.TerminateThreadErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}