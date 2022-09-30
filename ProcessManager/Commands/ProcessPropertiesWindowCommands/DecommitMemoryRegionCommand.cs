using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class DecommitMemoryRegionCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public DecommitMemoryRegionCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.DecommitMemoryRegion(Parameter as MemoryRegionInfo))
            {
                _ = MessageBox.Show(Properties.Resources.MemoryRegionDecommitMemoryErrorMessage, Properties.Resources.MemoryRegionDecommitMemoryErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}