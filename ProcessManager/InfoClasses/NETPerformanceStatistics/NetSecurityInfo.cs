using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni sulla sicurezza in un processo .NET.
    /// </summary>
    public class NetSecurityInfo : IDisposable
    {
        /// <summary>
        /// Contatori prestazioni.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori contatori.
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetSecurityInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetSecurityInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                int LinkTimeChecksCount = Convert.ToInt32(Counters["LinkTimeChecksCount"].NextValue());
                int TimeInRTChecksPercentage = Convert.ToInt32(Counters["TimeInRTChecksPercentage"].NextValue());
                int StackWalkDepth = Convert.ToInt32(Counters["StackWalkDepth"].NextValue());
                int TotalRuntimeChecks = Convert.ToInt32(Counters["TotalRuntimeChecks"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLinkTimeChecksCount, LinkTimeChecksCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInRTChecksPercentage, TimeInRTChecksPercentage.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceStackWalkDepth, StackWalkDepth.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalRuntimeChecks, TotalRuntimeChecks.ToString("N0", CultureInfo.CurrentCulture));
            } 
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                int LinkTimeChecksCount = Convert.ToInt32(Counters["LinkTimeChecksCount"].NextValue());
                int TimeInRTChecksPercentage = Convert.ToInt32(Counters["TimeInRTChecksPercentage"].NextValue());
                int StackWalkDepth = Convert.ToInt32(Counters["StackWalkDepth"].NextValue());
                int TotalRuntimeChecks = Convert.ToInt32(Counters["TotalRuntimeChecks"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLinkTimeChecksCount] = LinkTimeChecksCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInRTChecksPercentage] = TimeInRTChecksPercentage.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceStackWalkDepth] = StackWalkDepth.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalRuntimeChecks] = TotalRuntimeChecks.ToString("N0", CultureInfo.CurrentCulture);
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