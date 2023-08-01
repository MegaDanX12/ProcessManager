using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class TerminateProcessCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public TerminateProcessCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            try
            {
                if (!VM.TerminateProcess(Parameter as ProcessInfo))
                {
                    _ = MessageBox.Show(Properties.Resources.ProcessTerminationFailedMessage, Properties.Resources.ProcessTerminationFailedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Win32Exception ex)
            {
                _ = MessageBox.Show(ex.Message, Properties.Resources.ProcessTerminationFailedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}