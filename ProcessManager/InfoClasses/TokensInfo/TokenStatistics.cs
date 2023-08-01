using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.TokensInfo
{
    /// <summary>
    /// Contiene le statistiche di un token di accesso.
    /// </summary>
    public class TokenStatistics
    {
        /// <summary>
        /// Tipo di token (primario o di impersonazione).
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Livello di impersonazione.
        /// </summary>
        public string ImpersonationLevel { get; }

        /// <summary>
        /// ID del token.
        /// </summary>
        public string TokenLUID { get; }

        /// <summary>
        /// ID della sessione che il token rappresenta.
        /// </summary>
        public string AuthenticationLUID { get; }

        /// <summary>
        /// Quantità di memoria utilizzata.
        /// </summary>
        public string MemoryUsage { get; }

        /// <summary>
        /// Quantità di memoria disponibile.
        /// </summary>
        public string MemoryAvailable { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TokenStatistics"/>.
        /// </summary>
        /// <param name="Type">Tipo di token.</param>
        /// <param name="ImpersonationLevel">Livello di impersonazione.</param>
        /// <param name="TokenLUID">ID del token.</param>
        /// <param name="AuthenticationLUID">ID dell'autenticatore.</param>
        /// <param name="MemoryUsage">Memoria utilizzata.</param>
        /// <param name="MemoryAvailable">Memoria disponibile.</param>
        public TokenStatistics(string Type, string ImpersonationLevel, string TokenLUID, string AuthenticationLUID, string MemoryUsage, string MemoryAvailable)
        {
            this.Type = Type;
            this.ImpersonationLevel = ImpersonationLevel;
            this.TokenLUID = TokenLUID;
            this.AuthenticationLUID = AuthenticationLUID;
            this.MemoryUsage = MemoryUsage;
            this.MemoryAvailable = MemoryAvailable;
        }
    }
}