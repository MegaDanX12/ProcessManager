using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Statistiche cumulative dei conteggi degli handle aperti del processo.
    /// </summary>
    public class HandleStatisticsCumulative : CumulativeStatisticsBase
    {
        /// <summary>
        /// Numero massimo di handle aperti.
        /// </summary>
        public uint MaxHandleCount { get; private set; }

        /// <summary>
        /// Numero medio di handle aperti.
        /// </summary>
        public uint AverageHandleCount { get; private set; }

        /// <summary>
        /// Numero minimo di handle aperti.
        /// </summary>
        public uint MinHandleCount { get; private set; }

        /// <summary>
        /// Numero massimo di handle GDI aperti.
        /// </summary>
        public int MaxGDIHandleCount { get; private set; }

        /// <summary>
        /// Numero medio di handle GDI aperti.
        /// </summary>
        public int AverageGDIHandleCount { get; private set; }

        /// <summary>
        /// Numero minimo di handle GDI aperti.
        /// </summary>
        public int MinGDIHandleCount { get; private set; }

        /// <summary>
        /// Numero massimo di handle USER aperti.
        /// </summary>
        public int MaxUSERHandleCount { get; private set; }

        /// <summary>
        /// Numero medio di handle USER aperti.
        /// </summary>
        public int AverageUSERHandleCount { get; private set; }

        /// <summary>
        /// Numero minimo di handle USER aperti.
        /// </summary>
        public int MinUSERHandleCount { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="HandleStatisticsCumulative"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="MonitoringDuration">Durata, in secondi, del monitoraggio.</param>
        public HandleStatisticsCumulative(string ProcessName, int MonitoringDuration = 0) : base(ProcessName, MonitoringDuration)
        {
            if (MonitoringDuration > 0)
            {
                MonitoringEndTime = MonitoringStartTime.AddSeconds(MonitoringDuration);
            }
            MaxHandleCount = 0;
            AverageHandleCount = 0;
            MinHandleCount = 0;
            MaxGDIHandleCount = 0;
            AverageGDIHandleCount = 0;
            MinGDIHandleCount = 0;
            MaxUSERHandleCount = 0;
            AverageUSERHandleCount = 0;
            MinUSERHandleCount = 0;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Statistics">Istanza di <see cref="HandleStatistics"/> con le informazioni sull'ultima esecuzione del processo.</param>
        public void UpdateData(HandleStatistics Statistics)
        {
            PerRuntimeStatistics.Add(Statistics);
            uint HandleCountTotal = 0;
            int GDIHandleCountTotal = 0;
            int USERHandleCountTotal = 0;
            foreach (HandleStatistics data in PerRuntimeStatistics.Cast<HandleStatistics>())
            {
                HandleCountTotal += data.HandleCountNumber;
                GDIHandleCountTotal += data.GDIHandlesCountNumber;
                USERHandleCountTotal += data.USERHandlesCountNumber;
            }
            if (MaxHandleCount is 0 || Statistics.HandleCountNumber > MaxHandleCount)
            {
                MaxHandleCount = Statistics.HandleCountNumber;
            }
            if (MinHandleCount is 0 || Statistics.HandleCountNumber < MinHandleCount)
            {
                MinHandleCount = Statistics.HandleCountNumber;
            }
            AverageHandleCount = HandleCountTotal / (uint)PerRuntimeStatistics.Count;
            if (MaxGDIHandleCount is 0 || Statistics.GDIHandlesCountNumber > MaxGDIHandleCount)
            {
                MaxGDIHandleCount = Statistics.GDIHandlesCountNumber;
            }
            if (MinGDIHandleCount is 0 || Statistics.GDIHandlesCountNumber < MinGDIHandleCount)
            {
                MinGDIHandleCount = Statistics.GDIHandlesCountNumber;
            }
            AverageGDIHandleCount = GDIHandleCountTotal / PerRuntimeStatistics.Count;
            if (MaxUSERHandleCount is 0 || Statistics.USERHandlesCountNumber > MaxUSERHandleCount)
            {
                MaxUSERHandleCount = Statistics.USERHandlesCountNumber;
            }
            if (MinUSERHandleCount is 0 || Statistics.USERHandlesCountNumber < MinUSERHandleCount)
            {
                MinUSERHandleCount = Statistics.USERHandlesCountNumber;
            }
            AverageUSERHandleCount = USERHandleCountTotal / PerRuntimeStatistics.Count;
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileXML(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\HandleStatistics.xml";
            string DocumentString =
                "<ProcessHandleStatistics>" + Environment.NewLine +
                "   <TotalHandleData>" + Environment.NewLine +
                "       <Max>" + MaxHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </TotalHandleData>" + Environment.NewLine +
                "   <GDIHandleData>" + Environment.NewLine +
                "       <Max>" + MaxGDIHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageGDIHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "<Average>" + Environment.NewLine +
                "       <Min>" + MinGDIHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </GDIHandleData>" + Environment.NewLine +
                "   <USERHandleData>" + Environment.NewLine +
                "       <Max>" + MaxUSERHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "       <Average>" + AverageUSERHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "       <Min>" + MinUSERHandleCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "   </USERHandleData>" + Environment.NewLine +
                "</ProcessHandleStatistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\Run" + i + ".xml");
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
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\HandleStatistics.txt";
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Process handle statistics");
            Writer.WriteLine();
            Writer.WriteLine("Max handle count: " + MaxHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average handle count: " + AverageHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min handle count: " + MinHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max GDI handle count: " + MaxGDIHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average GDI handle count: " + AverageGDIHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min GDI handle count: " + MinGDIHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.WriteLine("Max USER handle count: " + MaxUSERHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Average USER handle count: " + AverageUSERHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine("Min USER handle count: " + MinUSERHandleCount.ToString("N0", CultureInfo.CurrentCulture));
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileText(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\Run" + i + ".txt");
            }
        }

        /// <summary>
        /// Scrive i dati di monitoraggio in un file binario.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileBinary(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\HandleStatistics.bin";
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(MaxHandleCount);
            Writer.Write(AverageHandleCount);
            Writer.Write(MinHandleCount);
            Writer.Write(MaxGDIHandleCount);
            Writer.Write(AverageGDIHandleCount);
            Writer.Write(MinGDIHandleCount);
            Writer.Write(MaxUSERHandleCount);
            Writer.Write(AverageUSERHandleCount);
            Writer.Write(MinUSERHandleCount);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileBinary(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\HandleData\\Run" + i + ".bin");
            }
        }
    }
}