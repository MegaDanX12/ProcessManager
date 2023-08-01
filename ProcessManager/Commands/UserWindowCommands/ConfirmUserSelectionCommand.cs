using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.UserWindowCommands
{
    public class ConfirmUserSelectionCommand : ICommand
    {
        private readonly UserInfoVM VM;

        public ConfirmUserSelectionCommand(UserInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            UsersListWindow Window = Parameter as UsersListWindow;
            if (Window == null)
            {
                return false;
            }
            else
            {
                if (Window.UsersListBox.SelectedIndex != -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Execute(object Parameter)
        {
            UsersListWindow Window = Parameter as UsersListWindow;
            Contract.Requires(Window != null);
            Window.DialogResult = true;
            Window.Close();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}