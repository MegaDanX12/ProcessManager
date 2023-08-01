using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.WindowsInfo
{
    /// <summary>
    /// Rappresenta una proprietà di una finestra.
    /// </summary>
    public class WindowProperty
    {
        /// <summary>
        /// Nome della proprietà.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Valore della proprietà.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowProperty"/>.
        /// </summary>
        /// <param name="Name">Nome della proprietà.</param>
        /// <param name="Value">Valore della proprietà.</param>
        public WindowProperty(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
