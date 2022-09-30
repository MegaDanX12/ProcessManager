using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class DebugProcessCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public DebugProcessCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.IsProcessDebugged(Parameter as ProcessInfo))
            {
                if (!VM.DebugProcess(Parameter as ProcessInfo))
                {
                    _ = MessageBox.Show(Properties.Resources.DebugProcessErrorMessage, Properties.Resources.DebugProcessErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.DebugProcessErrorMessage, Properties.Resources.DebugProcessErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}