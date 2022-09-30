using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class HandlesDetailedInfoCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public HandlesDetailedInfoCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            HandlesCountInfoWindow Window = new HandlesCountInfoWindow(VM);
            Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}