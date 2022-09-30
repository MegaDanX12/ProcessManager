using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.UserWindowCommands
{
    public class SelectUserCommand : ICommand
    {
        private readonly UserInfoVM VM;

        public SelectUserCommand(UserInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (Parameter != null)
            {
                UserInfoWindow UserInfoWindow = Parameter as UserInfoWindow;
                UsersListWindow Window = new UsersListWindow
                {
                    Owner = UserInfoWindow
                };
                if (Window.ShowDialog().Value)
                {
                    UserInfoWindow.UsernameTextBox.Text = (string)Window.UsersListBox.SelectedItem;
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}