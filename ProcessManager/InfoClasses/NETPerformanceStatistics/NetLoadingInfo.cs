using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni di caricamento in un processo .NET.
    /// </summary>
    public class NetLoadingInfo : IDisposable
    {
        /// <summary>
        /// Contatori di prestazioni.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori dei contatori.
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetLoadingInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetLoadingInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                long LoaderHeapBytes = Convert.ToInt64(Counters["LoaderHeapBytes"].NextValue());
                int CurrentAppdomains = Convert.ToInt32(Counters["CurrentAppdomainsCount"].NextValue());
                int CurrentAssemblies = Convert.ToInt32(Counters["CurrentAssembliesCount"].NextValue());
                int CurrentClassesLoaded = Convert.ToInt32(Counters["CurrentClassesLoadedCount"].NextValue());
                int AppdomainsRate = Convert.ToInt32(Counters["AppdomainsRate"].NextValue());
                int UnloadedAppdomainsRate = Convert.ToInt32(Counters["UnloadedAppdomainsRate"].NextValue());
                int AssembliesRate = Convert.ToInt32(Counters["AssembliesRate"].NextValue());
                int ClassesLoadedRate = Convert.ToInt32(Counters["ClassesLoadedRate"].NextValue());
                int LoadFailuresRate = Convert.ToInt32(Counters["LoadFailuresRate"].NextValue());
                int LoadFailuresTotalCount = Convert.ToInt32(Counters["LoadFailuresTotalCount"].NextValue());
                int TotalAppdomainsCount = Convert.ToInt32(Counters["TotalAppdomainsCount"].NextValue());
                int TotalAppdomainsUnloadedCount = Convert.ToInt32(Counters["TotalAppdomainsUnloadedCount"].NextValue());
                int TotalAssembliesCount = Convert.ToInt32(Counters["TotalAssembliesCount"].NextValue());
                int TotalClassesLoadedCount = Convert.ToInt32(Counters["TotalClassesLoadedCount"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLoaderHeapBytes, LoaderHeapBytes.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentAppdomansCount, CurrentAppdomains.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentAssembliesCount, CurrentAssemblies.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentClassesLoadedCount, CurrentClassesLoaded.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceAppdomainsRate, AppdomainsRate.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceUnloadedAppdomainsRate, UnloadedAppdomainsRate.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceAssembliesRate, AssembliesRate.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceClassesLoadedRate, ClassesLoadedRate.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLoadFailuresRate, LoadFailuresRate.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLoadFailuresTotalCount, LoadFailuresTotalCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAppdomainsCount, TotalAppdomainsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAppdomainsUnloadedCount, TotalAppdomainsUnloadedCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAssembliesCount, TotalAssembliesCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalClassesLoadedCount, TotalClassesLoadedCount.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                long LoaderHeapBytes = Convert.ToInt64(Counters["LoaderHeapBytes"].NextValue());
                int CurrentAppdomains = Convert.ToInt32(Counters["CurrentAppdomainsCount"].NextValue());
                int CurrentAssemblies = Convert.ToInt32(Counters["CurrentAssembliesCount"].NextValue());
                int CurrentClassesLoaded = Convert.ToInt32(Counters["CurrentClassesLoadedCount"].NextValue());
                int AppdomainsRate = Convert.ToInt32(Counters["AppdomainsRate"].NextValue());
                int UnloadedAppdomainsRate = Convert.ToInt32(Counters["UnloadedAppdomainsRate"].NextValue());
                int AssembliesRate = Convert.ToInt32(Counters["AssembliesRate"].NextValue());
                int ClassesLoadedRate = Convert.ToInt32(Counters["ClassesLoadedRate"].NextValue());
                int LoadFailuresRate = Convert.ToInt32(Counters["LoadFailuresRate"].NextValue());
                int LoadFailuresTotalCount = Convert.ToInt32(Counters["LoadFailuresTotalCount"].NextValue());
                int TotalAppdomainsCount = Convert.ToInt32(Counters["TotalAppdomainsCount"].NextValue());
                int TotalAppdomainsUnloadedCount = Convert.ToInt32(Counters["TotalAppdomainsUnloadedCount"].NextValue());
                int TotalAssembliesCount = Convert.ToInt32(Counters["TotalAssembliesCount"].NextValue());
                int TotalClassesLoadedCount = Convert.ToInt32(Counters["TotalClassesLoadedCount"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLoaderHeapBytes] = LoaderHeapBytes.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentAppdomansCount] = CurrentAppdomains.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentAssembliesCount] = CurrentAssemblies.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceCurrentClassesLoadedCount] = CurrentClassesLoaded.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceAppdomainsRate] = AppdomainsRate.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceUnloadedAppdomainsRate] = UnloadedAppdomainsRate.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceAssembliesRate] = AssembliesRate.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceClassesLoadedRate] = ClassesLoadedRate.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLoadFailuresRate] = LoadFailuresRate.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLoadFailuresTotalCount] = LoadFailuresTotalCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAppdomainsCount] = TotalAppdomainsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAppdomainsUnloadedCount] = TotalAppdomainsUnloadedCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalAssembliesCount] = TotalAssembliesCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalClassesLoadedCount] = TotalClassesLoadedCount.ToString("N0", CultureInfo.CurrentCulture);
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