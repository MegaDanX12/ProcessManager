using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ServicesInfoWindowCommands
{
    public class ShowServiceStatusCommand : ICommand
    {
        private readonly HostedServicesDataVM VM;

        public ShowServiceStatusCommand(HostedServicesDataVM VM)
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
            ServiceStatusInfoWindow Window = new(VM);
            Window.ShowDialog();
        }
    }
}