using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni sui lock e i thread in un processo .NET.
    /// </summary>
    public class NetLockAndThreadInfo : IDisposable
    {
        /// <summary>
        /// Contatori prestazioni.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori dei contatori.
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetLockAndThreadInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetLockAndThreadInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                int LogicalThreadsCount = Convert.ToInt32(Counters["LogicalThreadsCount"].NextValue());
                int PhysicalThreadsCount = Convert.ToInt32(Counters["PhysicalThreadsCount"].NextValue());
                int RecognizedThreadsCount = Convert.ToInt32(Counters["RecognizedThreadsCount"].NextValue());
                int TotalRecognizedThreadsCount = Convert.ToInt32(Counters["TotalRecognizedThreadsCount"].NextValue());
                int ContentionRatePerSec = Convert.ToInt32(Counters["ContentionRatePerSec"].NextValue());
                int CurrentQueueLength = Convert.ToInt32(Counters["CurrentQueueLength"].NextValue());
                int QueueLengthPerSec = Convert.ToInt32(Counters["QueueLengthPerSec"].NextValue());
                int QueueLengthPeak = Convert.ToInt32(Counters["QueueLengthPeak"].NextValue());
                int RecognizedThreadsRatePerSec = Convert.ToInt32(Counters["RecognizedThreadsRatePerSec"].NextValue());
                int TotalContentionsCount = Convert.ToInt32(Counters["TotalContentionsCount"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLogicalThreadsCount, LogicalThreadsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformancePhysicalThreadsCount, PhysicalThreadsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceRecognizedThreadsCount, RecognizedThreadsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalRecognizedThreadsCount, TotalRecognizedThreadsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceContentionRatePerSec, ContentionRatePerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentQueueLength, CurrentQueueLength.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceQueueLengthPerSec, QueueLengthPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceQueueLengthPeak, QueueLengthPeak.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceRecognizedThreadsRatePerSec, RecognizedThreadsRatePerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalContentionsCount, TotalContentionsCount.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                int LogicalThreadsCount = Convert.ToInt32(Counters["LogicalThreadsCount"].NextValue());
                int PhysicalThreadsCount = Convert.ToInt32(Counters["PhysicalThreadsCount"].NextValue());
                int RecognizedThreadsCount = Convert.ToInt32(Counters["RecognizedThreadsCount"].NextValue());
                int TotalRecognizedThreadsCount = Convert.ToInt32(Counters["TotalRecognizedThreadsCount"].NextValue());
                int ContentionRatePerSec = Convert.ToInt32(Counters["ContentionRatePerSec"].NextValue());
                int CurrentQueueLength = Convert.ToInt32(Counters["CurrentQueueLength"].NextValue());
                int QueueLengthPerSec = Convert.ToInt32(Counters["QueueLengthPerSec"].NextValue());
                int QueueLengthPeak = Convert.ToInt32(Counters["QueueLengthPeak"].NextValue());
                int RecognizedThreadsRatePerSec = Convert.ToInt32(Counters["RecognizedThreadsRatePerSec"].NextValue());
                int TotalContentionsCount = Convert.ToInt32(Counters["TotalContentionsCount"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLogicalThreadsCount] = LogicalThreadsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformancePhysicalThreadsCount] = PhysicalThreadsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceRecognizedThreadsCount] = RecognizedThreadsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalRecognizedThreadsCount] = TotalRecognizedThreadsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceContentionRatePerSec] = ContentionRatePerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentQueueLength] = CurrentQueueLength.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceQueueLengthPerSec] = QueueLengthPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceQueueLengthPeak] = QueueLengthPeak.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceRecognizedThreadsRatePerSec] = RecognizedThreadsRatePerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalContentionsCount] = TotalContentionsCount.ToString("N0", CultureInfo.CurrentCulture);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (PerformanceCounter counter in Counters.Values)
                    {
                        counter.Close();
                        counter.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}