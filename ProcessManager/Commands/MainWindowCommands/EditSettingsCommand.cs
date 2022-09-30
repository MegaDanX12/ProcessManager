using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
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
    public class EditSettingsCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public EditSettingsCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            SettingsWindow Window = new();
            Window.ShowDialog();
        }

        public event EventHandler CanExecuteChanged;
    }
}