using ProcessManager.InfoClasses.HandleSpecificInfo;
using System;
using System.Threading;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per TimerInfoWindow.xaml
    /// </summary>
    public partial class TimerInfoWindow : Window
    {
        private Timer DataUpdateTimer;

        private bool IsTimerDisposed;

        public TimerInfoWindow(TimerInfo Info)
        {
            DataContext = Info;
            InitializeComponent();
        }

        private void UpdateData(object State)
        {
            TimerInfo Info = (TimerInfo)State;
            if (!IsTimerDisposed)
            {
                Info.UpdateData();
                DataUpdateTimer.Change(1000, Timeout.Infinite);
            }
        }

        private void TimerInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataUpdateTimer.Dispose();
            IsTimerDisposed = true;
        }

        private void TimerInfoWindow_ContentRendered(object sender, EventArgs e)
        {
            DataUpdateTimer = new(new(UpdateData), DataContext, 1000, Timeout.Infinite);
        }
    }
}