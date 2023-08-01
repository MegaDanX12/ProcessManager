using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProcessManager.Watchdog
{
    /// <summary>
    /// Dati sul limite di utilizzo CPU per un gruppo di applicazioni.
    /// </summary>
    public class CpuUsageLimitsData : IDisposable
    {
        /// <summary>
        /// Limite di utilizzo (in percentuale).
        /// </summary>
        public byte UsageLimit { get; }

        /// <summary>
        /// Percorsi completi delle applicazioni su cui applicare il limite.
        /// </summary>
        public ObservableCollection<string> ExecutablePaths { get; }

        /// <summary>
        /// Processi a cui il limite è attualmente applicato.
        /// </summary>
        public ObservableCollection<ProcessInfo> LimitedProcesses { get; }

        /// <summary>
        /// Handle nativo al job associato a questo limite.
        /// </summary>
        private readonly IntPtr JobHandle;

        private bool disposedValue;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="CpuUsageLimitsData"/>.
        /// </summary>
        /// <param name="UsageLimit">Limite di utilizzo (in percentuale).</param>
        /// <param name="JobHandle">Handle nativo al job associato.</param>
        public CpuUsageLimitsData(byte UsageLimit, IntPtr JobHandle)
        {
            this.UsageLimit = UsageLimit;
            ExecutablePaths = new();
            LimitedProcesses = new();
            this.JobHandle = JobHandle;
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="CpuUsageLimitsData"/>.
        /// </summary>
        /// <param name="UsageLimit">Limite di utilizzo (in percentuale).</param>
        /// <param name="JobHandle">Handle nativo al job associato.</param>
        /// <param name="ExecutablePaths">Percorsi completi degli eseguibili limitati.</param>
        public CpuUsageLimitsData(byte UsageLimit, IntPtr JobHandle, List<string> ExecutablePaths)
        {
            this.UsageLimit = UsageLimit;
            this.ExecutablePaths = new(ExecutablePaths);
            LimitedProcesses = new();
            this.JobHandle = JobHandle;
        }

        /// <summary>
        /// Recupera informazioni sul job associato al limite e sull'attività dei processi limitati attualmente in esecuzione.
        /// </summary>
        /// <returns>Istanza di <see cref="JobInfo"/> con le informazioni.</returns>
        public JobInfo GetAssociatedJobData()
        {
            return NativeHelpers.GetJobData(JobHandle, LimitedProcesses.ToList());
        }

        /// <summary>
        /// Aggiorna i dati sul job associato al limite.
        /// </summary>
        /// <returns>Un'istanza di <see cref="JobInfo"/> con i dati aggiornati.</returns>
        public JobInfo UpdateJobData()
        {
            return NativeHelpers.GetJobData(JobHandle);
        }

        /// <summary>
        /// Aggiunge un processo ai processi limitati.
        /// </summary>
        /// <param name="Process">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool AddProcess(ProcessInfo Process)
        {
            if (Process.LimitProcessCpuUsage(JobHandle))
            {
                Process.ProcessLimiterJobAddTicksCount = (ulong)DateTime.Now.Ticks;
                ProcessTimesAndIOData Data = Process.GetProcessTimesAndIOData();
                Process.ProcessLimiterJobAddPageFaultCount = Data.PageFaultCount;
                Process.ProcessLimiterJobAddReadOperationCount = Data.ReadOperationCount;
                Process.ProcessLimiterJobAddWriteOperationCount = Data.WriteOperationCount;
                Process.ProcessLimiterJobAddOtherOperationCount = Data.OtherOperationCount;
                Process.ProcessLimiterJobAddReadBytes = Data.ReadBytesCount;
                Process.ProcessLimiterJobAddWriteBytes = Data.WriteBytesCount;
                Process.ProcessLimiterJobAddOtherBytes = Data.OtherBytesCount;
                Process.Exit += Process_Exit;
                LimitedProcesses.Add(Process);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Aggiunge un processo al job associato a questo limite.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Il processo aggiunto al job non viene inserito nella lista dei processi limitati.</remarks>
        public bool AddProcess(SafeProcessHandle ProcessHandle)
        {
            return NativeHelpers.AddProcessToJob(JobHandle, ProcessHandle);
        }

        private void Process_Exit(object sender, EventArgs e)
        {
            (sender as ProcessInfo).Exit -= Process_Exit;
            _ = LimitedProcesses.Remove(sender as ProcessInfo);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _ = NativeHelpers.CloseHandle(JobHandle);
                disposedValue = true;
            }
        }

        ~CpuUsageLimitsData()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}