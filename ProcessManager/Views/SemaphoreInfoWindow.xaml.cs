using ProcessManager.InfoClasses.HandleSpecificInfo;
using ProcessManager.ViewModels;
using System;
using System.Threading;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per SemaphoreInfoWindow.xaml
    /// </summary>
    public partial class SemaphoreInfoWindow : Window
    {
        private readonly ProcessPropertiesVM VM;

        private Timer DataUpdateTimer;

        private bool IsTimerDisposed;
        
        public SemaphoreInfoWindow(ProcessPropertiesVM VM, SemaphoreInfo Info)
        {
            if (Info is not null)
            {
                this.VM = VM;
                DataContext = Info;
                InitializeComponent();
                AcquireSemaphoreButton.Command = this.VM.AcquireSemaphoreCommand;
                ReleaseSemaphoreButton.Command = this.VM.ReleaseSemaphoreCommand;
                AcquireSemaphoreButton.CommandParameter = DataContext;
                ReleaseSemaphoreButton.CommandParameter = DataContext;
            }
            else
            {
                Close();
            }
        }

        private void SemaphoreInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataUpdateTimer.Dispose();
            IsTimerDisposed = true;
            ((SemaphoreInfo)DataContext).Dispose();
        }

        private void SemaphoreInfoWindow_ContentRendered(object sender, EventArgs e)
        {
            DataUpdateTimer = new(new(UpdateData), DataContext, 1000, Timeout.Infinite);
        }

        private void UpdateData(object State)
        {
            SemaphoreInfo Info = (SemaphoreInfo)State;
            if (!IsTimerDisposed)
            {
                Info.UpdateData();
                _ = DataUpdateTimer.Change(1000, Timeout.Infinite);
            }
        }
    }
}