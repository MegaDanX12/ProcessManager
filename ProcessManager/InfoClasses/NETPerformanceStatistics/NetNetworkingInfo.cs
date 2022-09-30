using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace ProcessManager.InfoClasses.NETPerformanceStatistics
{
    /// <summary>
    /// Informazioni di rete di un processo .NET.
    /// </summary>
    public class NetNetworkingInfo : IDisposable
    {
        /// <summary>
        /// Contatori prestazioni.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> Counters;
        private bool disposedValue;

        /// <summary>
        /// Valori dei contatori (numeri interi).
        /// </summary>
        public Dictionary<string, string> Values { get; } = new();

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="NetNetworkingInfo"/>.
        /// </summary>
        /// <param name="Counters">Contatore prestazioni.</param>
        public NetNetworkingInfo(Dictionary<string, PerformanceCounter> Counters)
        {
            Contract.Requires(Counters != null);
            this.Counters = Counters;
            if (Counters.Count > 0)
            {
                long BytesReceived = Convert.ToInt64(Counters["BytesReceived"].NextValue());
                long BytesSent = Convert.ToInt64(Counters["BytesSent"].NextValue());
                int ConnectionsEstablished = Convert.ToInt32(Counters["ConnectionsEstablished"].NextValue());
                int DatagramsReceived = Convert.ToInt32(Counters["DatagramsReceived"].NextValue());
                int DatagramsSent = Convert.ToInt32(Counters["DatagramsSent"].NextValue());
                int HttpWebRequestsAverageLifetime = Convert.ToInt32(Counters["HttpWebRequestsAverageLifetime"].NextValue());
                int HttpWebRequestsAverageQueueTime = Convert.ToInt32(Counters["HttpWebRequestsAverageQueueTime"].NextValue());
                int HttpWebRequestsCreatedPerSec = Convert.ToInt32(Counters["HttpWebRequestsCreatedPerSec"].NextValue());
                int HttpWebRequestsQueuedPerSec = Convert.ToInt32(Counters["HttpWebRequestsQueuedPerSec"].NextValue());
                int HttpWebRequestsAbortedPerSec = Convert.ToInt32(Counters["HttpWebRequestsAbortedPerSec"].NextValue());
                int HttpWebRequestsFailedPerSec = Convert.ToInt32(Counters["HttpWebRequestsFailedPerSec"].NextValue());
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceBytesReceived, BytesReceived.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceBytesSent, BytesSent.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceConnectionsEstablished, ConnectionsEstablished.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceDatagramsReceived, DatagramsReceived.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceDatagramsSent, DatagramsSent.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAverageLifetime, HttpWebRequestsAverageLifetime.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAverageQueueTime, HttpWebRequestsAverageQueueTime.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRCreatedPerSec, HttpWebRequestsCreatedPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRQueuedPerSec, HttpWebRequestsQueuedPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAbortedPerSec, HttpWebRequestsAbortedPerSec.ToString("N0", CultureInfo.CurrentCulture));
                Values.Add(Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRFailedPerSec, HttpWebRequestsFailedPerSec.ToString("N0", CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void Update()
        {
            if (Counters.Count > 0)
            {
                long BytesReceived = Convert.ToInt64(Counters["BytesReceived"].NextValue());
                long BytesSent = Convert.ToInt64(Counters["BytesSent"].NextValue());
                int ConnectionsEstablished = Convert.ToInt32(Counters["ConnectionsEstablished"].NextValue());
                int DatagramsReceived = Convert.ToInt32(Counters["DatagramsReceived"].NextValue());
                int DatagramsSent = Convert.ToInt32(Counters["DatagramsSent"].NextValue());
                int HttpWebRequestsAverageLifetime = Convert.ToInt32(Counters["HttpWebRequestsAverageLifetime"].NextValue());
                int HttpWebRequestsAverageQueueTime = Convert.ToInt32(Counters["HttpWebRequestsAverageQueueTime"].NextValue());
                int HttpWebRequestsCreatedPerSec = Convert.ToInt32(Counters["HttpWebRequestsCreatedPerSec"].NextValue());
                int HttpWebRequestsQueuedPerSec = Convert.ToInt32(Counters["HttpWebRequestsQueuedPerSec"].NextValue());
                int HttpWebRequestsAbortedPerSec = Convert.ToInt32(Counters["HttpWebRequestsAbortedPerSec"].NextValue());
                int HttpWebRequestsFailedPerSec = Convert.ToInt32(Counters["HttpWebRequestsFailedPerSec"].NextValue());
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceBytesReceived] = BytesReceived.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceBytesSent] = BytesSent.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceConnectionsEstablished] = ConnectionsEstablished.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceDatagramsReceived] = DatagramsReceived.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceDatagramsSent] = DatagramsSent.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAverageLifetime] = HttpWebRequestsAverageLifetime.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAverageQueueTime] = HttpWebRequestsAverageQueueTime.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRCreatedPerSec] = HttpWebRequestsCreatedPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRQueuedPerSec] = HttpWebRequestsQueuedPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRAbortedPerSec] = HttpWebRequestsAbortedPerSec.ToString("N0", CultureInfo.CurrentCulture);
                Values[Properties.Resources.ProcessPropertiesWindowNetPerformanceHWRFailedPerSec] = HttpWebRequestsFailedPerSec.ToString("N0", CultureInfo.CurrentCulture);
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