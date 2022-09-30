using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.ProcessStatistics;
using ProcessManager.WMI;
using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatisticsClasses
{
    /// <summary>
    /// Contiene le statistiche CPU di un processo.
    /// </summary>
    public class CPUStatistics : StatisticsBase
    {
        /// <summary>
        /// Priorità base.
        /// </summary>
        public string BasePriority { get; }

        private string CyclesValue;

        /// <summary>
        /// Cicli di esecuzione.
        /// </summary>
        public string Cycles
        {
            get => CyclesValue;
            private set
            {
                if (CyclesValue != value)
                {
                    CyclesValue = value;
                    NotifyPropertyChanged(nameof(Cycles));
                }
            }
        }

        /// <summary>
        /// Numero cicli di esecuzione.
        /// </summary>
        public ulong CyclesCount { get; private set; }

        private string KernelTimeValue;

        /// <summary>
        /// Tempo di esecuzione kernel.
        /// </summary>
        public string KernelTime
        {
            get => KernelTimeValue;
            private set
            {
                if (KernelTimeValue != value)
                {
                    KernelTimeValue = value;
                    NotifyPropertyChanged(nameof(KernelTime));
                }
            }
        }

        private string UserTimeValue;

        /// <summary>
        /// Tempo di esecuzione utente.
        /// </summary>
        public string UserTime
        {
            get => UserTimeValue;
            private set
            {
                if (UserTimeValue != value)
                {
                    UserTimeValue = value;
                    NotifyPropertyChanged(nameof(UserTime));
                }
            }
        }

        private string TotalTimeValue;

        /// <summary>
        /// Tempo di esecuzione totale.
        /// </summary>
        public string TotalTime
        {
            get => TotalTimeValue;
            private set
            {
                if (TotalTime != value)
                {
                    TotalTimeValue = value;
                    NotifyPropertyChanged(nameof(TotalTime));
                }
            }
        }

        /// <summary>
        /// Tempi del processo.
        /// </summary>
        public TimeSpan?[] ProcessTimes { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="CPUStatistics"/>.
        /// </summary>
        /// <param name="PID">ID del processo.</param>
        /// <param name="Handle">Handle al processo.</param>
        public CPUStatistics(uint PID, SafeProcessHandle Handle)
        {
            ProcessTimes = NativeHelpers.GetProcessTimes(Handle);
            BasePriority = WMIProcessInfoMethods.GetProcessBasePriority(PID);
            CyclesCount = NativeHelpers.GetProcessCPUCycles(Handle);
            CyclesValue = CyclesCount.ToString("N0", CultureInfo.CurrentCulture);
            KernelTimeValue = ProcessTimes[0].HasValue ? GetProcessTime(ProcessTimes[0].Value) : Properties.Resources.UnavailableText;
            UserTimeValue = ProcessTimes[1].HasValue ? GetProcessTime(ProcessTimes[1].Value) : Properties.Resources.UnavailableText;
            TotalTimeValue = ProcessTimes[2].HasValue ? GetProcessTime(ProcessTimes[2].Value) : Properties.Resources.UnavailableText;
        }

        /// <summary>
        /// Recupera il tempo di esecuzione dato del processo come stringa.
        /// </summary>
        /// <param name="Time">Tempo di esecuzione.</param>
        /// <returns>Il tempo di esecuzione fornito come stringa.</returns>
        private static string GetProcessTime(TimeSpan Time)
        {
            return Time.Days.ToString("D2", CultureInfo.CurrentCulture) + ":" + Time.Hours.ToString("D2", CultureInfo.CurrentCulture) + ":" + Time.Minutes.ToString("D2", CultureInfo.CurrentCulture) + ":" + Time.Seconds.ToString("D2", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public override void Update(SafeProcessHandle Handle)
        {
            ProcessTimes = NativeHelpers.GetProcessTimes(Handle);
            CyclesCount = NativeHelpers.GetProcessCPUCycles(Handle);
            Cycles = CyclesCount.ToString("N0", CultureInfo.CurrentCulture);
            KernelTime = ProcessTimes[0].HasValue ? GetProcessTime(ProcessTimes[0].Value) : Properties.Resources.UnavailableText;
            UserTime = ProcessTimes[1].HasValue ? GetProcessTime(ProcessTimes[1].Value) : Properties.Resources.UnavailableText;
            TotalTime = ProcessTimes[2].HasValue ? GetProcessTime(ProcessTimes[2].Value) : Properties.Resources.UnavailableText;
        }

        /// <summary>
        /// Scrive su file XML i dati di questa istanza.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileXML(string FilePath)
        {
            string DocumentString =
                "<CpuStatistics>" + Environment.NewLine +
                "  <CyclesCount>" + Cycles + "</CyclesCount>" + Environment.NewLine;
            if (ProcessTimes[0].HasValue)
            {
                DocumentString += "   <KernelTime>" + ProcessTimes[0].Value.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</KernelTime>" + Environment.NewLine;
            }
            else
            {
                DocumentString += "   <KernelTime>0</KernelTime>" + Environment.NewLine;
            }
            if (ProcessTimes[1].HasValue)
            {
                DocumentString += "   <UserTime>" + ProcessTimes[1].Value.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</UserTime>" + Environment.NewLine;
            }
            else
            {
                DocumentString += "   <UserTime>0</UserTime>" + Environment.NewLine;
            }
            if (ProcessTimes[2].HasValue)
            {
                DocumentString += "   <TotalTime>" + ProcessTimes[2].Value.Ticks.ToString("N0", CultureInfo.InvariantCulture) + "</TotalTime>" + Environment.NewLine;
            }
            else
            {
                DocumentString += "   <TotalTime>0</TotalTime>" + Environment.NewLine +
                    "</CpuStatistics>";
            }
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
        }

        /// <summary>
        /// Scrive su un file di testo i dati di questa istanza.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileText(string FilePath)
        {
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Cpu statistics on process termination");
            Writer.WriteLine();
            Writer.WriteLine("Cycles count: " + Cycles);
            if (ProcessTimes[0].HasValue)
            {
                Writer.WriteLine("Kernel time: " + ProcessTimes[0].Value.ToString(@"dd\.hh\:mm\:ss"));
            }
            else
            {
                Writer.WriteLine("Kernel time: 0");
            }
            if (ProcessTimes[1].HasValue)
            {
                Writer.WriteLine("User time: " + ProcessTimes[1].Value.ToString(@"dd\.hh\:mm\:ss"));
            }
            else
            {
                Writer.WriteLine("User time: 0");
            }
            if (ProcessTimes[2].HasValue)
            {
                Writer.WriteLine("Total time: " + ProcessTimes[2].Value.ToString(@"dd\.hh\:mm\:ss"));
            }
            else
            {
                Writer.WriteLine("Total time: 0");
            }
        }

        /// <summary>
        /// Scrive in un file binario i dati di questa istanza.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileBinary(string FilePath)
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(CyclesCount);
            if (ProcessTimes[0].HasValue)
            {
                Writer.Write(ProcessTimes[0].Value.Ticks);
            }
            else
            {
                Writer.Write((long)0);
            }
            if (ProcessTimes[1].HasValue)
            {
                Writer.Write(ProcessTimes[1].Value.Ticks);
            }
            else
            {
                Writer.Write((long)0);
            }
            if (ProcessTimes[2].HasValue)
            {
                Writer.Write(ProcessTimes[2].Value.Ticks);
            }
            else
            {
                Writer.Write((long)0);
            }
        }
    }
}