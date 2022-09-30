using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class CloseHandleCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public CloseHandleCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.CloseHandle(Parameter as HandleInfo))
            {
                _ = MessageBox.Show(Properties.Resources.HandleClosingErrorMessageText, Properties.Resources.HandleClosingErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}