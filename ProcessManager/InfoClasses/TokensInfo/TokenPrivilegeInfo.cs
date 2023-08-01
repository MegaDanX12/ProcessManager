using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.TokensInfo
{
    /// <summary>
    /// Contiene informazioni su un privilegio applicato a un token di accesso.
    /// </summary>
    public class TokenPrivilegeInfo
    {
        /// <summary>
        /// Nome del privilegio.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Stato del privilegio.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Decrizione del privilegio.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TokenPrivilegeInfo"/>.
        /// </summary>
        /// <param name="Name">Nome del privilegio.</param>
        /// <param name="Status">Stato del privilegio.</param>
        /// <param name="Description">Descrizione del privilegio.</param>
        public TokenPrivilegeInfo(string Name, string Status, string Description)
        {
            this.Name = Name;
            this.Status = Status;
            this.Description = Description;
        }
    }
}