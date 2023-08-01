using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class FreeMemoryRegionCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public FreeMemoryRegionCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.FreeMemoryRegion(Parameter as MemoryRegionInfo))
            {
                _ = MessageBox.Show(Properties.Resources.MemoryRegionFreeMemoryErrorMessage, Properties.Resources.MemoryRegionFreeMemoryErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}