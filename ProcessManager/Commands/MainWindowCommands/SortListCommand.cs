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
            if (SortingColumn is "Clear")
            {
                VM.DatagridSortColumn = SortingColumn;
                VM.SortProcessList();
            }
            else
            {
                if (!VM.IsDatagridSorted)
                {
                    VM.DatagridSortColumn = SortingColumn;
                    VM.DatagridSortOrder = SortOrder.Ascending;
                    VM.IsDatagridSorted = true;
                    VM.SortProcessList();
                }
                else
                {
                    if (SortingColumn == VM.DatagridSortColumn)
                    {
                        VM.DatagridSortOrder = VM.DatagridSortOrder is SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                        VM.SortProcessList();
                    }
                    else
                    {
                        VM.DatagridSortColumn = SortingColumn;
                        VM.DatagridSortOrder = SortOrder.Ascending;
                        VM.SortProcessList();
                    }
                }
            }
        }
    }
}