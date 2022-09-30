using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.TokensInfo
{
    /// <summary>
    /// Informazioni su un gruppo incluso in un token di accesso.
    /// </summary>
    public class TokenGroupInfo
    {
        /// <summary>
        /// Nome del gruppo.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Caratteristiche del gruppo.
        /// </summary>
        public string Flags { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TokenGroupInfo"/>.
        /// </summary>
        /// <param name="Name">Nome del gruppo.</param>
        /// <param name="Flags">Caratteristiche del gruppo.</param>
        public TokenGroupInfo(string Name, string Flags)
        {
            this.Name = Name;
            this.Flags = Flags;
        }
    }
}