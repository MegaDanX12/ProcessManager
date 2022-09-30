using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.ViewModels
{
    public class PeHeaderInfoVM
    {
        /// <summary>
        /// Informazioni sull'intestazione PE.
        /// </summary>
        public PEHeaderInfo HeaderInfo { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PeHeaderInfoVM"/>.
        /// </summary>
        /// <param name="HeaderInfo">Informazioni sull'intestazione.</param>
        public PeHeaderInfoVM(PEHeaderInfo HeaderInfo)
        {
            this.HeaderInfo = HeaderInfo;
        }
    }
}