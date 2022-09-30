using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class SleepMachineCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public SleepMachineCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.SuspendSystem())
            {
                _ = MessageBox.Show(Properties.Resources.SleepMachineErrorMessage, Properties.Resources.SleepMachineErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}