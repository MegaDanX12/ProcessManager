using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace ProcessManager.InfoClasses.ServicesInfo
{
    /// <summary>
    /// Rappresenta un servizio.
    /// </summary>
    public class Service : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Handle nativo al servizio.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Nome del servizio nel database.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Nome del servizio utilizzato dalle applicazioni.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// ID del processo associato al servizio.
        /// </summary>
        /// <remarks>Se il servizio non è in esecuzione, questo valore è 0.</remarks>
        public uint PID { get; private set; }

        private string StateValue;

        /// <summary>
        /// Stato del servizio.
        /// </summary>
        public string State 
        { 
            get
            {
                return StateValue;
            }
            private set
            {
                if (StateValue != value)
                {
                    StateValue = value;
                    NotifyPropertyChanged(nameof(State));
                }
            }
        }

        /// <summary>
        /// Tipo di servizio.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Comandi accettati dal servizio.
        /// </summary>
        public List<string> AcceptedControls { get; }

        /// <summary>
        /// Indica se il servizio è in esecuzione in un processo di sistema.
        /// </summary>
        public string RunsInSystemProcess { get; }

        /// <summary>
        /// Tipo di avvio.
        /// </summary>
        public string StartType { get; }

        /// <summary>
        /// Modalità di controllo errori.
        /// </summary>
        public string ErrorControlMode { get; }

        /// <summary>
        /// Gruppo di caricamento.
        /// </summary>
        public string LoadOrderGroup { get; }

        /// <summary>
        /// Tag identificativo.
        /// </summary>
        public string TagID { get; }

        /// <summary>
        /// Servizi e gruppi di caricamento da cui il servizio dipende.
        /// </summary>
        public List<string> Dependencies { get; }

        /// <summary>
        /// Nome dell'account sotto cui il servizio viene eseguito.
        /// </summary>
        public string ServiceStartUserAccountName { get; }

        /// <summary>
        /// Indica se l'auto-avvio del servizio deve essere ritardato.
        /// </summary>
        public string IsAutoStartDelayed { get; }

        /// <summary>
        /// Descrizione del servizio.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Azioni da eseguire in caso di crash del servizio.
        /// </summary>
        public ServiceFailureActions FailureActions { get; }

        /// <summary>
        /// Indica se eseguire le azioni indicate nel campo <see cref="FailureActions"/> anche quando il servizio non va in crash.
        /// </summary>
        public string ExecuteFailureActionsOnNormalFailure { get; }

        /// <summary>
        /// Nodo preferito.
        /// </summary>
        public string PreferredNode { get; }

        /// <summary>
        /// Tempo per eseguire le azione prespegnimento.
        /// </summary>
        public string PreshutdownTimeout { get; }

        /// <summary>
        /// Privilegi richiesti per il servizio.
        /// </summary>
        public List<string> RequiredPrivileges { get; }

        /// <summary>
        /// Trigger del servizio.
        /// </summary>
        public List<ServiceTrigger> Triggers { get; }

        /// <summary>
        /// Tipo di protezione del servizio.
        /// </summary>
        public string LaunchProtectionType { get; }

        /// <summary>
        /// Tipo di SID del servizio.
        /// </summary>
        public string SIDType { get; }

        /// <summary>
        /// Tempo di reset delle azioni in caso di crash.
        /// </summary>
        public string FailureActionsResetPeriod { get; }

        private bool disposedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="Service"/>.
        /// </summary>
        /// <param name="Handle">Handle nativo al servizio.</param>
        /// <param name="Name">Nome del servizio nel database.</param>
        /// <param name="DisplayName">Nome del servizio utilizzato dalle applicazioni.</param>
        /// <param name="PID">ID del processo associato al servizio.</param>
        /// <param name="State">Stato del servizio.</param>
        /// <param name="Type">Tipo di servizio.</param>
        /// <param name="AcceptedControls">Comandi accettati dal servizio.</param>
        /// <param name="RunsInSystemProcess">Indica se il servizio è in esecuzione in un processo di sistema.</param>
        /// <param name="StartType">Tipo di avvio del servizio.</param>
        /// <param name="ErrorControlMode">Modalità di controllo errori.</param>
        /// <param name="LoadOrderGroup">Gruppo di caricamento.</param>
        /// <param name="TagID">Tag identificativo.</param>
        /// <param name="Dependencies">Dipendenze del servizio.</param>
        /// <param name="StartUserAccountName">Nome dell'account sotto cui il servizio deve essere eseguito.</param>
        /// <param name="IsAutoStartDelayed">Indica se l'avvio automatico del servizio deve essere ritardato.</param>
        /// <param name="Description">Descrizione.</param>
        /// <param name="FailureActions">Azioni da eseguire in caso di crash del servizio.</param>
        /// <param name="ExecuteFailureActionsOnNormalFailure">Indica se eseguire le azioni indicate nel campo <see cref="FailureActions"/> anche quando il servizio non va in crash.</param>
        /// <param name="PreferredNode">Nodo preferito.</param>
        /// <param name="PreshutdownTimeout">Tempo per eseguire le azioni prespegnimento.</param>
        /// <param name="RequiredPrivileges">Privilegi richiesti per il servizio.</param>
        /// <param name="Triggers">Trigger del servizio.</param>
        /// <param name="LaunchProtectedType">Tipo di protezione del servizio.</param>
        /// <param name="SIDType">Tipo di SID del servizio.</param>
        public Service(IntPtr Handle, string Name, string DisplayName, uint PID, string State, string Type, List<string> AcceptedControls, bool RunsInSystemProcess, string StartType, string ErrorControlMode, string LoadOrderGroup, uint TagID, List<string> Dependencies, string StartUserAccountName, bool? IsAutoStartDelayed, string Description, ServiceFailureActions FailureActions, bool? ExecuteFailureActionsOnNormalFailure, ushort? PreferredNode, uint? PreshutdownTimeout, List<string> RequiredPrivileges, List<ServiceTrigger> Triggers, string LaunchProtectedType, string SIDType)
        {
            this.Handle = Handle;
            this.Name = Name;
            this.DisplayName = DisplayName;
            this.PID = PID;
            StateValue = State;
            this.Type = Type;
            this.AcceptedControls = AcceptedControls;
            this.RunsInSystemProcess = RunsInSystemProcess ? Properties.Resources.YesText : "No";
            this.StartType = StartType;
            this.ErrorControlMode = ErrorControlMode;
            this.LoadOrderGroup = !string.IsNullOrWhiteSpace(LoadOrderGroup) ? LoadOrderGroup : Properties.Resources.NoneText;
            this.TagID = TagID != 0 ? TagID.ToString("D0", CultureInfo.InvariantCulture) : Properties.Resources.NotAssignedText;
            this.Dependencies = Dependencies;
            ServiceStartUserAccountName = StartUserAccountName;
            if (IsAutoStartDelayed.HasValue)
            {
                this.IsAutoStartDelayed = IsAutoStartDelayed.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                this.IsAutoStartDelayed = Properties.Resources.UnavailableText;
            }
            this.Description = Description;
            this.FailureActions = FailureActions;
            if (ExecuteFailureActionsOnNormalFailure.HasValue)
            {
                this.ExecuteFailureActionsOnNormalFailure = ExecuteFailureActionsOnNormalFailure.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                this.ExecuteFailureActionsOnNormalFailure = Properties.Resources.UnavailableText;
            }
            if (PreferredNode.HasValue)
            {
                this.PreferredNode = PreferredNode.Value.ToString("D0", CultureInfo.InvariantCulture);
            }
            else
            {
                this.PreferredNode = Properties.Resources.UnavailableText;
            }
            if (PreshutdownTimeout.HasValue)
            {
                this.PreshutdownTimeout = PreshutdownTimeout.Value.ToString("D0", CultureInfo.InvariantCulture);
            }
            else
            {
                this.PreshutdownTimeout = Properties.Resources.UnavailableText;
            }
            this.RequiredPrivileges = RequiredPrivileges;
            this.Triggers = Triggers;
            LaunchProtectionType = LaunchProtectedType;
            this.SIDType = SIDType;
        }

        /// <summary>
        /// Aggiorna lo stato del servizio.
        /// </summary>
        /// <param name="NewState">Nuovo stato.</param>
        public void UpdateServiceState(string NewState)
        {
            State = NewState;
        }

        /// <summary>
        /// Aggiorna il PID del processo in cui il servizio è in esecuzione.
        /// </summary>
        /// <param name="PID"></param>
        public void UpdateServicePID(uint PID)
        {
            this.PID = PID;
        }

        /// <summary>
        /// Apre un nuovo handle al servizio.
        /// </summary>
        /// <param name="SCMHandle">Handle nativo a Gestione Controllo Servizi.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool OpenNewHandle(IntPtr SCMHandle)
        {
            IntPtr NewHandle = NativeHelpers.OpenService(SCMHandle, Name);
            if (NewHandle != IntPtr.Zero)
            {
                Handle = NewHandle;
                return true;
            }
            else
            {
                Handle = IntPtr.Zero;
                return false;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NativeMethods.Win32ServiceFunctions.CloseServiceHandle(Handle);
                disposedValue = true;
            }
        }

         ~Service()
         {
             Dispose(disposing: false);
         }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}