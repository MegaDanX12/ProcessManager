using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowChildrenProcessesCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShowChildrenProcessesCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            List<ProcessInfo> ChildrenProcesses = VM.GetProcessChildren(Parameter as ProcessInfo);
            if (ChildrenProcesses.Count > 0)
            {
                ChildrenProcessesListVM ChildrenListVM = new(ChildrenProcesses);
                ProcessChildrenListWindow ListWindow = new();
                ListWindow.ChildrenProcessesDataGrid.DataContext = ChildrenListVM;
                ListWindow.ShowDialog();
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.NoChildrenProcessesErrorMessage, Properties.Resources.NoChildrenProcessesErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}