using System;
using System.ComponentModel;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni sui tempi di un processo e conteggi I/O.
    /// </summary>
    public struct ProcessTimesAndIOData
    {
        /// <summary>
        /// Tempo di esecuzione utente.
        /// </summary>
        public TimeSpan UserTime { get; }
        /// <summary>
        /// Tempo di esecuzione kernel.
        /// </summary>
        public TimeSpan KernelTime { get; }
        /// <summary>
        /// Numero di page fault incontrati.
        /// </summary>
        public uint PageFaultCount { get; }
        /// <summary>
        /// Operazioni di lettura eseguite.
        /// </summary>
        public ulong ReadOperationCount { get; }
        /// <summary>
        /// Operazioni di scrittura eseguite.
        /// </summary>
        public ulong WriteOperationCount { get; }
        /// <summary>
        /// Operazioni diverse da lettura e scrittura eseguite.
        /// </summary>
        public ulong OtherOperationCount { get; }
        /// <summary>
        /// Numero di byte letti.
        /// </summary>
        public ulong ReadBytesCount { get; }
        /// <summary>
        /// Numero di byte scritti.
        /// </summary>
        public ulong WriteBytesCount { get; }
        /// <summary>
        /// Numero di byte trasferiti in operazioni diverse da lettura e scrittura.
        /// </summary>
        public ulong OtherBytesCount { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="ProcessTimesAndIOData"/>.
        /// </summary>
        /// <param name="UserTime">Tempo di esecuzione utente.</param>
        /// <param name="KernelTime">Tempo di esecuzione kernel.</param>
        /// <param name="PageFaultCount">Conteggio di page fault incontrati.</param>
        /// <param name="ReadOperationCount">Operazioni di lettura eseguite.</param>
        /// <param name="WriteOperationCount">Operazioni di scrittura eseguite.</param>
        /// <param name="OtherOperationCount">Altre operazioni eseguite.</param>
        /// <param name="ReadBytesCount">Numero di byte letti.</param>
        /// <param name="WriteBytesCount">Numero di byte scritti.</param>
        /// <param name="OtherBytesCount">Numero di byte trasferiti in operazioni diverse da lettura e scrittura.</param>
        public ProcessTimesAndIOData(TimeSpan UserTime, TimeSpan KernelTime, uint PageFaultCount, ulong ReadOperationCount, ulong WriteOperationCount, ulong OtherOperationCount, ulong ReadBytesCount, ulong WriteBytesCount, ulong OtherBytesCount)
        {
            this.UserTime = UserTime;
            this.KernelTime = KernelTime;
            this.PageFaultCount = PageFaultCount;
            this.ReadOperationCount = ReadOperationCount;
            this.WriteOperationCount = WriteOperationCount;
            this.OtherOperationCount = OtherOperationCount;
            this.ReadBytesCount = ReadBytesCount;
            this.WriteBytesCount = WriteBytesCount;
            this.OtherBytesCount = OtherBytesCount;
        }
    }

    /// <summary>
    /// Informazioni sull'attività di un processo in un job.
    /// </summary>
    public class JobAssociatedProcessActivityInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Nome del processo.
        /// </summary>
        public string ProcessName { get; }

        /// <summary>
        /// Tempo di esecuzione in modalità utente del processo da quando è associato al job.
        /// </summary>
        public TimeSpan JobUserTime { get; }

        /// <summary>
        /// Percentuale del tempo di esecuzione in modalità utente del processo rispetto al tempo in modalità utente del job.
        /// </summary>
        public byte JobUserTimePercentage { get; }

        /// <summary>
        /// Tempo di esecuzione in modalità kernel del processo da quando è associato al job.
        /// </summary>
        public TimeSpan JobKernelTime { get; }

        /// <summary>
        /// Percentuale del tempo di esecuzione in modalità kernel del processo rispetto al tempo in modalità kernel del job.
        /// </summary>
        public byte JobKernelTimePercentage { get; }

        /// <summary>
        /// Tempo di esecuzione totale del processo da quando è associato al job.
        /// </summary>
        public TimeSpan JobTotalTime { get; }

        /// <summary>
        /// Percentuale del tempo di esecuzione totale del processo rispetto al tempo totale del job.
        /// </summary>
        public byte JobTotaleTimePercentage { get; }

        /// <summary>
        /// Numero di page fault incontrati dal processo da quando è associato al job.
        /// </summary>
        public uint JobPageFaultCount { get; }

        /// <summary>
        /// Percentuale del numero di page fault incontrati dal processo rispetto al numero totale di questi ultimi incontrati dal job.
        /// </summary>
        public byte JobPageFaultCountPercentage { get; }

        /// <summary>
        /// Numero di operazioni di lettura eseguite.
        /// </summary>
        public ulong JobReadOperationCount { get; }

        /// <summary>
        /// Percentuale del numero di operazioni di lettura eseguite rispetto al totale del job.
        /// </summary>
        public byte JobReadOperationCountPercentage { get; }

        /// <summary>
        /// Numero di operazioni di scrittura eseguite.
        /// </summary>
        public ulong JobWriteOperationCount { get; }

        /// <summary>
        /// Percentuale del numero di operazioni di scrittura eseguite rispetto al totale del job.
        /// </summary>
        public byte JobWriteOperationCountPercentage { get; }

        /// <summary>
        /// Numero di operazioni di I/O eseguite, diverse da lettura e scrittura.
        /// </summary>
        public ulong JobOtherOperationCount { get; }

        /// <summary>
        /// Percentuale del numero di operazioni diverse da lettura e scrittura eseguite rispetto al totale del job.
        /// </summary>
        public byte JobOtherOperationCountPercentage { get; }

        /// <summary>
        /// Numero di byte letti.
        /// </summary>
        public ulong JobReadTransferCount { get; }

        /// <summary>
        /// Percentuale del numero di byte letti rispetto al totale del job.
        /// </summary>
        public byte JobReadTransferCountPercentage { get; }

        /// <summary>
        /// Numero di byte scritti.
        /// </summary>
        public ulong JobWriteTransferCount { get; }

        /// <summary>
        /// Percentuale del numero di byte scritti rispetto al totale del job.
        /// </summary>
        public byte JobWriteTransferCountPercentage { get; }

        /// <summary>
        /// Numero di byte trasferiti durante operazioni diverse da letura e scrittura.
        /// </summary>
        public ulong JobOtherTransferCount { get; }

        /// <summary>
        /// Percentuale del numero di byte trasferiti in operazioni diverse da lettura e scrittura rispetto al totale del job.
        /// </summary>
        public byte JobOtherTransferCountPercentage { get; }

        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> associata al processo.
        /// </summary>
        private readonly ProcessInfo Info;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="JobAssociatedProcessActivityInfo"/>.
        /// </summary>
        /// <param name="UserTime">Tempo di esecuzione in modalità utente del job.</param>
        /// <param name="KernelTime">Tempo di esecuzione in modalità kernel del job.</param>
        /// <param name="PageFaultCount">Numero totale di page fault incontrati.</param>
        /// <param name="Info">Istanza di <see cref="ProcessInfo"/> associata al processo.</param>
        /// <param name="ReadOperationCount">Numero di operazioni di lettura eseguite.</param>
        /// <param name="WriteOperationCount">Numero di operazioni di scrittura eseguite.</param>
        /// <param name="OtherOperationCount">Numero di operazioni eseguite diverse da lettura e scrittura.</param>
        /// <param name="ReadTransferCount">Numero di byte letti.</param>
        /// <param name="WriteTransferCount">Numero di byte scritti.</param>
        /// <param name="OtherTransferCount">Numero di byte trasferiti durante operazioni diverse da lettura e scrittura.</param>
        public JobAssociatedProcessActivityInfo(ulong UserTime, ulong KernelTime, uint PageFaultCount, ProcessInfo Info, ulong ReadOperationCount, ulong WriteOperationCount, ulong OtherOperationCount, ulong ReadTransferCount, ulong WriteTransferCount, ulong OtherTransferCount)
        {
            this.Info = Info;
            ProcessTimesAndIOData Data = Info.GetProcessTimesAndIOData();
            ProcessName = Info.Name;
            JobUserTime = new(Data.UserTime.Ticks - (long)Info.ProcessLimiterJobAddTicksCount.Value);
            JobKernelTime = new(Data.KernelTime.Ticks - (long)Info.ProcessLimiterJobAddTicksCount.Value);
            JobTotalTime = JobUserTime + JobKernelTime;
            JobPageFaultCount = Data.PageFaultCount - Info.ProcessLimiterJobAddPageFaultCount.Value;
            JobReadOperationCount = Data.ReadOperationCount - Info.ProcessLimiterJobAddReadOperationCount.Value;
            JobWriteOperationCount = Data.WriteOperationCount - Info.ProcessLimiterJobAddWriteOperationCount.Value;
            JobOtherOperationCount = Data.OtherOperationCount - Info.ProcessLimiterJobAddOtherOperationCount.Value;
            JobReadTransferCount = Data.ReadBytesCount - Info.ProcessLimiterJobAddReadBytes.Value;
            JobWriteTransferCount = Data.WriteBytesCount - Info.ProcessLimiterJobAddWriteBytes.Value;
            JobOtherTransferCount = Data.OtherBytesCount - Info.ProcessLimiterJobAddOtherBytes.Value;
            JobUserTimePercentage = (byte)(100 * (ulong)JobUserTime.Ticks / UserTime);
            JobKernelTimePercentage = (byte)(100 * (ulong)JobKernelTime.Ticks / KernelTime);
            JobPageFaultCountPercentage = (byte)(100 * JobPageFaultCount / PageFaultCount);
            JobReadOperationCountPercentage = (byte)(100 * JobReadOperationCount / ReadOperationCount);
            JobWriteOperationCountPercentage = (byte)(100 * JobWriteOperationCount / WriteOperationCount);
            JobOtherOperationCountPercentage = (byte)(100 * JobOtherOperationCount / OtherOperationCount);
            JobReadTransferCountPercentage = (byte)(100 * JobReadTransferCount / ReadTransferCount);
            JobWriteTransferCountPercentage = (byte)(100 * JobWriteTransferCount / WriteTransferCount);
            JobOtherTransferCountPercentage = (byte)(100 * JobOtherTransferCount / OtherTransferCount);
        }
    }
}