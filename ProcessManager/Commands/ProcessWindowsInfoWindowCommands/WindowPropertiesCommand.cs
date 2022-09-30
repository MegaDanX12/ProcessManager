using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class WindowPropertiesCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public WindowPropertiesCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (Parameter is not null)
            {
                Tuple<ProcessWindowsInfo, WindowInfo> Parameters = Parameter as Tuple<ProcessWindowsInfo, WindowInfo>;
                VM.SetCurrentWindowInfo(Parameters.Item2);
                WindowPropertiesWindow Window = new(VM)
                {
                    Owner = Parameters.Item1
                };
                Window.Show();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}