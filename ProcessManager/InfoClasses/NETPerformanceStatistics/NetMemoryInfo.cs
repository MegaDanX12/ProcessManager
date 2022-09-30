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
    /// Informazioni sulla memoria utilizzata da un processo .NET.
    /// </summary>
    public class NetMemoryInfo : IDisposable
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
        /// Inizializza una nuova istanza di <see cref="NetMemoryInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatori prestazioni.</param>
        public NetMemoryInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                long HeapsBytes = Convert.ToInt64(Counters["HeapsBytes"].NextValue());
                long GCHandles = Convert.ToInt64(Counters["GCHandles"].NextValue());
                long Gen0Collections = Convert.ToInt64(Counters["Gen0Collections"].NextValue());
                long Gen1Collections = Convert.ToInt64(Counters["Gen1Collections"].NextValue());
                long Gen2Collections = Convert.ToInt64(Counters["Gen2Collections"].NextValue());
                long InducedGC = Convert.ToInt64(Counters["InducedGC"].NextValue());
                long PinnedObjectsCount = Convert.ToInt64(Counters["PinnedObjectsCount"].NextValue());
                long SynchronizationBlocksInUseCount = Convert.ToInt64(Counters["SynchronizationBlocksInUseCount"].NextValue());
                long TotalCommittedBytesCount = Convert.ToInt64(Counters["TotalCommittedBytesCount"].NextValue());
                long TotalReservedBytesCount = Convert.ToInt64(Counters["TotalReservedBytesCount"].NextValue());
                long TimeInGCPercentage = Convert.ToInt64(Counters["TimeInGCPercentage"].NextValue());
                long AllocatedBytesPerSecond = Convert.ToInt64(Counters["AllocatedBytesPerSecond"].NextValue());
                long FinalizationSurvivors = Convert.ToInt64(Counters["FinalizationSurvivors"].NextValue());
                long Gen0HeapSize = Convert.ToInt64(Counters["Gen0HeapSize"].NextValue());
                long Gen0PromotedBytesPerSec = Convert.ToInt64(Counters["Gen0PromotedBytesPerSec"].NextValue());
                long Gen1HeapSize = Convert.ToInt64(Counters["Gen1HeapSize"].NextValue());
                long Gen1PromotedBytesPerSec = Convert.ToInt64(Counters["Gen1PromotedBytesPerSec"].NextValue());
                long Gen2HeapSize = Convert.ToInt64(Counters["Gen2HeapSize"].NextValue());
                long LargeObjectHeapSize = Convert.ToInt64(Counters["LargeObjectHeapSize"].NextValue());
                long PromotedFinalizationMemoryFromGen0 = Convert.ToInt64(Counters["PromotedFinalizationMemoryFromGen0"].NextValue());
                long PromotedMemoryFromGen0 = Convert.ToInt64(Counters["PromotedMemoryFromGen0"].NextValue());
                long PromotedMemoryFromGen1 = Convert.ToInt64(Counters["PromotedMemoryFromGen1"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHeapsBytes, HeapsBytes.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGCHandles, GCHandles.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0Collections, Gen0Collections.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1Collections, Gen1Collections.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen2Collections, Gen2Collections.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceInducedGC, InducedGC.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformancePinnedObjectsCount, PinnedObjectsCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceSyncBlocksInUseCount, SynchronizationBlocksInUseCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalCommittedBytesCount, TotalCommittedBytesCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalReservedBytesCount, TotalReservedBytesCount.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInGCPercentage, TimeInGCPercentage.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceAllocatedBytesPerSecond, AllocatedBytesPerSecond.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceFinalizazionSurvivors, FinalizationSurvivors.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0HeapSize, Gen0HeapSize.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0PromotedBytesPerSec, Gen0PromotedBytesPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1HeapSize, Gen1HeapSize.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1PromotedBytesPerSec, Gen1PromotedBytesPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceGen2HeapSize, Gen2HeapSize.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceLargeObjectHeapSize, LargeObjectHeapSize.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedFinalizationMemoryFromGen0, PromotedFinalizationMemoryFromGen0.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedMemoryFromGen0, PromotedMemoryFromGen0.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedMemoryFromGen1, PromotedMemoryFromGen1.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                long HeapsBytes = Convert.ToInt64(Counters["HeapsBytes"].NextValue());
                long GCHandles = Convert.ToInt64(Counters["GCHandles"].NextValue());
                long Gen0Collections = Convert.ToInt64(Counters["Gen0Collections"].NextValue());
                long Gen1Collections = Convert.ToInt64(Counters["Gen1Collections"].NextValue());
                long Gen2Collections = Convert.ToInt64(Counters["Gen2Collections"].NextValue());
                long InducedGC = Convert.ToInt64(Counters["InducedGC"].NextValue());
                long PinnedObjectsCount = Convert.ToInt64(Counters["PinnedObjectsCount"].NextValue());
                long SynchronizationBlocksInUseCount = Convert.ToInt64(Counters["SynchronizationBlocksInUseCount"].NextValue());
                long TotalCommittedBytesCount = Convert.ToInt64(Counters["TotalCommittedBytesCount"].NextValue());
                long TotalReservedBytesCount = Convert.ToInt64(Counters["TotalReservedBytesCount"].NextValue());
                long TimeInGCPercentage = Convert.ToInt64(Counters["TimeInGCPercentage"].NextValue());
                long AllocatedBytesPerSecond = Convert.ToInt64(Counters["AllocatedBytesPerSecond"].NextValue());
                long FinalizationSurvivors = Convert.ToInt64(Counters["FinalizationSurvivors"].NextValue());
                long Gen0HeapSize = Convert.ToInt64(Counters["Gen0HeapSize"].NextValue());
                long Gen0PromotedBytesPerSec = Convert.ToInt64(Counters["Gen0PromotedBytesPerSec"].NextValue());
                long Gen1HeapSize = Convert.ToInt64(Counters["Gen1HeapSize"].NextValue());
                long Gen1PromotedBytesPerSec = Convert.ToInt64(Counters["Gen1PromotedBytesPerSec"].NextValue());
                long Gen2HeapSize = Convert.ToInt64(Counters["Gen2HeapSize"].NextValue());
                long LargeObjectHeapSize = Convert.ToInt64(Counters["LargeObjectHeapSize"].NextValue());
                long PromotedFinalizationMemoryFromGen0 = Convert.ToInt64(Counters["PromotedFinalizationMemoryFromGen0"].NextValue());
                long PromotedMemoryFromGen0 = Convert.ToInt64(Counters["PromotedMemoryFromGen0"].NextValue());
                long PromotedMemoryFromGen1 = Convert.ToInt64(Counters["PromotedMemoryFromGen1"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHeapsBytes] = HeapsBytes.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGCHandles] = GCHandles.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0Collections] = Gen0Collections.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1Collections] = Gen1Collections.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen2Collections] = Gen2Collections.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceInducedGC] = InducedGC.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformancePinnedObjectsCount] = PinnedObjectsCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceSyncBlocksInUseCount] = SynchronizationBlocksInUseCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalCommittedBytesCount] = TotalCommittedBytesCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTotalReservedBytesCount] = TotalReservedBytesCount.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceTimeInGCPercentage] = TimeInGCPercentage.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceAllocatedBytesPerSecond] = AllocatedBytesPerSecond.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceFinalizazionSurvivors] = FinalizationSurvivors.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0HeapSize] = Gen0HeapSize.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen0PromotedBytesPerSec] = Gen0PromotedBytesPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1HeapSize] = Gen1HeapSize.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen1PromotedBytesPerSec] = Gen1PromotedBytesPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceGen2HeapSize] = Gen2HeapSize.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceLargeObjectHeapSize] = LargeObjectHeapSize.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedFinalizationMemoryFromGen0] = PromotedFinalizationMemoryFromGen0.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedMemoryFromGen0] = PromotedMemoryFromGen0.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformancePromotedMemoryFromGen1] = PromotedMemoryFromGen1.ToString("N0", CultureInfo.CurrentCulture);
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