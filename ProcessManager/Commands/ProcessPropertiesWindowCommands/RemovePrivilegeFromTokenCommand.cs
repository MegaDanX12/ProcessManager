using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class RemovePrivilegeFromTokenCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public RemovePrivilegeFromTokenCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (MessageBox.Show(Properties.Resources.TokenPrivilegeRemoveWarningMessageText, Properties.Resources.TokenPrivilegeRemoveWarningMessageTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Tuple<TokenInfo, int> Parameters = Parameter as Tuple<TokenInfo, int>;
                if (!VM.RemovePrivilege(Parameters.Item1.PrivilegesInfo[Parameters.Item2].Name))
                {
                    _ = MessageBox.Show(Properties.Resources.TokenPrivilegeRemoveErrorMessageText, Properties.Resources.TokenPrivilegeRemoveErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Parameters.Item1.UpdatePrivilegesData();
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}