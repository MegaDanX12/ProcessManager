using System;
using System.Windows.Input;

namespace ProcessManager.Commands.UserWindowCommands
{
    public class ConfirmUserSettingsCommand : ICommand
    {

        public bool CanExecute(object Parameter)
        {
            if (Parameter != null)
            {
                UserInfoWindow UserWindow = Parameter as UserInfoWindow;
                return !string.IsNullOrWhiteSpace(UserWindow.UsernameTextBox.Text);
            }
            else
            {
                return false;
            }
        }

        public void Execute(object Parameter)
        {
            if (Parameter != null)
            {
                UserInfoWindow UserWindow = Parameter as UserInfoWindow;
                UserWindow.DialogResult = true;
                MainWindow MainWindow = (MainWindow)UserWindow.Owner;
                MainWindow.Username = UserWindow.UsernameTextBox.Text;
                MainWindow.Password = UserWindow.PasswordTextBox.SecurePassword;
                UserWindow.Close();
            }
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