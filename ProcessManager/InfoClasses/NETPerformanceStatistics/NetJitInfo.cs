using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni sulla compilazione JIT in un processo .NET.
    /// </summary>
    public class NetJitInfo : IDisposable
    {
        /// <summary>
        /// Contatori di prestazioni.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori contatori.
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetJitInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetJitInfo(Dictionary<string, PerformanceCounter> Counters)
        {

            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                long ILBytesJittedCount = Convert.ToInt64(Counters["ILBytesJittedCount"].NextValue());
                int MethodsJittedCount = Convert.ToInt32(Counters["MethodsJittedCount"].NextValue());
                int TimeInJitPercentage = Convert.ToInt32(Counters["TimeInJitPercentage"].NextValue());
                long ILBytesJittedPerSec = Convert.ToInt64(Counters["ILBytesJittedPerSec"].NextValue());
                int StandardJitFailures = Convert.ToInt32(Counters["StandardJitFailures"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceILBytesJittedCount, ILBytesJittedCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceMethodsJittedCount, MethodsJittedCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInJitPercentage, TimeInJitPercentage.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceILBytesJittedPerSec, ILBytesJittedPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceStandardJitFailures, StandardJitFailures.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                long ILBytesJittedCount = Convert.ToInt64(Counters["ILBytesJittedCount"].NextValue());
                int MethodsJittedCount = Convert.ToInt32(Counters["MethodsJittedCount"].NextValue());
                int TimeInJitPercentage = Convert.ToInt32(Counters["TimeInJitPercentage"].NextValue());
                long ILBytesJittedPerSec = Convert.ToInt64(Counters["ILBytesJittedPerSec"].NextValue());
                int StandardJitFailures = Convert.ToInt32(Counters["StandardJitFailures"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceILBytesJittedCount] = ILBytesJittedCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceMethodsJittedCount] = MethodsJittedCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInJitPercentage] = TimeInJitPercentage.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceILBytesJittedPerSec] = ILBytesJittedPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceStandardJitFailures] = StandardJitFailures.ToString("N0", CultureInfo.CurrentCulture);
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