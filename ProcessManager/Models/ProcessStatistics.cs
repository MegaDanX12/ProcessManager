using ProcessManager.InfoClasses.ProcessStatisticsClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    public class ProcessStatistics
    {
        /// <summary>
        /// Statistiche CPU.
        /// </summary>
        public CPUStatistics CPU { get; }

        /// <summary>
        /// Statistiche memoria.
        /// </summary>
        public MemoryStatistics Memory { get; }

        /// <summary>
        /// Statistiche I/O.
        /// </summary>
        public IOStatistics IO { get; }

        /// <summary>
        /// Statistiche handle.
        /// </summary>
        public HandleStatistics Handle { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="ProcessStatistics"/>.
        /// </summary>
        /// <param name="CPU">Statistiche CPU.</param>
        /// <param name="Memory">Statistiche della memoria.</param>
        /// <param name="IO">Statistiche I/O.</param>
        /// <param name="Handle">Statistiche handle.</param>
        /// <param name="Network">Statistiche rete.</param>
        public ProcessStatistics(CPUStatistics CPU, MemoryStatistics Memory, IOStatistics IO, HandleStatistics Handle)
        {
            this.CPU = CPU;
            this.Memory = Memory;
            this.IO = IO;
            this.Handle = Handle;
        }
    }
}