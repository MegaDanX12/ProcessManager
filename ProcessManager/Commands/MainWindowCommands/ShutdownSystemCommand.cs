using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShutdownSystemCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShutdownSystemCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            bool HybridShutdown = (bool)Parameter;
            if (!HybridShutdown)
            {
                if (!VM.ShutdownSystem())
                {
                    _ = MessageBox.Show(Properties.Resources.ShutdownSystemErrorMessage, Properties.Resources.ShutdownSystemErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (!VM.ShutdownSystemHybrid())
                {
                    _ = MessageBox.Show(Properties.Resources.ShutdownSystemErrorMessage, Properties.Resources.ShutdownSystemErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}