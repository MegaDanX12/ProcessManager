using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ServiceConfigurationInfoWindowCommands
{
    public class ShowServiceOptionalConfigurationCommand : ICommand
    {
        private readonly HostedServicesDataVM VM;

        public ShowServiceOptionalConfigurationCommand(HostedServicesDataVM VM)
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
            ServiceOptionalConfigurationInfoWindow Window = new(VM);
            Window.ShowDialog();
        }
    }
}