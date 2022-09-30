using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class RestartSystemCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public RestartSystemCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            bool BootOptionsRestart = (bool)Parameter;
            if (!BootOptionsRestart)
            {
                if (!VM.RestartSystem())
                {
                    _ = MessageBox.Show(Properties.Resources.RestartSystemErrorMessage, Properties.Resources.RestartSystemErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (!VM.RestartSystemToBootOptions())
                {
                    _ = MessageBox.Show(Properties.Resources.RestartSystemErrorMessage, Properties.Resources.RestartSystemErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}