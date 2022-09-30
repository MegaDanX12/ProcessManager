using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ShowHandlePropertiesCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ShowHandlePropertiesCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            HandlePropertiesWindow Window = new(Parameter as HandleInfo, VM);
            _ = Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}