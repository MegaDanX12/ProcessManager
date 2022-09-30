using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Statistiche cumulative dell'attività I/O di un processo.
    /// </summary>
    public class IOStatisticsCumulative : CumulativeStatisticsBase
    {
        /// <summary>
        /// Numero massimo di operazioni di lettura eseguite.
        /// </summary>
        public ulong MaxReadCount { get; private set; }

        /// <summary>
        /// Numero medio di operazioni di lettura eseguite.
        /// </summary>
        public ulong AverageReadCount { get; private set; }

        /// <summary>
        /// Numero minimo di operazioni di lettura eseguite.
        /// </summary>
        public ulong MinReadCount { get; private set; }

        /// <summary>
        /// Numero massimo di operazioni di scrittura eseguite.
        /// </summary>
        public ulong MaxWriteCount { get; private set; }

        /// <summary>
        /// Numero medio di operazioni di scrittura eseguite.
        /// </summary>
        public ulong AverageWriteCount { get; private set; }

        /// <summary>
        /// Numero minimo di operazioni di scrittura eseguite.
        /// </summary>
        public ulong MinWriteCount { get; private set; }

        /// <summary>
        /// Numero massimo di operazioni diverse da lettura e scrittura eseguite.
        /// </summary>
        public ulong MaxOtherCount { get; private set; }

        /// <summary>
        /// Numero medio di operazioni diverse da lettura e scrittura eseguite.
        /// </summary>
        public ulong AverageOtherCount { get; private set; }

        /// <summary>
        /// Numero minimo di operazioni diverse da lettura e scrittura eseguite.
        /// </summary>
        public ulong MinOtherCount { get; private set; }

        /// <summary>
        /// Quantita massima di byte letti durante operazioni di lettura.
        /// </summary>
        public ulong MaxReadBytes { get; private set; }

        /// <summary>
        /// Quantità media di byte letti durante operazioni di lettura.
        /// </summary>
        public ulong AverageReadBytes { get; private set; }

        /// <summary>
        /// Quantità minima di byte letti durante operazioni di lettura.
        /// </summary>
        public ulong MinReadBytes { get; private set; }

        /// <summary>
        /// Quantità massima di byte scritti durante operazioni di scrittura.
        /// </summary>
        public ulong MaxWriteBytes { get; private set; }

        /// <summary>
        /// Quantità media di byte scritti durante operazioni di scrittura.
        /// </summary>
        public ulong AverageWriteBytes { get; private set; }

        /// <summary>
        /// Quantità minima di byte scritti durante operazioni di scrittura.
        /// </summary>
        public ulong MinWriteBytes { get; private set; }

        /// <summary>
        /// Quantità massima di byte elaborati durante operazioni diverse da lettura e scrittura.
        /// </summary>
        public ulong MaxOtherBytes { get; private set; }

        /// <summary>
        /// Quantità media di byte elaborati durante operazioni diverse da lettura e scrittura.
        /// </summary>
        public ulong AverageOtherBytes { get; private set; }

        /// <summary>
        /// Quantità minima di byte elaborati durante operazioni diverse da lettura e scrittura.
        /// </summary>
        public ulong MinOtherBytes { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="IOStatisticsCumulative"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="MonitoringDuration">Durata, in secondi, del monitoraggio.</param>
        public IOStatisticsCumulative(string ProcessName, int MonitoringDuration = 0) : base(ProcessName, MonitoringDuration)
        {
            MaxReadCount = 0;
            AverageReadCount = 0;
            MinReadCount = 0;
            MaxWriteCount = 0;
            AverageWriteCount = 0;
            MinOtherCount = 0;
            MaxOtherCount = 0;
            AverageOtherCount = 0;
            MinOtherCount = 0;
            MaxReadBytes = 0;
            AverageReadBytes = 0;
            MinReadBytes = 0;
            MaxWriteBytes = 0;
            AverageWriteBytes = 0;
            MinWriteBytes = 0;
            MaxOtherBytes = 0;
            AverageOtherBytes = 0;
            MinOtherBytes = 0;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Statistics">Istanza di <see cref="IOStatistics"/> con le informazioni.</param>
        public void UpdateData(IOStatistics Statistics)
        {
            ulong TotalReadCount = 0;
            ulong TotalWriteCount = 0;
            ulong TotalOtherCount = 0;
            ulong TotalReadBytes = 0;
            ulong TotalWriteBytes = 0;
            ulong TotalOtherBytes = 0;
            foreach (IOStatistics data in PerRuntimeStatistics.Cast<IOStatistics>())
            {
                TotalReadCount += data.IOData[0];
                TotalWriteCount += data.IOData[1];
                TotalOtherCount += data.IOData[2];
                TotalReadBytes += data.IOData[3];
                TotalWriteBytes += data.IOData[4];
                TotalOtherBytes += data.IOData[5];
            }
            if (MaxReadCount is 0 || Statistics.IOData[0] > MaxReadCount)
            {
                MaxReadCount = Statistics.IOData[0];
            }
            if (MinReadCount is 0 || Statistics.IOData[0] < MinReadCount)
            {
                MinReadCount = Statistics.IOData[0];
            }
            AverageReadCount = TotalReadCount / (ulong)PerRuntimeStatistics.Count;
            if (MaxWriteCount is 0 || Statistics.IOData[1] > MaxWriteCount)
            {
                MaxWriteCount = Statistics.IOData[1];
            }
            if (MinWriteCount is 0 || Statistics.IOData[1] < MinWriteCount)
            {
                MinWriteCount = Statistics.IOData[1];
            }
            AverageWriteCount = TotalWriteCount / (ulong)PerRuntimeStatistics.Count;
            if (MaxOtherCount is 0 || Statistics.IOData[2] > MaxOtherCount)
            {
                MaxOtherCount = Statistics.IOData[2];
            }
            if (MinOtherCount is 0 || Statistics.IOData[2] < MinOtherCount)
            {
                MinOtherCount = Statistics.IOData[2];
            }
            AverageOtherCount = TotalOtherCount / (ulong)PerRuntimeStatistics.Count;
            if (MaxReadBytes is 0 || Statistics.IOData[3] > MaxReadBytes)
            {
                MaxReadBytes = Statistics.IOData[3];
            }
            if (MinReadBytes is 0 || Statistics.IOData[3] < MinReadBytes)
            {
                MinReadBytes = Statistics.IOData[3];
            }
            AverageReadBytes = TotalReadBytes / (ulong)PerRuntimeStatistics.Count;
            if (MaxWriteBytes is 0 || Statistics.IOData[4] > MaxWriteBytes)
            {
                MaxWriteBytes = Statistics.IOData[4];
            }
            if (MinWriteBytes is 0 || Statistics.IOData[4] < MinWriteBytes)
            {
                MinWriteBytes = Statistics.IOData[4];
            }
            AverageWriteBytes = TotalWriteBytes / (ulong)PerRuntimeStatistics.Count;
            if (MaxOtherBytes is 0 || Statistics.IOData[5] > MaxOtherBytes)
            {
                MaxOtherBytes = Statistics.IOData[5];
            }
            if (MinOtherBytes is 0 || Statistics.IOData[5] < MinOtherBytes)
            {
                MinOtherBytes = Statistics.IOData[5];
            }
            AverageOtherBytes = TotalOtherBytes / (ulong)PerRuntimeStatistics.Count;
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileXML(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\IOStatistics.xml";
            string DocumentString =
                "<ProcessIOStatistics>" + Environment.NewLine +
                "   <ReadOperations>" + Environment.NewLine +
                "       <Max>" + MaxReadCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageReadCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinReadCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </ReadOperations>" + Environment.NewLine +
                "   <WriteOperations" + Environment.NewLine +
                "       <Max>" + MaxWriteCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageWriteCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + AverageWriteCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </WriteOperations>" + Environment.NewLine +
                "   <OtherOperations>" + Environment.NewLine +
                "       <Max>" + MaxOtherCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageOtherCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinOtherCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </OtherOperations>" + Environment.NewLine +
                "   <ReadBytes>" + Environment.NewLine +
                "       <Max>" + MaxReadBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageReadBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinReadBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </ReadBytes>" + Environment.NewLine +
                "   <WriteBytes>" + Environment.NewLine +
                "       <Max>" + MaxWriteBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageWriteBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinWriteBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </WriteBytes>" + Environment.NewLine +
                "   <OtherBytes>" + Environment.NewLine +
                "       <Max>" + MaxOtherBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageOtherBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinOtherBytes.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </OtherBytes>" + Environment.NewLine +
                "</ProcessIOStatistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\Run" + i + ".xml");
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
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\IOStatistics.txt";
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Process I/O statistics");
            Writer.WriteLine();
            Writer.WriteLine("Max reading operations: " + MaxReadCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average reading operations: " + AverageReadCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min reading operations: " + MinReadCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max writing operations: " + MaxWriteCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average writing operations: " + AverageWriteCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min writing operations: " + MinWriteCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max other operations count: " + MaxOtherCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average other operations count: " + AverageOtherCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min other operations count: " + MinOtherCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max read bytes: " + MaxReadBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average read bytes: " + AverageReadBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min read bytes:  " + MinReadBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max write bytes: " + MaxWriteBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average write bytes: " + AverageWriteBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min write bytes: " + MinWriteBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max processed bytes: " + MaxOtherBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average processed bytes: " + AverageOtherBytes.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min processed bytes: " + MinOtherBytes.ToString("N0", CultureInfo.CurrentCulture));
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\Run" + i + ".txt");
            }
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileBinary(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\IOStatistics.bin";
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(MaxReadCount);
            Writer.Write(AverageReadCount);
            Writer.Write(MinReadCount);
            Writer.Write(MaxWriteCount);
            Writer.Write(AverageWriteCount);
            Writer.Write(MinWriteCount);
            Writer.Write(MaxOtherCount);
            Writer.Write(AverageOtherCount);
            Writer.Write(MinOtherCount);
            Writer.Write(MaxReadBytes);
            Writer.Write(AverageReadBytes);
            Writer.Write(MinReadBytes);
            Writer.Write(MaxWriteBytes);
            Writer.Write(AverageWriteBytes);
            Writer.Write(MinWriteBytes);
            Writer.Write(MaxOtherBytes);
            Writer.Write(AverageOtherBytes);
            Writer.Write(MinOtherBytes);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\IOData\\Run" + i + ".bin");
            }
        }
    }
}