using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowLogCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShowLogCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            LogManager Window = new();
            Window.Show();
        }

        public event EventHandler CanExecuteChanged;
    }
}