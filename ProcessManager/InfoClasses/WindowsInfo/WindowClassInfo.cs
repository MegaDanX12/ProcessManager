using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.WindowsInfo
{
    /// <summary>
    /// Contiene informazioni sulla classe di una finestra.
    /// </summary>
    public class WindowClassInfo
    {
        /// <summary>
        /// Atom che identifica la classe di cui fa parte la finestra.
        /// </summary>
        public string WindowClassAtom { get; }

        /// <summary>
        /// Valore stili della classe.
        /// </summary>
        public string WindowClassStyles { get; }

        /// <summary>
        /// Valore dell'handle del modulo che ha creato la classe.
        /// </summary>
        public string WindowClassModuleInstanceHandleValue { get; }

        /// <summary>
        /// Valore dell'handle dell'icona associata alla classe.
        /// </summary>
        public string WindowClassIconHandleValue { get; }

        /// <summary>
        /// Valore dell'handle dell'icona piccola associata alla classe.
        /// </summary>
        public string WindowClassSmallIconHandleValue { get; }

        /// <summary>
        /// Valore dell'handle dell'cursore associato alla classe.
        /// </summary>
        public string WindowClassCursorHandleValue { get; }

        /// <summary>
        /// Valore dell'handle del pennello del background associato alla classe.
        /// </summary>
        public string WindowClassBackgroundBrushHandleValue { get; }

        /// <summary>
        /// Valore dell'handle al nome del menù associato alla classe.
        /// </summary>
        public string WindowClassMenuNameHandleValue { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowClassInfo"/>.
        /// </summary>
        /// <param name="ClassAtom">Valore Atom della classe.</param>
        /// <param name="ClassStyles">Valore stili della classe</param>
        /// <param name="ClassModuleInstanceHandleValue">Valore dell'handle all'istanza del modulo che ha creato la classe.</param>
        /// <param name="ClassIconHandleValue">Valore dell'handle all'icona associata alla classe.</param>
        /// <param name="ClassSmallIconHandleValue">Valore dell'handle all'icona piccola associata alla classe.</param>
        /// <param name="ClassCursorHandleValue">Valore dell'handle al cursore associato alla classe.</param>
        /// <param name="ClassBackgroundBrushHandleValue">Valore dell'handle al pennello dello sfondo associato alla classe.</param>
        /// <param name="ClassMenuNameHandleValue">Valore all'handle del nome del menù associato alla classe.</param>
        public WindowClassInfo(string ClassAtom, string ClassStyles, string ClassModuleInstanceHandleValue, string ClassIconHandleValue, string ClassSmallIconHandleValue, string ClassCursorHandleValue, string ClassBackgroundBrushHandleValue, string ClassMenuNameHandleValue)
        {
            WindowClassAtom = ClassAtom;
            WindowClassStyles = ClassStyles;
            WindowClassModuleInstanceHandleValue = ClassModuleInstanceHandleValue;
            WindowClassIconHandleValue = ClassIconHandleValue;
            WindowClassSmallIconHandleValue = ClassSmallIconHandleValue;
            WindowClassCursorHandleValue = ClassCursorHandleValue;
            WindowClassBackgroundBrushHandleValue = ClassBackgroundBrushHandleValue;
            WindowClassMenuNameHandleValue = ClassMenuNameHandleValue;
        }
    }
}