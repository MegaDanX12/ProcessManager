using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class SetProcessMinimumWorkingSetSizeCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public SetProcessMinimumWorkingSetSizeCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            ProcessInfo Info = Parameter as ProcessInfo;
            (IntPtr MinimumSize, IntPtr MaximumSize) = Info.GetWorkingSetLimits();
            WorkingSetSizeWindow Window = new(MinimumSize, MaximumSize, false, Info);
            if (!Window.ShowDialog().Value)
            {
                _ = MessageBox.Show(Properties.Resources.SetWorkingSetSizeErrorMessage, Properties.Resources.SetWorkingSetSizeErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}