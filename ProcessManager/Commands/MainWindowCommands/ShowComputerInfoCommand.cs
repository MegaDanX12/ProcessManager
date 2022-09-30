using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowComputerInfoCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShowComputerInfoCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            ComputerInfoWindow Window = new();
            _ = Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}