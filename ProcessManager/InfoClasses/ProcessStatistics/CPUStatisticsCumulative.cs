using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Statistiche cumulative dell'uso della CPU da parte di un processo.
    /// </summary>
    public class CPUStatisticsCumulative : CumulativeStatisticsBase
    {
        /// <summary>
        /// Numero di cicli CPU massimo.
        /// </summary>
        public ulong MaxCyclesCount { get; private set; }

        /// <summary>
        /// Numero medio di cicli CPU.
        /// </summary>
        public ulong AverageCyclesCount { get; private set; }

        /// <summary>
        /// NUmero di cicli CPU minimo.
        /// </summary>
        public ulong MinCyclesCount { get; private set; }

        /// <summary>
        /// Tempo kernel massimo.
        /// </summary>
        public TimeSpan MaxKernelTime { get; private set; }

        /// <summary>
        /// Tempo kernel medio.
        /// </summary>
        public TimeSpan AverageKernelTime { get; private set; }

        /// <summary>
        /// Tempo kernel minimo.
        /// </summary>
        public TimeSpan MinKernelTime { get; private set; }

        /// <summary>
        /// Tempo utente massimo.
        /// </summary>
        public TimeSpan MaxUserTime { get; private set; }

        /// <summary>
        /// Tempo utente medio.
        /// </summary>
        public TimeSpan AverageUserTime { get; private set; }

        /// <summary>
        /// Tempo utente minimo.
        /// </summary>
        public TimeSpan MinUserTime { get; private set; }

        /// <summary>
        /// Tempo totale massimo.
        /// </summary>
        public TimeSpan MaxTotalTime { get; private set; }

        /// <summary>
        /// Tempo totale medio.
        /// </summary>
        public TimeSpan AverageTotalTime { get; private set; }

        /// <summary>
        /// Tempo totale minimo.
        /// </summary>
        public TimeSpan MinTotalTime { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="CPUStatisticsCumulative"/>.
        /// </summary>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="MonitoringDuration">Durata del monitoraggio, in secondi.</param>
        public CPUStatisticsCumulative(string ProcessName, int MonitoringDuration = 0) : base(ProcessName, MonitoringDuration)
        {
            MaxCyclesCount = 0;
            AverageCyclesCount = 0;
            MinCyclesCount = 0;
            MaxKernelTime = TimeSpan.Zero;
            AverageKernelTime = TimeSpan.Zero;
            MinKernelTime = TimeSpan.Zero;
            MaxUserTime = TimeSpan.Zero;
            AverageUserTime = TimeSpan.Zero;
            MinUserTime = TimeSpan.Zero;
            MaxTotalTime = TimeSpan.Zero;
            AverageTotalTime = TimeSpan.Zero;
            MinTotalTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Statistics">Istanza di <see cref="CPUStatistics"/> con le informazioni relative all'ultima esecuzione del processo.</param>
        public void UpdateData(CPUStatistics Statistics)
        {
            PerRuntimeStatistics.Add(Statistics);
            ulong TotalCyclesCount = 0;
            long TotalKernelTime = 0;
            long TotalUserTime = 0;
            long TotalTime = 0;
            foreach (CPUStatistics data in PerRuntimeStatistics.Cast<CPUStatistics>())
            {
                TotalCyclesCount += data.CyclesCount;
                if (data.ProcessTimes[0].HasValue)
                {
                    TotalKernelTime += data.ProcessTimes[0].Value.Ticks;
                }
                if (data.ProcessTimes[1].HasValue)
                {
                    TotalUserTime += data.ProcessTimes[1].Value.Ticks;
                }
                if (data.ProcessTimes[2].HasValue)
                {
                    TotalTime += data.ProcessTimes[2].Value.Ticks;
                }
            }
            if (MaxCyclesCount is 0 || Statistics.CyclesCount > MaxCyclesCount)
            {
                MaxCyclesCount = Statistics.CyclesCount;
            }
            if (MinCyclesCount is 0 || Statistics.CyclesCount < MinCyclesCount)
            {
                MinCyclesCount = Statistics.CyclesCount;
            }
            AverageCyclesCount = TotalCyclesCount / (ulong)PerRuntimeStatistics.Count;
            if (Statistics.ProcessTimes[0].HasValue)
            {
                if (MaxKernelTime == TimeSpan.Zero || Statistics.ProcessTimes[0] > MaxKernelTime)
                {
                    MaxKernelTime = Statistics.ProcessTimes[0].Value;
                }
                if (MinKernelTime == TimeSpan.Zero || Statistics.ProcessTimes[0] < MinKernelTime)
                {
                    MinKernelTime = Statistics.ProcessTimes[0].Value;
                }
                AverageKernelTime = new(TotalKernelTime / PerRuntimeStatistics.Count);
            }
            if (Statistics.ProcessTimes[1].HasValue)
            {
                if (MaxUserTime == TimeSpan.Zero || Statistics.ProcessTimes[1] > MaxUserTime)
                {
                    MaxUserTime = Statistics.ProcessTimes[1].Value;
                }
                if ( MinUserTime == TimeSpan.Zero || Statistics.ProcessTimes[1] < MinUserTime)
                {
                    MinUserTime = Statistics.ProcessTimes[1].Value;
                }
                AverageUserTime = new(TotalUserTime / PerRuntimeStatistics.Count);
            }
            if (Statistics.ProcessTimes[2].HasValue)
            {
                if (MaxTotalTime == TimeSpan.Zero || Statistics.ProcessTimes[2] > MaxTotalTime)
                {
                    MaxTotalTime = Statistics.ProcessTimes[2].Value;
                }
                if (MinTotalTime == TimeSpan.Zero || Statistics.ProcessTimes[2] < MinTotalTime)
                {
                    MaxTotalTime = Statistics.ProcessTimes[2].Value;
                }
                AverageTotalTime = new(TotalTime / PerRuntimeStatistics.Count);
            }
        }

        /// <summary>
        /// Scrive il risultato del monitoraggio su file XML.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileXML(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\CpuStatistics.xml";
            string DocumentString =
                "<ProcessCpuStatistics>" + Environment.NewLine +
                "  <CpuCyclesData>" + Environment.NewLine +
                "      <Max>" + MaxCyclesCount.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "      <Average>" + AverageCyclesCount.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "      <Min>" + MinCyclesCount.ToString("N0", CultureInfo.InvariantCulture) + "</Min>" + Environment.NewLine +
                "  </CpuCyclesData>" + Environment.NewLine +
                "  <KernelTimeData>" + Environment.NewLine +
                "      <Max>" + MaxKernelTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "      <Average>" + AverageKernelTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "      <Min>" + MinKernelTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "<Min>" + Environment.NewLine +
                "  </KernelTimeData>" + Environment.NewLine +
                "  <UserTimeData>" + Environment.NewLine +
                "      <Max>" + MaxUserTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "      <Average>" + AverageUserTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "      <Min>" + MinUserTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "<Min>" + Environment.NewLine +
                "  </UserTimeData>" + Environment.NewLine +
                "  <TotalTimeData>" + Environment.NewLine +
                "      <Max>" + MaxTotalTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Max>" + Environment.NewLine +
                "      <Average>" + AverageTotalTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</Average>" + Environment.NewLine +
                "      <Min>" + MinTotalTime.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "<Min>" + Environment.NewLine +
                "  </TotalTimeData>" + Environment.NewLine +
                "</ProcessCpuStatistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileXML(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\Run" + i + ".xml");
            }
        }

        /// <summary>
        /// Scrive il risultato del monitoraggio in un file di testo.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileText(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\CpuStatistics.txt";
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Process CPU statistics");
            Writer.WriteLine();
            Writer.Write("Max cycles count: ");
            Writer.WriteLine(MaxCyclesCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.Write("Average cycles count: ");
            Writer.WriteLine(AverageCyclesCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.Write("Min cycles count: ");
            Writer.WriteLine(MinCyclesCount.ToString("N0", CultureInfo.CurrentCulture));
            Writer.WriteLine();
            Writer.Write("Max kernel time: ");
            Writer.WriteLine(MaxKernelTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Average kernel time: ");
            Writer.WriteLine(AverageKernelTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Min kernel time: ");
            Writer.WriteLine(MinKernelTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.WriteLine();
            Writer.Write("Max user time: ");
            Writer.WriteLine(MaxUserTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Average user time: ");
            Writer.WriteLine(AverageUserTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Min user time: ");
            Writer.WriteLine(MinUserTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.WriteLine();
            Writer.Write("Max total time: ");
            Writer.WriteLine(MaxTotalTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Average total time: ");
            Writer.WriteLine(AverageTotalTime.ToString(@"dd\.hh\:mm\:ss"));
            Writer.Write("Min total time: ");
            Writer.WriteLine(MinTotalTime.ToString(@"dd\.hh\:mm\:ss"));
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileText(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\Run" + i + ".txt");
            }
        }

        /// <summary>
        /// Scrive il risultato del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FolderPath">Percorso della cartella dove salvare il file.</param>
        public override void WriteToFileBinary(string FolderPath)
        {
            string MonitoringStartTimeString = MonitoringStartTime.ToString("g");
            string MonitoringEndTimeString = MonitoringEndTime.ToString("g");
            string FilePath = FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\CpuStatistics.bin";
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(MaxCyclesCount);
            Writer.Write(AverageCyclesCount);
            Writer.Write(MinCyclesCount);
            Writer.Write(MaxKernelTime.Ticks);
            Writer.Write(AverageKernelTime.Ticks);
            Writer.Write(MinKernelTime.Ticks);
            Writer.Write(MaxUserTime.Ticks);
            Writer.Write(AverageUserTime.Ticks);
            Writer.Write(MinUserTime.Ticks);
            Writer.Write(MaxTotalTime.Ticks);
            Writer.Write(AverageTotalTime.Ticks);
            Writer.Write(MinTotalTime.Ticks);
            for (int i = 0; i < PerRuntimeStatistics.Count; i++)
            {
                PerRuntimeStatistics[i].WriteToFileBinary(FolderPath + "\\" + ProcessName.Remove(ProcessName.LastIndexOf('.')) + "\\" + MonitoringStartTimeString + "-" + MonitoringEndTimeString + "\\CpuData\\Run" + i + ".bin");
            }
        }
    }
}