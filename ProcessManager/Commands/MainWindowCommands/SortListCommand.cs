using ProcessManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class SortListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly ProcessInfoVM VM;


        public SortListCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string SortingColumn = parameter as string;
            if (!VM.IsDatagridSorted)
            {
                VM.DatagridSortColumn = SortingColumn;
                VM.DatagridSortOrder = SortOrder.Descending;
                VM.IsDatagridSorted = true;
                VM.SortProcessList();
            }
            else
            {
                if (SortingColumn == VM.DatagridSortColumn)
                {
                    VM.DatagridSortOrder = VM.DatagridSortOrder is SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending;
                    VM.SortProcessList();
                }
                else
                {
                    VM.DatagridSortColumn = SortingColumn;
                    VM.DatagridSortOrder = SortOrder.Descending;
                    VM.SortProcessList();
                }
            }
        }
    }
}