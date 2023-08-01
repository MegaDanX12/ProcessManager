using ProcessManager.Models;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowProcessWindowsInfoCommand : ICommand
    {
        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            
            if (Parameter is not null)
            {
                ProcessWindowsInfo Window = new(Parameter as ProcessInfo);
                Window.Show();
            }
            else
            {
                ProcessWindowsInfo Window = new();
                Window.Show();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}