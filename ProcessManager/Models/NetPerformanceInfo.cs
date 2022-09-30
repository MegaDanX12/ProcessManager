using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using ProcessManager.InfoClasses.NETPerformanceStatistics;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni sulle prestazioni di un processo .NET.
    /// </summary>
    public class NetPerformanceInfo : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Informazioni sulle eccezioni.
        /// </summary>
        public NetExceptionsInfo ExceptionsInfo { get; }

        /// <summary>
        /// Informazioni sull'interazione con il codice non gestito.
        /// </summary>
        public NetInteropInfo InteropInfo { get; }

        /// <summary>
        /// Informazioni sulla compilazione JIT.
        /// </summary>
        public NetJitInfo JitInfo { get; }

        /// <summary>
        /// Informazioni di caricamento.
        /// </summary>
        public NetLoadingInfo LoadingInfo { get; }

        /// <summary>
        /// Informazioni sui lock e i thread.
        /// </summary>
        public NetLockAndThreadInfo LockAndThreadInfo { get; }

        /// <summary>
        /// Informazioni sulla memoria utilizzata.
        /// </summary>
        public NetMemoryInfo MemoryInfo { get; }

        /// <summary>
        /// Informazioni di rete.
        /// </summary>
        public NetNetworkingInfo NetworkingInfo { get; }

        /// <summary>
        /// Informazioni sulla sicurezza.
        /// </summary>
        public NetSecurityInfo SecurityInfo { get; }

        /// <summary>
        /// Timer di aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateTimer;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetPerformanceInfo"/>.
        /// </summary>
        /// <param name="Counters">Dizionario che contiene tutti i contatori delle prestazioni relativi al CLR dove appare il processo.</param>
        public NetPerformanceInfo(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Contract.Requires(Counters != null);
            if (Counters.ContainsKey("Exceptions"))
            {
                ExceptionsInfo = new NetExceptionsInfo(GetExceptionsCounters(Counters));
            }
            if (Counters.ContainsKey("Interop"))
            {
                InteropInfo = new NetInteropInfo(GetInteropCounters(Counters));
            }
            if (Counters.ContainsKey("JIT"))
            {
                JitInfo = new NetJitInfo(GetJitCounters(Counters));
            }
            if (Counters.ContainsKey("Loading"))
            {
                LoadingInfo = new NetLoadingInfo(GetLoadingCounters(Counters));
            }
            if (Counters.ContainsKey("LockAndThreads"))
            {
                LockAndThreadInfo = new NetLockAndThreadInfo(GetLockAndThreadsCounters(Counters));
            }
            if (Counters.ContainsKey("Memory"))
            {
                MemoryInfo = new NetMemoryInfo(GetMemoryCounters(Counters));
            }
            if (Counters.ContainsKey("Networking"))
            {
                NetworkingInfo = new NetNetworkingInfo(GetNetworkingCounters(Counters));
            }
            if (Counters.ContainsKey("Security"))
            {
                SecurityInfo = new NetSecurityInfo(GetSecurityCounters(Counters));
            }
        }
        #region Counters Retrieval Methods
        /// <summary>
        /// Recupera i contatori prestazioni relativi alle eccezioni.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetExceptionsCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Exceptions"])
            {
                if (counter.CounterName.Equals("# of Exceps Thrown", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ExceptionsThrownCount", counter);
                }
                else if (counter.CounterName.Equals("# of Exceps Thrown / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ExceptionsThrownPerSec", counter);
                }
                else if (counter.CounterName.Equals("# of Filters / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("FiltersPerSec", counter);
                }
                else if (counter.CounterName.Equals("# of Finallys / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("FinallysPerSec", counter);
                }
                else if (counter.CounterName.Equals("Throw To Catch Depth / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CatchDepth", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi all'interazione con il codice non gestito.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetInteropCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Interop"])
            {
                if (counter.CounterName.Equals("# of CCWs", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CCWsCount", counter);
                }
                else if (counter.CounterName.Equals("# of marshalling", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("MarshallingCount", counter);
                }
                else if (counter.CounterName.Equals("# of Stubs", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("StubsCount", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi alla compilazione JIT.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetJitCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["JIT"])
            {
                if (counter.CounterName.Equals("# of IL Bytes JITted", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ILBytesJittedCount", counter);
                }
                else if (counter.CounterName.Equals("# of Methods JITted", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("MethodsJittedCount", counter);
                }
                else if (counter.CounterName.Equals("% Time in Jit", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TimeInJitPercentage", counter);
                }
                else if (counter.CounterName.Equals("IL Bytes Jitted / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ILBytesJittedPerSec", counter);
                }
                else if (counter.CounterName.Equals("Standard Jit Failures", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("StandardJitFailures", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi al caricamento di classi, assembly e domini applicazione.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetLoadingCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Loading"])
            {
                if (counter.CounterName.Equals("Bytes in Loader Heap", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LoaderHeapBytes", counter);
                }
                else if (counter.CounterName.Equals("Current appdomains", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CurrentAppdomainsCount", counter);
                }
                else if (counter.CounterName.Equals("Current Assemblies", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CurrentAssembliesCount", counter);
                }
                else if (counter.CounterName.Equals("Current Classes Loaded", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CurrentClassesLoadedCount", counter);
                }
                else if (counter.CounterName.Equals("Rate of appdomains", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("AppdomainsRate", counter);
                }
                else if (counter.CounterName.Equals("Rate of appdomains unloaded", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("UnloadedAppdomainsRate", counter);
                }
                else if (counter.CounterName.Equals("Rate of Assemblies", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("AssembliesRate", counter);
                }
                else if (counter.CounterName.Equals("Rate of Classes Loaded", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ClassesLoadedRate", counter);
                }
                else if (counter.CounterName.Equals("Rate of Load Failures", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LoadFailuresRate", counter);
                }
                else if (counter.CounterName.Equals("Total # of Load Failures", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LoadFailuresTotalCount", counter);
                }
                else if (counter.CounterName.Equals("Total Appdomains", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalAppdomainsCount", counter);
                }
                else if (counter.CounterName.Equals("Total appdomains unloaded", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalAppdomainsUnloadedCount", counter);
                }
                else if (counter.CounterName.Equals("Total Assemblies", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalAssembliesCount", counter);
                }
                else if (counter.CounterName.Equals("Total Classes Loaded", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalClassesLoadedCount", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi ai lock e ai thread.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetLockAndThreadsCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["LockAndThreads"])
            {
                if (counter.CounterName.Equals("# of current logical Threads", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LogicalThreadsCount", counter);
                }
                else if (counter.CounterName.Equals("# of current physical Threads", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("PhysicalThreadsCount", counter);
                }
                else if (counter.CounterName.Equals("# of current recognized Threads", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("RecognizedThreadsCount", counter);
                }
                else if (counter.CounterName.Equals("# of total recognized Threads", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalRecognizedThreadsCount", counter);
                }
                else if (counter.CounterName.Equals("Contention Rate / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ContentionRatePerSec", counter);
                }
                else if (counter.CounterName.Equals("Current Queue Length", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("CurrentQueueLength", counter);
                }
                else if (counter.CounterName.Equals("Queue Length / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("QueueLengthPerSec", counter);
                }
                else if (counter.CounterName.Equals("Queue Length Peak", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("QueueLengthPeak", counter);
                }
                else if (counter.CounterName.Equals("rate of recognized threads / sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("RecognizedThreadsRatePerSec", counter);
                }
                else if (counter.CounterName.Equals("Total # of Contentions", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalContentionsCount", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi alla memoria.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetMemoryCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Memory"])
            {
                if (counter.CounterName.Equals("# Bytes in all Heaps", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HeapsBytes", counter);
                }
                else if (counter.CounterName.Equals("# GC Handles", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("GCHandles", counter);
                }
                else if (counter.CounterName.Equals("# Gen 0 Collections", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen0Collections", counter);
                }
                else if (counter.CounterName.Equals("# Gen 1 Collections", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen1Collections", counter);
                }
                else if (counter.CounterName.Equals("# Gen 2 Collections", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen2Collections", counter);
                }
                else if (counter.CounterName.Equals("# Induced GC", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("InducedGC", counter);
                }
                else if (counter.CounterName.Equals("# of Pinned Objects", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("PinnedObjectsCount", counter);
                }
                else if (counter.CounterName.Equals("# of Sink Blocks in use", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("SynchronizationBlocksInUseCount", counter);
                }
                else if (counter.CounterName.Equals("# Total committed Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalCommittedBytesCount", counter);
                }
                else if (counter.CounterName.Equals("# Total reserved Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalReservedBytesCount", counter);
                }
                else if (counter.CounterName.Equals("% Time in GC", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TimeInGCPercentage", counter);
                }
                else if (counter.CounterName.Equals("Allocated Bytes/sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("AllocatedBytesPerSecond", counter);
                }
                else if (counter.CounterName.Equals("Finalization Survivors", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("FinalizationSurvivors", counter);
                }
                else if (counter.CounterName.Equals("Gen 0 heap size", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen0HeapSize", counter);
                }
                else if (counter.CounterName.Equals("Gen 0 Promoted Bytes/Sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen0PromotedBytesPerSec", counter);
                }
                else if (counter.CounterName.Equals("Gen 1 Heap size", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen1HeapSize", counter);
                }
                else if (counter.CounterName.Equals("Gen 1 Promoted Bytes/Sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen1PromotedBytesPerSec", counter);
                }
                else if (counter.CounterName.Equals("Gen 2 Heap size", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("Gen2HeapSize", counter);
                }
                else if (counter.CounterName.Equals("Large Object Heap size", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LargeObjectHeapSize", counter);
                }
                else if (counter.CounterName.Equals("Promoted Finalization-Memory from Gen 0", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("PromotedFinalizationMemoryFromGen0", counter);
                }
                else if (counter.CounterName.Equals("Promoted Memory from Gen 0", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("PromotedMemoryFromGen0", counter);
                }
                else if (counter.CounterName.Equals("Promoted Memory from Gen 1", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("PromotedMemoryFromGen1", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi alla rete.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetNetworkingCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Networking"])
            {
                if (counter.CounterName.Equals("Bytes Received", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("BytesReceived", counter);
                }
                else if (counter.CounterName.Equals("Bytes Sent", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("BytesSent", counter);
                }
                else if (counter.CounterName.Equals("Connections Established", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("ConnectionsEstablished", counter);
                }
                else if (counter.CounterName.Equals("Datagrams Received", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("DatagramsReceived", counter);
                }
                else if (counter.CounterName.Equals("Datagrams Sent", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("DatagramsSent", counter);
                }
                else if (counter.CounterName.Equals("HttpWebRequests Average Lifetime", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HttpWebRequestsAverageLifetime", counter);
                }
                else if (counter.CounterName.Equals("HttpWebRequests Average Queue Time", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HttpWebRequestsAverageQueueTime", counter);
                }
                else if (counter.CounterName.Equals("HttpWebRequests Created/sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HttpWebRequestsCreatedPerSec", counter);
                }
                else if (counter.CounterName.Equals("HttpWebRequests Aborted/sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HttpWebRequestsAbortedPerSec", counter);
                }
                else if (counter.CounterName.Equals("HttpWebRequests Failed/sec", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("HttpWebRequestsFailedPerSec", counter);
                }
            }
            return SpecificCounters;
        }

        /// <summary>
        /// Recupera i contatori prestazioni relativi alla sicurezza.
        /// </summary>
        /// <param name="Counters">Dizionario con tutti i contatori.</param>
        /// <returns>Un dizionario che contiene i contatori specifici.</returns>
        private static Dictionary<string, PerformanceCounter> GetSecurityCounters(Dictionary<string, List<PerformanceCounter>> Counters)
        {
            Dictionary<string, PerformanceCounter> SpecificCounters = new Dictionary<string, PerformanceCounter>();
            foreach (PerformanceCounter counter in Counters["Security"])
            {
                if (counter.CounterName.Equals("# Link Time Checks", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("LinkTimeChecksCount", counter);
                }
                else if (counter.CounterName.Equals("% Time in RT checks", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TimeInRTChecksPercentage", counter);
                }
                else if (counter.CounterName.Equals("Stack Walk Depth", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("StackWalkDepth", counter);
                }
                else if (counter.CounterName.Equals("Total Runtime Checks", StringComparison.OrdinalIgnoreCase))
                {
                    SpecificCounters.Add("TotalRuntimeChecks", counter);
                }
            }
            return SpecificCounters;
        }
        #endregion

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            ExceptionsInfo.Update();
            InteropInfo.Update();
            JitInfo.Update();
            LoadingInfo.Update();
            LockAndThreadInfo.Update();
            MemoryInfo.Update();
            if (NetworkingInfo != null)
            {
                NetworkingInfo.Update();
            }
            SecurityInfo.Update();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ExceptionsInfo.Dispose();
                    InteropInfo.Dispose();
                    JitInfo.Dispose();
                    LoadingInfo.Dispose();
                    LockAndThreadInfo.Dispose();
                    MemoryInfo.Dispose();
                    if (NetworkingInfo != null)
                    {
                        NetworkingInfo.Dispose();
                    }
                    SecurityInfo.Dispose();
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