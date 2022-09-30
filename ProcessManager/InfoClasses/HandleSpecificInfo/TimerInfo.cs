using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using System;
using System.ComponentModel;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    public class TimerInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Handle al processo a cui appartiene il timer.
        /// </summary>
        private readonly SafeProcessHandle ProcessHandle;

        /// <summary>
        /// Istanza di <see cref="HandleInfo"/> relativa al timer.
        /// </summary>
        private readonly HandleInfo AssociatedInfoInstance;

        public event PropertyChangedEventHandler PropertyChanged;

        private string RemainingTimeValue;

        /// <summary>
        /// Tempo rimanente alla segnalazione del timer.
        /// </summary>
        public string RemainingTime 
        {
            get
            {
                return RemainingTimeValue;
            }
            private set
            {
                if (RemainingTimeValue != value)
                {
                    RemainingTimeValue = value;
                    NotifyPropertyChanged(nameof(RemainingTime));
                }
            }
        }

        private string TimeSinceLastSignalValue;

        /// <summary>
        /// Tempo passato dall'ultima segnalazione.
        /// </summary>
        public string TimeSinceLastSignal
        {
            get
            {
                return TimeSinceLastSignalValue;
            }
            private set
            {
                if (TimeSinceLastSignalValue != value)
                {
                    TimeSinceLastSignalValue = value;
                    NotifyPropertyChanged(nameof(TimeSinceLastSignal));
                }
            }
        }

        private string TimerStateValue;

        /// <summary>
        /// Indica se il timer è segnalato o meno.
        /// </summary>
        public string TimerState
        {
            get
            {
                return TimerStateValue;
            }
            private set
            {
                if (TimerStateValue != value)
                {
                    TimerStateValue = value;
                    NotifyPropertyChanged(nameof(TimerState));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="TimerInfo"/>.
        /// </summary>
        /// <param name="RemainingTime">Tempo rimanente alla segnalazione del timer oppure tempo trascorso dall'ultima segnalazione.</param>
        /// <param name="TimerState">Indica se il timer è stato segnalato o meno.</param>
        public TimerInfo(long? RemainingTime, bool? TimerState, SafeProcessHandle Handle, HandleInfo Info)
        {
            ProcessHandle = Handle;
            AssociatedInfoInstance = Info;
            if (RemainingTime.HasValue)
            {
                if (RemainingTime < 0)
                {
                    RemainingTimeValue = new TimeSpan((long)RemainingTime).ToString("c");
                    TimeSinceLastSignalValue = TimeSpan.Zero.ToString("c");
                }
                else
                {
                    RemainingTimeValue = TimeSpan.Zero.ToString("c");
                    TimeSinceLastSignalValue = new TimeSpan((long)RemainingTime).ToString("c");
                }
            }
            else
            {
                RemainingTimeValue = Properties.Resources.UnavailableText;
                TimeSinceLastSignalValue = Properties.Resources.UnavailableText;
            }
            if (TimerState.HasValue)
            {
                TimerStateValue = TimerState.Value ? Properties.Resources.YesText : "No";
            }
            else
            {
                TimerStateValue = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            TimerInfo NewInfo = NativeHelpers.GetTimerSpecificInfo(ProcessHandle, AssociatedInfoInstance, true);
            RemainingTime = NewInfo.RemainingTime;
            TimeSinceLastSignal = NewInfo.TimeSinceLastSignal;
            TimerState = NewInfo.TimerState;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}