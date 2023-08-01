using ProcessManager.Models;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ShowThreadWindowsInfoCommand : ICommand
    {
        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            ProcessWindowsInfo Window = new(ThreadInfo: Parameter as ThreadInfo);
            Window.Show();
        }

        public event EventHandler CanExecuteChanged;
    }
}