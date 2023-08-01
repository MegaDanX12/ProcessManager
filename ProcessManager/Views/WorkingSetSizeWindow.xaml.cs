using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per WorkingSetSizeWindow.xaml
    /// </summary>
    public partial class WorkingSetSizeWindow : Window
    {
        /// <summary>
        /// Attuale dimensione del working set massimo o minimo.
        /// </summary>
        private readonly IntPtr CurrentWorkingSetSize;

        /// <summary>
        /// Attuale dimensione del working set massimo o minimo.
        /// </summary>
        private readonly IntPtr CurrentWorkingSetSize2;

        /// <summary>
        /// Indica l'operazione da eseguire (true per modificare il limite massimo, false per modificare il limite minimo).
        /// </summary>
        private readonly bool Operation;

        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> associata al processo del quale modificare i limiti.
        /// </summary>
        private readonly ProcessInfo Process;
        public WorkingSetSizeWindow(IntPtr CurrentWorkingSetSize, IntPtr CurrentWorkingSetSize2, bool Operation, ProcessInfo Process)
        {
            this.CurrentWorkingSetSize = CurrentWorkingSetSize;
            this.CurrentWorkingSetSize2 = CurrentWorkingSetSize2;
            this.Operation = Operation;
            this.Process = Process;
            InitializeComponent();
            WorkingSetSizeTextbox.Text = ((double)CurrentWorkingSetSize.ToInt64() / 1024 / 1024).ToString("F2", CultureInfo.CurrentCulture);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(WorkingSetSizeTextbox.Text))
            {
                if (double.TryParse(WorkingSetSizeTextbox.Text, out double NewSize))
                {
                    ulong NewSizeInteger = Convert.ToUInt64(NewSize * 1024 * 1024);
                    DialogResult = Operation ? Process.SetMaximumWorkingSetSize(NewSizeInteger, CurrentWorkingSetSize2) : Process.SetMinimumWorkingSetSize(NewSizeInteger, CurrentWorkingSetSize2);
                }
            }
        }
    }
}