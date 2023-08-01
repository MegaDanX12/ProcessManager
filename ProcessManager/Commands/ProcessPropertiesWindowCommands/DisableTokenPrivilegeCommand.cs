using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class DisableTokenPrivilegeCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public DisableTokenPrivilegeCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Tuple<TokenInfo, int> Parameters = Parameter as Tuple<TokenInfo, int>;
            if (!VM.DisablePrivilege(Parameters.Item1.PrivilegesInfo[Parameters.Item2].Name))
            {
                _ = MessageBox.Show(Properties.Resources.TokenPrivilegeChangeErrorMessageText, Properties.Resources.TokenPrivilegeChangeErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Parameters.Item1.UpdatePrivilegesData();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}