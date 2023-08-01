using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ShowTokenPropertiesCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ShowTokenPropertiesCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            ProcessTokenAdvancedInfoWindow Window = new(VM);
            Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}