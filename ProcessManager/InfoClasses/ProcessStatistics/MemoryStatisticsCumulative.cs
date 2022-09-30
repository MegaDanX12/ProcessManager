using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Statistiche cumulative dell'uso della memoria da parte di un processo.
    /// </summary>
    public class MemoryStatisticsCumulative : CumulativeStatisticsBase
    {
        /// <summary>
        /// Memoria privata massima.
        /// </summary>
        public ulong MaxPrivateBytes { get; private set; }

        /// <summary>
        /// Memoria privata media.
        /// </summary>
        public ulong AveragePrivateBytes { get; private set; }

        /// <summary>
        /// Memoria private minima.
        /// </summary>
        public ulong MinPrivateBytes { get; private set; }

        /// <summary>
        /// Memoria virtuale massima.
        /// </summary>
        public ulong MaxVirtualSize { get; private set; }

        /// <summary>
        /// Memoria virtuale media.
        /// </summary>
        public ulong AverageVirtualSize { get; private set; }

        /// <summary>
        /// Memoria virtuale minima.
        /// </summary>
        public ulong MinVirtualSize { get; private set; }

        /// <summary>
        /// Massimo numero di page fault.
        /// </summary>
        public ulong MaxPageFaultCount { get; private set; }

        /// <summary>
        /// Numero di page fault medio.
        /// </summary>
        public ulong AveragePageFaultCount { get; private set; }

        /// <summary>
        /// Numero di page fault minimo.
        /// </summary>
        public ulong MinPageFaultCount { get; private set; }

        /// <summary>
        /// Working set massimo.
        /// </summary>
        public ulong MaxWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set medio.
        /// </summary>
        public ulong AverageWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set minimo.
        /// </summary>
        public ulong MinWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set privato massimo.
        /// </summary>
        public ulong MaxPrivateWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set privato medio.
        /// </summary>
        public ulong AveragePrivateWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set privato minimo.
        /// </summary>
        public ulong MinPrivateWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condivisibile massimo.
        /// </summary>
        public ulong MaxShareableWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condivisibile medio.
        /// </summary>
        public ulong AverageShareableWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condivisibile minimo.
        /// </summary>
        public ulong MinShareableWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condiviso massimo.
        /// </summary>
        public ulong MaxSharedWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condiviso medio.
        /// </summary>
        public ulong AverageSharedWorkingSetSize { get; private set; }

        /// <summary>
        /// Working set condiviso minimo.
        /// </summary>
        public ulong MinSharedWorkingSetSize { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza <see cref="MemoryStatisticsCumulative"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="MonitoringDuration">Durata del monitoraggio.</param>
        public MemoryStatisticsCumulative(string ProcessName, int MonitoringDuration = 0) : base(ProcessName, MonitoringDuration)
        {
            MaxPrivateBytes = 0;
            AveragePrivateBytes = 0;
            MinPrivateBytes = 0;
            MaxVirtualSize = 0;
            AverageVirtualSize = 0;
            MinVirtualSize = 0;
            MaxPageFaultCount = 0;
            AveragePageFaultCount = 0;
            MinPageFaultCount = 0;
            MaxWorkingSetSize = 0;
            AverageWorkingSetSize = 0;
            MinWorkingSetSize = 0;
            MaxPrivateWorkingSetSize = 0;
            AveragePrivateWorkingSetSize = 0;
            MinPrivateWorkingSetSize = 0;
            MaxShareableWorkingSetSize = 0;
            AverageShareableWorkingSetSize = 0;
            MinShareableWorkingSetSize = 0;
            MaxSharedWorkingSetSize = 0;
            AverageSharedWorkingSetSize = 0;
            MinSharedWorkingSetSize = 0;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Statistics">Istanza di <see cref="MemoryStatistics"/> con le informazioni.</param>
        public void UpdateData(MemoryStatistics Statistics)
        {
            ulong TotalPrivateBytes = 0;
            ulong TotalVirtualSize = 0;
            ulong TotalPageFaultCount = 0;
            ulong TotalWorkingSetSize = 0;
            ulong TotalPrivateWorkingSetSize = 0;
            ulong TotalShareableWorkingSetSize = 0;
            ulong TotalSharedWorkingSetSize = 0;
            foreach (MemoryStatistics data in PerRuntimeStatistics.Cast<MemoryStatistics>())
            {
                TotalPrivateBytes += data.MemoryData[0];
                TotalVirtualSize += data.MemoryData[2];
                TotalPageFaultCount += data.MemoryData[4];
                TotalWorkingSetSize += data.MemoryData[5];
                TotalPrivateWorkingSetSize += data.WsData[0];
                TotalShareableWorkingSetSize += data.WsData[1];
                TotalSharedWorkingSetSize += data.WsData[2];
            }
            if (MaxPrivateBytes is 0 || Statistics.MemoryData[0] > MaxPrivateBytes)
            {
                MaxPrivateBytes = Statistics.MemoryData[0];
            }
            if (MinPrivateBytes is 0 || Statistics.MemoryData[0] < MinPrivateBytes)
            {
                MinPrivateBytes = Statistics.MemoryData[0];
            }
            AveragePrivateBytes = TotalPrivateBytes / (ulong)PerRuntimeStatistics.Count;
            if (MaxVirtualSize is 0 || Statistics.MemoryData[2] > MaxVirtualSize)
            {
                MaxVirtualSize = Statistics.MemoryData[2];
            }
            if (MinVirtualSize is 0 || Statistics.MemoryData[2] < MinVirtualSize)
            {
                MinPrivateBytes = Statistics.MemoryData[2];
            }
            AverageVirtualSize = TotalVirtualSize / (ulong)PerRuntimeStatistics.Count;
            if (MaxPageFaultCount is 0 || Statistics.MemoryData[4] > MaxPageFaultCount)
            {
                MaxPageFaultCount = Statistics.MemoryData[4];
            }
            if (MinPageFaultCount is 0 || Statistics.MemoryData[4] < MinPageFaultCount)
            {
                MinPageFaultCount = Statistics.MemoryData[4];
            }
            AveragePageFaultCount = TotalPageFaultCount / (ulong)PerRuntimeStatistics.Count;
            if (MaxWorkingSetSize is 0 || Statistics.MemoryData[5] > MaxWorkingSetSize)
            {
                MinWorkingSetSize = Statistics.MemoryData[5];
            }
            if (MinWorkingSetSize is 0 || Statistics.MemoryData[5] < MinWorkingSetSize)
            {
                MinWorkingSetSize = Statistics.MemoryData[5];
            }
            AveragePageFaultCount = TotalPageFaultCount / (ulong)PerRuntimeStatistics.Count;
            if (MaxPrivateWorkingSetSize is 0 || Statistics.WsData[0] > MaxPrivateWorkingSetSize)
            {
                MaxPrivateWorkingSetSize = Statistics.WsData[0];
            }
            if (MinPrivateWorkingSetSize is 0 || Statistics.WsData[0] < MinPrivateWorkingSetSize)
            {
                MinPrivateWorkingSetSize = Statistics.WsData[0];
            }
            AveragePrivateWorkingSetSize = TotalPrivateWorkingSetSize / (ulong)PerRuntimeStatistics.Count;
            if (MaxShareableWorkingSetSize is 0 || Statistics.WsData[1] > MaxShareableWorkingSetSize)
            {
                MaxShareableWorkingSetSize = Statistics.WsData[1];
            }
            if (MinShareableWorkingSetSize is 0 || Statistics.WsData[1] < MinShareableWorkingSetSize)
            {
                MinShareableWorkingSetSize = Statistics.WsData[1];
            }
            AverageShareableWorkingSetSize = TotalShareableWorkingSetSize / (ulong)PerRuntimeStatistics.Count;
            if (MaxShareableWorkingSetSize is 0 || Statistics.WsData[2] > MaxShareableWorkingSetSize)
            {
                MaxShareableWorkingSetSize = Statistics.WsData[2];
            }
            if (MinSharedWorkingSetSize is 0 || Statistics.WsData[2] < MinSharedWorkingSetSize)
            {
                MinSharedWorkingSetSize = Statistics.WsData[2];
            }
            AverageSharedWorkingSetSize = TotalSharedWorkingSetSize / (ulong)PerRuntimeStatistics.Count;
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileBinary(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\MemoryStatistics.bin";
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(MaxPrivateBytes);
            Writer.Write(AveragePrivateBytes);
            Writer.Write(MinPrivateBytes);
            Writer.Write(MaxVirtualSize);
            Writer.Write(AverageVirtualSize);
            Writer.Write(MinVirtualSize);
            Writer.Write(MaxPageFaultCount);
            Writer.Write(AveragePageFaultCount);
            Writer.Write(MinPageFaultCount);
            Writer.Write(MaxWorkingSetSize);
            Writer.Write(AverageWorkingSetSize);
            Writer.Write(MinWorkingSetSize);
            Writer.Write(MaxPrivateWorkingSetSize);
            Writer.Write(AveragePrivateWorkingSetSize);
            Writer.Write(MinPrivateWorkingSetSize);
            Writer.Write(MaxShareableWorkingSetSize);
            Writer.Write(AverageShareableWorkingSetSize);
            Writer.Write(MinShareableWorkingSetSize);
            Writer.Write(MaxSharedWorkingSetSize);
            Writer.Write(AverageSharedWorkingSetSize);
            Writer.Write(MinSharedWorkingSetSize);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\Run" + i + ".bin");
            }
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file di testo.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileText(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\MemoryStatistics.txt";
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Process memory statistics");
            Writer.WriteLine();
            Writer.WriteLine("Max private memory size: " + MaxPrivateBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average private memory size: " + AveragePrivateBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min private memory size: " + MinPrivateBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max virtual size: " + MaxVirtualSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average virtual size: " + AverageVirtualSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min virtual size: " + MinVirtualSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max page fault count: " + MaxPageFaultCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average page fault count: " + AveragePageFaultCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min page fault count: " + MinPageFaultCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max working set size: " + MaxWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average working set size: " + AverageWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min working set size: " + MinWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max private working set size: " + MaxPrivateWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average private working set size: " + AveragePrivateWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min private working set size: " + MinPrivateWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max shareable working set size: " + MaxShareableWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average shareable working set size: " + AverageShareableWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min shareable working set size: " + MinShareableWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max shared working set size: " + MaxSharedWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average shared working set size: " + AverageSharedWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min shared working set size: " + MinSharedWorkingSetSize.ToString("N0", CultureInfo.CurrentCulture));
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\Run" + i + ".txt");
            }
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileXML(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\MemoryStatistics.xml";
            string DocumentString =
                "<ProcessMemoryStatistics>" + Environment.NewLine +
                "   <PrivateMemory>" + Environment.NewLine +
                "       <Max>" + MaxPrivateBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AveragePrivateBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinPrivateBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </PrivateMemory>" + Environment.NewLine +
                "   <VirtualSize>" + Environment.NewLine +
                "       <Max>" + MaxVirtualSize.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageVirtualSize.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinVirtualSize.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </VirtualSize>" + Environment.NewLine +
                "   <PageFault>" + Environment.NewLine +
                "       <Max>" + MaxPageFaultCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AveragePageFaultCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinPageFaultCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </PageFault>" + Environment.NewLine +
                "   <WorkingSetSize>" + Environment.NewLine +
                "       <Max>" + MaxWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </WorkingSetSize>" + Environment.NewLine +
                "   <PrivateWorkingSetSize>" + Environment.NewLine +
                "       <Max>" + MaxPrivateWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AveragePrivateWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinPrivateWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </PrivateWorkingSetSize>" + Environment.NewLine +
                "   <ShareableWorkingSetSize>" + Environment.NewLine +
                "       <Max>" + MaxShareableWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageShareableWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinShareableWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </ShareableWorkingSetSize>" + Environment.NewLine +
                "   <SharedWorkingSetSize>" + Environment.NewLine +
                "       <Max>" + MaxSharedWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageSharedWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinSharedWorkingSetSize.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </SharedWorkingSetSize>" + Environment.NewLine +
                "</ProcessMemoryStatistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\MemoryData\\Run" + i + ".xml");
            }
        }
    }
}