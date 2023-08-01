using ProcessManager.InfoClasses.PEInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni sull'intestazione PE di un eseguibile.
    /// </summary>
    public class PEHeaderInfo
    {
        /// <summary>
        /// Informazioni generali sull'immagine.
        /// </summary>
        public PEHeaderGeneralInfo GeneralInfo { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PEHeaderInfo"/>.
        /// </summary>
        /// <param name="GeneralInfo">Informazioni generali.</param>
        public PEHeaderInfo(PEHeaderGeneralInfo GeneralInfo)
        {
            this.GeneralInfo = GeneralInfo;
        }
    }
}