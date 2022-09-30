using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.ProcessStatistics
{
    /// <summary>
    /// Conteggio degli handle di uno specifico tipo.
    /// </summary>
    public class HandleCountInfo
    {
        /// <summary>
        /// Tipo di handle.
        /// </summary>
        public string HandleType { get; }

        /// <summary>
        /// Numero di handle.
        /// </summary>
        public string Count { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="HandleCountInfo"/>.
        /// </summary>
        /// <param name="HandleType">Tipo di handle.</param>
        /// <param name="Count">Numero di handle.</param>
        public HandleCountInfo(string HandleType, uint Count)
        {
            this.HandleType = HandleType;
            this.Count = Count.ToString("N0", CultureInfo.CurrentCulture);
        }
    }
}