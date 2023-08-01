using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands
{
    public class AffinityChangeCommand : ICommand
    {
        private readonly AffinityMenuVM VM;

        public AffinityChangeCommand(AffinityMenuVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            VM.ChangeAffinity(Parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}