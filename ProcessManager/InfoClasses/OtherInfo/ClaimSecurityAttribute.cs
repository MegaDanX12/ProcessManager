using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.OtherInfo
{
    /// <summary>
    /// Rappresenta un attributo di sicurezza che può essere associato a un token di accesso o a un contesto di autorizzazione.
    /// </summary>
    public class ClaimSecurityAttribute
    {
        /// <summary>
        /// Nome dell'attributo.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Tipo di dato dei valori dell'attributo.
        /// </summary>
        public string ValueType { get; }

        /// <summary>
        /// Caratteristiche dell'attributo.
        /// </summary>
        public string Flags { get; }

        /// <summary>
        /// Valori dell'attributo.
        /// </summary>
        public List<string> Values { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ClaimSecurityAttribute"/>.
        /// </summary>
        /// <param name="Name">Nome dell'attributo.</param>
        /// <param name="ValueType">Tipo dei valori dell'attributo.</param>
        /// <param name="Flags">Caratteristiche dell'attributo.</param>
        /// <param name="Values">Valori dell'attributo.</param>
        public ClaimSecurityAttribute(string Name, string ValueType, string Flags, List<string> Values)
        {
            this.Name = Name;
            this.ValueType = ValueType;
            this.Flags = Flags;
            this.Values = Values;
        }
    }
}