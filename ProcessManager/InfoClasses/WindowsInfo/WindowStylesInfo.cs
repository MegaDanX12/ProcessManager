using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.WindowsInfo
{
    /// <summary>
    /// Contiene informazioni sugli stili della finestra.
    /// </summary>
    public class WindowStylesInfo
    {
        /// <summary>
        /// Valore stili della finestra.
        /// </summary>
        public string WindowStylesValue { get; }

        /// <summary>
        /// Stili della finestra.
        /// </summary>
        public List<string> WindowStyles { get; }

        /// <summary>
        /// Valore stili estesi della finestra.
        /// </summary>
        public string WindowExtendedStylesValue { get; }

        /// <summary>
        /// Stili estesi della finestra.
        /// </summary>
        public List<string> WindowExtendedStyles { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowStylesInfo"/>.
        /// </summary>
        /// <param name="WindowStylesValue">Valore stili della finestra.</param>
        /// <param name="WindowStyles">Stili della finestra.</param>
        /// <param name="WindowExtendedStylesValue">Valore stili estesi della finestra.</param>
        /// <param name="WindowExtendedStyles">Stili estesi della finestra.</param>
        public WindowStylesInfo(string WindowStylesValue, List<string> WindowStyles, string WindowExtendedStylesValue, List<string> WindowExtendedStyles)
        {
            this.WindowStylesValue = WindowStylesValue;
            this.WindowStyles = WindowStyles;
            this.WindowExtendedStylesValue = WindowExtendedStylesValue;
            this.WindowExtendedStyles = WindowExtendedStyles;
        }
    }
}