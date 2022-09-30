using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class HibernateMachineCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public HibernateMachineCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.HibernateSystem())
            {
                _ = MessageBox.Show(Properties.Resources.HibernateMachineErrorMessage, Properties.Resources.HibernateMachineErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}