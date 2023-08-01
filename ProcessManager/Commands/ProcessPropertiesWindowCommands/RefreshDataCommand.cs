using ProcessManager.ViewModels;
using System;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class RefreshDataCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public RefreshDataCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (Settings.DataSource is ProcessDataSource.WMI)
            {
                switch (Parameter as string)
                {
                    case "Modules":
                        VM.UpdateModulesList();
                        break;
                    case "Memory":
                        VM.UpdateMemoryRegionsInfo();
                        break;
                    case "Handles":
                        VM.UpdateHandlesList();
                        break;
                }
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}