using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class StartLimitedProcessCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;

        public StartLimitedProcessCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SelectLimitWindow Window = new();
            if (Window.ShowDialog().Value)
            {
                VM.StartLimitedProcess(Window.Limit);
            }
        }
    }
}