using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ServicesInfoWindowCommands
{
    public class ShowServiceConfigurationCommand : ICommand
    {
        private readonly HostedServicesDataVM VM;

        public ShowServiceConfigurationCommand(HostedServicesDataVM VM)
        {
            this.VM = VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            ServiceConfigurationInfoWindow Window = new(VM);
            Window.ShowDialog();
        }
    }
}