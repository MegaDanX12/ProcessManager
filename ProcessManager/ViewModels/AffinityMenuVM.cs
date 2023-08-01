using ProcessManager.Commands;
using ProcessManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class AffinityMenuVM
    {
        /// <summary>
        /// Oggetti <see cref="MenuItem"/> presenti nel menù.
        /// </summary>
        public ObservableCollection<MenuItem> Items { get; }

        private readonly AffinityChangeCommand AffinityChange;

        public AffinityMenuVM()
        {
            Items = new ObservableCollection<MenuItem>();
            AffinityChange = new AffinityChangeCommand(this);
            int NumberOfProcessors = Environment.ProcessorCount;
            for (int i = 0; i < NumberOfProcessors; i++)
            {
                MenuItem ProcessorItem = new MenuItem
                {
                    Header = "Core " + i,
                    IsCheckable = true,
                    Command = ChangeAffinityCommand
                };
                Items.Add(ProcessorItem);
            }
        }

        /// <summary>
        /// Comando per cambiare l'affinità di un processo o di un thread.
        /// </summary>
        public ICommand ChangeAffinityCommand
        {
            get
            {
                return AffinityChange;
            }
        }

        /// <summary>
        /// Cambia l'affinità di un processo o di un thread.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> o di <see cref="ThreadInfo"/>.</param>
        public void ChangeAffinity(object Info)
        {
            Contract.Requires(Info != null);
            if (Info is ProcessInfo)
            {
                ProcessInfo ProcessInfo = Info as ProcessInfo;
                BitArray FormerAffinity = new BitArray(BitConverter.GetBytes(ProcessInfo.Affinity));
                BitArray NewAffinity = new BitArray(64, false);
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].IsChecked)
                    {
                        NewAffinity.Set(i, true);
                    }
                }
                if (!ProcessInfo.SetProcessAffinity(NewAffinity))
                {
                    foreach (MenuItem item in Items)
                    {
                        if (item.IsChecked)
                        {
                            item.IsChecked = false;
                            break;
                        }
                    }
                    for (int i = 0; i < FormerAffinity.Count; i++)
                    {
                        if (FormerAffinity[i])
                        {
                            foreach (MenuItem Item in Items)
                            {
                                if (((string)Item.Header).Contains(i.ToString(CultureInfo.CurrentCulture)))
                                {
                                    Item.IsChecked = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (Info is ThreadInfo)
            {

            }
        }
    }
}