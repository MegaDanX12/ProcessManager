using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class UserListVM
    {
        /// <summary>
        /// Lista nomi utente.
        /// </summary>
        public ObservableCollection<string> Usernames { get; }

        public UserListVM()
        {
            Usernames = new ObservableCollection<string>(NativeHelpers.GetLocalUsers());
        }
    }
}