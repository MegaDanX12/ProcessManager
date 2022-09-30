using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni sulle eccezioni in un processo .NET.
    /// </summary>
    public class NetExceptionsInfo : IDisposable
    {
        /// <summary>
        /// Contatore delle prestazioni da cui recuperare i dati.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori dei contatori.
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetExceptionsInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori delle prestazioni.</param>
        public NetExceptionsInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                int ExceptionsThrownCount = Convert.ToInt32(Counters["ExceptionsThrownCount"].NextValue());
                int ExceptionsThrownPerSec = Convert.ToInt32(Counters["ExceptionsThrownPerSec"].NextValue());
                int FiltersPerSec = Convert.ToInt32(Counters["FiltersPerSec"].NextValue());
                int FinallysPerSec = Convert.ToInt32(Counters["FinallysPerSec"].NextValue());
                int ThrowToCatchDepthPerSec = Convert.ToInt32(Counters["CatchDepth"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceExceptionsCount, ExceptionsThrownCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceExceptionsPerSec, ExceptionsThrownPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceFiltersPerSec, FiltersPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceFinallysPerSec, FinallysPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCatchDepth, ThrowToCatchDepthPerSec.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                int ExceptionsThrownCount = Convert.ToInt32(Counters["ExceptionsThrownCount"].NextValue());
                int ExceptionsThrownPerSec = Convert.ToInt32(Counters["ExceptionsThrownPerSec"].NextValue());
                int FiltersPerSec = Convert.ToInt32(Counters["FiltersPerSec"].NextValue());
                int FinallysPerSec = Convert.ToInt32(Counters["FinallysPerSec"].NextValue());
                int ThrowToCatchDepthPerSec = Convert.ToInt32(Counters["CatchDepth"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceExceptionsCount] = ExceptionsThrownCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceExceptionsPerSec] = ExceptionsThrownPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceFiltersPerSec] = FiltersPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceFinallysPerSec] = FinallysPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCatchDepth] = ThrowToCatchDepthPerSec.ToString("N0", CultureInfo.CurrentCulture);
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