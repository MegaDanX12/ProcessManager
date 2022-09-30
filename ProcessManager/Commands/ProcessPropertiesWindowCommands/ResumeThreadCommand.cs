using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ResumeThreadCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ResumeThreadCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.ResumeThread(Parameter as ThreadInfo))
            {
                _ = MessageBox.Show(Properties.Resources.ResumeThreadErrorMessageText, Properties.Resources.ResumeThreadErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}