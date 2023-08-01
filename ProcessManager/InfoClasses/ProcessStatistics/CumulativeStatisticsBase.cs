using System;
using System.Collections.Generic;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Classe base per le statistiche cumulative.
    /// </summary>
    public abstract class CumulativeStatisticsBase
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string ProcessName { get; }

        /// <summary>
        /// Data di inizio del monitoraggio.
        /// </summary>
        public DateTime MonitoringStartTime { get; }

        /// <summary>
        /// Data di fine del monitoraggio.
        /// </summary>
        public DateTime MonitoringEndTime { get; set; }

        /// <summary>
        /// Statistiche per ogni esecuzione del processo nel periodo di monitoraggio.
        /// </summary>
        public List<StatisticsBase> PerRuntimeStatistics { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="CumulativeStatisticsBase"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="MonitoringDuration">Durata, in secondi, del monitoraggio.</param>
        protected CumulativeStatisticsBase(string ProcessName, int MonitoringDuration = 0)
        {
            this.ProcessName = ProcessName;
            MonitoringStartTime = DateTime.Now;
            if (MonitoringDuration > 0)
            {
                MonitoringEndTime = MonitoringStartTime.AddSeconds(MonitoringDuration);
            }
            PerRuntimeStatistics = new();
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public abstract void WriteToFileXML(string FolderPath);

        /// <summary>
        /// Scrive i dati del monitoraggio in un file di testo.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public abstract void WriteToFileText(string FolderPath);

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public abstract void WriteToFileBinary(string FolderPath);
    }
}