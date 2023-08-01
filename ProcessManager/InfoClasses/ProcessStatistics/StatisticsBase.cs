using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Classe base per le statistiche.
    /// </summary>
    public abstract class StatisticsBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public abstract void Update(SafeProcessHandle Handle);

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public abstract void WriteToFileXML(string FilePath);

        /// <summary>
        /// Scrive i dati del monitoraggio in un file di testo.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public abstract void WriteToFileText(string FilePath);

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public abstract void WriteToFileBinary(string FilePath);


        protected void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}