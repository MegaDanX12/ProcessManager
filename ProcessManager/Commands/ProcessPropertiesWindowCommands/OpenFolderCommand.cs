using ProcessManager.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class OpenFolderCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public OpenFolderCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            _ = Process.Start("explorer.exe", "/select,\"" + Parameter + "\"");
        }

        public event EventHandler CanExecuteChanged;
    }
}