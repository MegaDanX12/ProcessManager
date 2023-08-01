using ProcessManager.WMI;
using System.ComponentModel;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni su un file di paging.
    /// </summary>
    public class PageFileInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Percorso del file di paging.
        /// </summary>
        public string PageFilePath { get; }

        private string TotalSizePagesValue;

        /// <summary>
        /// Dimensione totale del file di paging, in pagine.
        /// </summary>
        public string TotalSizePages
        {
            get => TotalSizePagesValue;
            private set
            {
                if (TotalSizePagesValue != value)
                {
                    TotalSizePagesValue = value;
                    NotifyPropertyChanged(nameof(TotalSizePages));
                }
            }
        }

        private string TotalInUsePagesValue;

        /// <summary>
        /// Numero totale di pagine in uso nel file di paging.
        /// </summary>
        public string TotalInUsePages
        {
            get => TotalInUsePagesValue;
            private set
            {
                if (TotalInUsePagesValue != value)
                {
                    TotalInUsePagesValue = value;
                    NotifyPropertyChanged(nameof(TotalInUsePages));
                }
            }
        }

        private string PeakUsagePagesValue;

        /// <summary>
        /// Numero massimo di pagine usate nel file di paging durante la sessione.
        /// </summary>
        public string PeakUsagePages
        {
            get => PeakUsagePagesValue;
            private set
            {
                if (PeakUsagePagesValue != value)
                {
                    PeakUsagePagesValue = value;
                    NotifyPropertyChanged(nameof(PeakUsagePages));
                }
            }
        }

        /// <summary>
        /// Dimensione di una pagina in memoria.
        /// </summary>
        private readonly uint MemoryPageSize;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PageFileInfo"/>.
        /// </summary>
        /// <param name="PageFilePath">Percorso completo del file di paging.</param>
        /// <param name="TotalSize">Dimensione totale del file di paging, in pagine.</param>
        /// <param name="TotalInUse">Numero di pagine usate nel file di paging.</param>
        /// <param name="PeakUsage">Numero massimo di pagine usate nel file di paging durante la sessione.</param>
        /// <param name="PageSize">Dimensione di una pagina in memoria, in bytes.</param>
        public PageFileInfo(string PageFilePath, uint TotalSize, uint TotalInUse, uint PeakUsage, uint PageSize)
        {
            MemoryPageSize = PageSize;
            this.PageFilePath = PageFilePath;
            TotalSizePagesValue = UtilityMethods.PagesCountToString(PageSize, TotalSize);
            TotalInUsePagesValue = UtilityMethods.PagesCountToString(PageSize, TotalInUse);
            PeakUsagePagesValue = UtilityMethods.PagesCountToString(PageSize, PeakUsage);
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            string FilePath = PageFilePath.Insert(PageFilePath.IndexOf('\\'), "\\");
            PagefileUsageInfo UsageInfo = WMISystemInfoMethods.GetPagefileUsageInfo(FilePath);
            ulong TotalSizePages = (ulong)UsageInfo.TotalSize * 1024 * 1024 / MemoryPageSize;
            ulong TotalInUsePages = (ulong)UsageInfo.TotalInUse * 1024 * 1024 / MemoryPageSize;
            ulong PeakUsagePages = (ulong)UsageInfo.PeakUsage * 1024 * 1024 / MemoryPageSize;
            this.TotalSizePages = UtilityMethods.PagesCountToString(MemoryPageSize, TotalSizePages);
            this.TotalInUsePages = UtilityMethods.PagesCountToString(MemoryPageSize, TotalInUsePages);
            this.PeakUsagePages = UtilityMethods.PagesCountToString(MemoryPageSize, PeakUsagePages);
        }

        /// <summary>
        /// Cambia le dimensioni massime e minime del file di paging.
        /// </summary>
        /// <param name="InitialSize">Dimensione iniziale (minima), in MB.</param>
        /// <param name="MaximumSize">Dimensione massima, in MB.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ChangeSize(uint InitialSize, uint MaximumSize)
        {
            return WMISystemInfoMethods.ChangePagefileSize(PageFilePath, InitialSize, MaximumSize);
        }

        /// <summary>
        /// Elimina il file di paging.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool Delete()
        {
            return WMISystemInfoMethods.DeletePageFile(PageFilePath);
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}