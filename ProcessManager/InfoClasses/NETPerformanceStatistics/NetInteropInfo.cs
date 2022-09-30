using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni di interazione con codice non gestito in un processo .NET.
    /// </summary>
    public class NetInteropInfo : IDisposable
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
        /// Inizializza una nuova istanza di <see cref="NetInteropInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetInteropInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count >  0)
            {
                int CCWsCount = Convert.ToInt32(Counters["CCWsCount"].NextValue());
                int MarshallingCount = Convert.ToInt32(Counters["MarshallingCount"].NextValue());
                int StubsCount = Convert.ToInt32(Counters["StubsCount"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCCWsCount, CCWsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceMarshallingCount, MarshallingCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceStubsCount, StubsCount.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                int CCWsCount = Convert.ToInt32(Counters["CCWsCount"].NextValue());
                int MarshallingCount = Convert.ToInt32(Counters["MarshallingCount"].NextValue());
                int StubsCount = Convert.ToInt32(Counters["StubsCount"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCCWsCount] = CCWsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceMarshallingCount] = MarshallingCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceStubsCount] = StubsCount.ToString("N0", CultureInfo.CurrentCulture);
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