using ProcessManager.InfoClasses.HandleSpecificInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Logica di interazione per EventInfoWindow.xaml
    /// </summary>
    public partial class EventInfoWindow : Window
    {
        private Timer DataUpdateTimer;

        private bool IsTimerDisposed;

        public EventInfoWindow(EventInfo Info)
        {
            DataContext = Info;
            InitializeComponent();
        }

        private void EventInfoWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataUpdateTimer.Dispose();
            IsTimerDisposed = true;
        }

        private void EventInfoWindow1_ContentRendered(object sender, EventArgs e)
        {
            DataUpdateTimer = new(new(UpdateData), DataContext, 1000, Timeout.Infinite);
        }

        private void UpdateData(object State)
        {
            EventInfo Info = (EventInfo)State;
            if (!IsTimerDisposed)
            {
                Info.UpdateData();
                DataUpdateTimer.Change(1000, Timeout.Infinite);
            }
        }
    }
}