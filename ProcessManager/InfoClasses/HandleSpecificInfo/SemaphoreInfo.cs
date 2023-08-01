using System;
using System.ComponentModel;
using System.Globalization;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su un semaforo.
    /// </summary>
    public class SemaphoreInfo : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handle nativo al semaforo.
        /// </summary>
        private readonly IntPtr SemaphoreHandle;

        private string CurrentCountValue;
        private bool disposedValue;

        /// <summary>
        /// Conteggio attuale.
        /// </summary>
        public string CurrentCount
        {
            get
            {
                return CurrentCountValue;
            }
            private set
            {
                if (CurrentCountValue != value)
                {
                    CurrentCountValue = value;
                    NotifyPropertyChanged(nameof(CurrentCount));
                }
            }
        }

        /// <summary>
        /// Conteggio massimo.
        /// </summary>
        public string MaximumCount { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SemaphoreInfo"/>.
        /// </summary>
        /// <param name="CurrentCount">Conteggio attuale.</param>
        /// <param name="MaximumCount">Conteggio massimo.</param>
        public SemaphoreInfo(IntPtr Handle, uint? CurrentCount, uint? MaximumCount)
        {
            SemaphoreHandle = Handle;
            if (CurrentCount.HasValue)
            {
                CurrentCountValue = Convert.ToString(CurrentCount.Value, CultureInfo.CurrentCulture);
            }
            else
            {
                CurrentCountValue = Properties.Resources.UnavailableText;
            }
            if (MaximumCount.HasValue)
            {
                this.MaximumCount = Convert.ToString(MaximumCount.Value, CultureInfo.CurrentCulture);
            }
            else
            {
                this.MaximumCount = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Diminuisce il conteggio del semaforo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool AcquireSemaphore()
        {
            bool Result = NativeHelpers.AcquireSemaphore(SemaphoreHandle);
            if (Result)
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Semaphoro acquisito", EventAction.SemaphoreOperation, null));
                uint CurrentCount = Convert.ToUInt32(this.CurrentCount);
                if (CurrentCount > 0)
                {
                    this.CurrentCount = Convert.ToString(CurrentCount - 1, CultureInfo.CurrentCulture);
                }
            }
            return Result;
        }

        /// <summary>
        /// Aumenta il conteggio del semaforo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ReleaseSemaphore()
        {
            bool Result = NativeHelpers.ReleaseSemaphore(SemaphoreHandle);
            if (Result)
            {
                Logger.WriteEntry(NativeHelpers.BuildLogEntryForInformation("Semaphoro rilasciato", EventAction.SemaphoreOperation, null));
                uint CurrentCount = Convert.ToUInt32(this.CurrentCount);
                this.CurrentCount = Convert.ToString(CurrentCount + 1, CultureInfo.CurrentCulture);
            }
            return Result;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            SemaphoreInfo NewInfo = NativeHelpers.GetSemaphoreSpecificInfo(null, null, SemaphoreHandle, true);
            CurrentCount = NewInfo.CurrentCount;
            NewInfo.Dispose();
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }
                NativeMethods.Win32OtherFunctions.CloseHandle(SemaphoreHandle);
                disposedValue = true;
            }
        }

        ~SemaphoreInfo()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}