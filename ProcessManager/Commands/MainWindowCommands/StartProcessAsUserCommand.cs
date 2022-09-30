using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class StartProcessAsUserCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public StartProcessAsUserCommand(ProcessInfoVM VM)
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
                UserInfoWindow Window = new();
                MainWindow MainWindow = Parameter as MainWindow;
                Window.Owner = MainWindow;
                _ = Window.ShowDialog();
                if (Window.DialogResult.Value)
                {
                    VM.StartProcessAsUser(MainWindow.Username, MainWindow.Password);
                    MainWindow.Password.Dispose();
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}