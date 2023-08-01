using ProcessManager.ViewModels;
using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
namespace ProcessManager.Commands.ProcessWindowsInfoWindowCommands
{
    public class CloseCommand : ICommand
    {
        private readonly ProcessWindowsInfoVM VM;

        public CloseCommand(ProcessWindowsInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.CloseWindow(Parameter as WindowInfo))
            {
                MessageBox.Show(Properties.Resources.WindowCloseErrorMessage, Properties.Resources.WindowCloseErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}