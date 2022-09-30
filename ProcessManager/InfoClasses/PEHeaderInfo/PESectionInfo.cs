using System;
using System.Globalization;

namespace ProcessManager.InfoClasses.PEInfo
{
    /// <summary>
    /// Informazioni su una sezione di un'intestazione PE.
    /// </summary>
    public class PESectionInfo
    {
        /// <summary>
        /// Nome della sezione.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indirizzo virtuale nell'immagine.
        /// </summary>
        public string VirtualAddress { get; }

        /// <summary>
        /// Dimensione della sezione.
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PESectionInfo"/>.
        /// </summary>
        /// <param name="Name">Nome della sezione.</param>
        /// <param name="VirtualAddress">Indirizzo virtuale nell'intestazione.</param>
        /// <param name="Size">Dimensione della sezione.</param>
        public PESectionInfo(string Name, IntPtr VirtualAddress, uint Size)
        {
            this.Name = Name;
            this.VirtualAddress = "0x" + VirtualAddress.ToString("X");
            this.Size = "0x" + Size.ToString("X", CultureInfo.CurrentCulture);
        }
    }
}