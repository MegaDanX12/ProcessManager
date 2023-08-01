using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.WindowsInfo
{
    /// <summary>
    /// Contiene informazioni generali su una finestra.
    /// </summary>
    public class WindowGeneralInfo
    {
        /// <summary>
        /// Nome della classe.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Valore dell'handle alla finestra.
        /// </summary>
        public string HandleValue { get; }

        /// <summary>
        /// Nome del processo, PID e TID.
        /// </summary>
        public string ThreadProcessIDAndName { get; }

        /// <summary>
        /// Dati sul rettangolo.
        /// </summary>
        public string RectangleInfo { get; }

        /// <summary>
        /// Dati sul rettangolo dell'area client.
        /// </summary>
        public string ClientAreaRectangleInfo { get; }

        /// <summary>
        /// Valore dell'handle all'istanza dell'applicazione.
        /// </summary>
        public string ApplicationInstanceHandleValue { get; }

        /// <summary>
        /// Valore dell'handle al menù assegnato alla finestra.
        /// </summary>
        public string MenuHandleValue { get; }

        /// <summary>
        /// Valore dati utente.
        /// </summary>
        public string UserData { get; }

        /// <summary>
        /// Indica se la finestra è una finestra Unicode nativa.
        /// </summary>
        public bool IsUnicode { get; }

        /// <summary>
        /// Indirizzo di memoria dove si trova la procedura principale della finestra.
        /// </summary>
        public string WindowProcedureMemoryAddress { get; }

        /// <summary>
        /// Indirizzo di memoria dove si trova la procedura principale (se la finestra è una finestra di dialogo).
        /// </summary>
        public string DialogProcedureMemoryAddress { get; }

        /// <summary>
        /// ID del controllo.
        /// </summary>
        public string DialogControlID { get; }

        /// <summary>
        /// Titolo.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Nome del modulo associato.
        /// </summary>
        public string WindowAssociatedModuleName { get; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowGeneralInfo"/>.
        /// </summary>
        /// <param name="ClassName">Nome della classe a cui appartiene la finestra.</param>
        /// <param name="HandleValue">Valore dell'handle alla finestra.</param>
        /// <param name="ThreadProcessIDAndName">Nome del processo, PID e TID associati alla finestra.</param>
        /// <param name="RectangleData">Dati sul rettangolo della finestra.</param>
        /// <param name="RectangleSize">Dimensione del rettangolo della finestra.</param>
        /// <param name="ClientAreaRectangleData">Dati sul rettangolo dell'area client della finestra.</param>
        /// <param name="ClientAreaSize">Dimensione del rettangolo dell'area client della finestra.</param>
        /// <param name="ApplicationInstanceHandleValue">Valore dell'handle all'istanza dell'applicazione.</param>
        /// <param name="MenuHandleValue">Valore dell'handle al menù assegnato alla finestra.</param>
        /// <param name="UserData">Dati utente.</param>
        /// <param name="IsUnicode">Indica se la finestra è una finestra native Unicode.</param>
        /// <param name="WindowProcedureHandleValue">Indirizzo di memoria della procedura principale della finestra.</param>
        /// <param name="DialogProcedureHandleValue">Indirizzo di memoria della procedura principale della finestra (se la finestra è una finestra di dialogo).</param>
        /// <param name="DialogControlID">ID del controllo della finestra di dialogo.</param>
        public WindowGeneralInfo(string ClassName, string HandleValue, string ThreadProcessIDAndName, string RectangleData, string ClientAreaRectangleData, string ApplicationInstanceHandleValue,  string MenuHandleValue, string UserData, bool IsUnicode, string WindowProcedureHandleValue, string DialogProcedureHandleValue, string DialogControlID, string WindowTitle, string WindowAssociatedModuleName)
        {
            this.ClassName = ClassName;
            this.HandleValue = HandleValue;
            this.ThreadProcessIDAndName = ThreadProcessIDAndName;
            this.RectangleInfo = RectangleData;
            this.ClientAreaRectangleInfo = ClientAreaRectangleData;
            this.ApplicationInstanceHandleValue = ApplicationInstanceHandleValue;
            this.MenuHandleValue = MenuHandleValue;
            this.UserData = UserData;
            this.IsUnicode = IsUnicode;
            WindowProcedureMemoryAddress = WindowProcedureHandleValue;
            DialogProcedureMemoryAddress = DialogProcedureHandleValue;
            this.DialogControlID = DialogControlID;
            Title = WindowTitle;
            this.WindowAssociatedModuleName = WindowAssociatedModuleName;
        }
    }
}