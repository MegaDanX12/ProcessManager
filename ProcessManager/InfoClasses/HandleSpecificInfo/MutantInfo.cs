using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using System.ComponentModel;
using System.Globalization;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su un mutante.
    /// </summary>
    public class MutantInfo : INotifyPropertyChanged
    {

        /// <summary>
        /// Handle al processo a cui appartiene il mutante.
        /// </summary>
        private readonly SafeProcessHandle ProcessHandle;

        /// <summary>
        /// Istanza di <see cref="HandleInfo"/> relativa al mutante.
        /// </summary>
        private readonly HandleInfo AssociatedInfoInstance;

        public event PropertyChangedEventHandler PropertyChanged;

        private string CurrentCountValue;

        /// <summary>
        /// Conteggio attuale del mutante.
        /// </summary>
        public string CurrentCount
        {
            get
            {
                return CurrentCountValue;
            }
            set
            {
                if (CurrentCountValue != value)
                {
                    CurrentCountValue = value;
                    NotifyPropertyChanged(nameof(CurrentCount));
                }
            }
        }

        /// <summary>
        /// Indica se il mutante è segnalato dal thread chiamante.
        /// </summary>
        public string OwnedByCaller { get; }

        private string AbandonedStateValue;

        /// <summary>
        /// Indica se il thread proprietario del mutante è terminato senza rilasciarlo.
        /// </summary>
        public string AbandonedState
        {
            get
            {
                return AbandonedStateValue;
            }
            set
            {
                if (AbandonedStateValue != value)
                {
                    AbandonedStateValue = value;
                    NotifyPropertyChanged(nameof(AbandonedState));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="MutantInfo"/>.
        /// </summary>
        /// <param name="CurrentCount">Conteggio attuale.</param>
        /// <param name="OwnedByCaller">Indica se il thread chiamante è proprietario del mutante.</param>
        /// <param name="AbandonedState">Indica se il mutante non è stato rilasciato prima del termine del thread proprietario.</param>
        public MutantInfo(int? CurrentCount, bool? OwnedByCaller, bool? AbandonedState, SafeProcessHandle Handle, HandleInfo Info)
        {
            ProcessHandle = Handle;
            AssociatedInfoInstance = Info;
            if (CurrentCount.HasValue)
            {
                CurrentCountValue = CurrentCount.Value.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                CurrentCountValue = Properties.Resources.UnavailableText;
            }
            if (OwnedByCaller.HasValue)
            {
                this.OwnedByCaller = OwnedByCaller.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                this.OwnedByCaller = Properties.Resources.UnavailableText;
            }
            if (AbandonedState.HasValue)
            {
                AbandonedStateValue = AbandonedState.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                AbandonedStateValue = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            MutantInfo NewInfo = NativeHelpers.GetMutantSpecificInfo(ProcessHandle, AssociatedInfoInstance, true);
            CurrentCount = NewInfo.CurrentCount;
            AbandonedState = NewInfo.AbandonedState;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}