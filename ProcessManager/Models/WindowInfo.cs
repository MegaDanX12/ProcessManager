using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.WindowsInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni su una finestra.
    /// </summary>
    public class WindowInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Evento che notifica dell'aggiornamento di una proprietà.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handle nativo alla finestra.
        /// </summary>
        private readonly IntPtr WindowHandle;

        /// <summary>
        /// Informazioni generali sulla finestra.
        /// </summary>
        private WindowGeneralInfo WindowGeneralInfo;

        /// <summary>
        /// Informazioni generali sulla finestra.
        /// </summary>
        public WindowGeneralInfo GeneralInfo
        {
            get => WindowGeneralInfo;
            private set
            {
                if (WindowGeneralInfo != value)
                {
                    WindowGeneralInfo = value;
                    NotifyPropertyChanged(nameof(GeneralInfo));
                }
            }
        }

        /// <summary>
        /// Informazioni sugli stili della finestra.
        /// </summary>
        private WindowStylesInfo WindowStyles;

        /// <summary>
        /// Informazioni sugli stili della finestra.
        /// </summary>
        public WindowStylesInfo Styles
        {
            get => WindowStyles;
            private set
            {
                if (WindowStyles != value)
                {
                    WindowStyles = value;
                    NotifyPropertyChanged(nameof(Styles));
                }
            }
        }

        /// <summary>
        /// Informazioni sulla classe della finestra.
        /// </summary>
        private WindowClassInfo WindowClassInfo;

        /// <summary>
        /// Informazioni sulla classe della finestra.
        /// </summary>
        public WindowClassInfo ClassInfo
        {
            get => WindowClassInfo;
            private set
            {
                if (WindowClassInfo != value)
                {
                    WindowClassInfo = value;
                    NotifyPropertyChanged(nameof(ClassInfo));
                }
            }
        }

        /// <summary>
        /// Proprietà della finestra.
        /// </summary>
        private ObservableCollection<WindowProperty> Properties;

        /// <summary>
        /// Proprietà della finestra.
        /// </summary>
        public ObservableCollection<WindowProperty> WindowProperties
        {
            get => Properties;
            private set
            {
                if (Properties != value)
                {
                    Properties = value;
                    NotifyPropertyChanged(nameof(WindowProperties));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="WindowInfo"/>.
        /// </summary>
        /// <param name="GeneralInfo">Informazioni generali sulla finestra.</param>
        /// <param name="Styles">Informazioni sugli stili della finestra.</param>
        /// <param name="ClassInfo">Informazioni sulla classe della finestra.</param>
        /// <param name="WindowProperties">Lista di proprietà della finestra.</param>
        public WindowInfo(IntPtr WindowHandle, WindowGeneralInfo GeneralInfo, WindowStylesInfo Styles, WindowClassInfo ClassInfo, List<WindowProperty> WindowProperties)
        {
            this.WindowHandle = WindowHandle;
            WindowGeneralInfo = GeneralInfo;
            WindowStyles = Styles;
            WindowClassInfo = ClassInfo;
            Properties = new ObservableCollection<WindowProperty>(WindowProperties);
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Determina se la finestra è visibile.
        /// </summary>
        /// <returns>true se la finestra è visibile, false altrimenti.</returns>
        public bool IsWindowVisible()
        {
            return NativeHelpers.IsWindowVisible(WindowHandle);
        }

        /// <summary>
        /// Determina se la finestra è attiva.
        /// </summary>
        /// <returns>true se la finestra è attiva, false altrimenti.</returns>
        public bool IsWindowEnabled()
        {
            return NativeHelpers.IsWindowEnabled(WindowHandle);
        }

        /// <summary>
        /// Determina se una finestra è impostata per essere sempre al di sopra di tutte le altre.
        /// </summary>
        /// <returns>true se la finestra è impostata per essere sempre al di sopra di tutte le altre, false altrimenti.</returns>
        public bool IsWindowAlwaysOnTop()
        {
            return Styles.WindowExtendedStyles.Contains("WS_EX_TOPMOST");
        }

        /// <summary>
        /// Recupera la percentuale di trasparenza della finestra.
        /// </summary>
        /// <returns>La percentuale di trasparenza.</returns>
        public int GetTransparencyPercentage()
        {
            return Styles.WindowExtendedStyles.Contains("WS_EX_LAYERED") ? NativeHelpers.GetWindowTransparencyPercentage(WindowHandle) : 100;
        }

        /// <summary>
        /// Imposta la percentuale di trasparenza della finestra.
        /// </summary>
        /// <param name="Value">Nuova percentuale di trasparenza.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool SetTransparencyPercentage(byte Value)
        {
            if (Styles.WindowExtendedStyles.Contains("WS_EX_LAYERED"))
            {
                if (NativeHelpers.SetWindowTransparencyPercentage(WindowHandle, Value))
                {
                    LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Impostata la trasparenza di una finestra, nuova percentuale: " + Value, EventAction.WindowOperation, null);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Porta la finestra in primo piano.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool BringToFront()
        {
            return NativeHelpers.BringWindowToFront(WindowHandle);
        }

        /// <summary>
        /// Ripristina lo stato di visibilità della finestra.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool Restore()
        {
            return NativeHelpers.RestoreWindow(WindowHandle);
        }

        /// <summary>
        /// Riduce a icona la finestra.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool Minimize()
        {
            return NativeHelpers.MinimizeWindow(WindowHandle);
        }

        /// <summary>
        /// Ingrandisce la finestra.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool Maximize()
        {
            return NativeHelpers.MaximizeWindow(WindowHandle);
        }

        /// <summary>
        /// Nasconde la finestra.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool Close()
        {
            return NativeHelpers.CloseWindow(WindowHandle);
        }

        /// <summary>
        /// Cambia la visibilità della finestra.
        /// </summary>
        /// <returns></returns>
        public bool ChangeVisibility()
        {
            return NativeHelpers.ChangeWindowVisibility(WindowHandle);
        }

        /// <summary>
        /// Abilita o disabilita la finestra.
        /// </summary>
        /// <returns></returns>
        public bool ChangeEnableStatus()
        {
            return NativeHelpers.ChangeWindowEnabledStatus(WindowHandle);
        }

        /// <summary>
        /// Cambia lo stato topmost della finestra.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ChangeTopMostStatus()
        {
            return IsWindowAlwaysOnTop()
                ? NativeHelpers.ChangeWindowTopMostStatus(WindowHandle, true)
                : NativeHelpers.ChangeWindowTopMostStatus(WindowHandle, false);
        }

        /// <summary>
        /// Aggiorna i dati sulla finestra.
        /// </summary>
        public void UpdateData(uint PID, ThreadInfo Info = null)
        {
            if (PID is not 0)
            {
                using SafeProcessHandle Handle = NativeHelpers.GetProcessHandle(PID);
                WindowInfo WindowInfo = NativeHelpers.GetWindowInfo(WindowHandle, Handle, PID);
                GeneralInfo = WindowInfo.GeneralInfo;
                Styles = WindowInfo.Styles;
                ClassInfo = WindowInfo.ClassInfo;
                WindowProperties = WindowInfo.WindowProperties;
            }
            else if (Info is not null)
            {
                uint ProcessID = Info.GetThreadAssociatedProcessID();
                using SafeProcessHandle Handle = NativeHelpers.GetProcessHandle(ProcessID);
                WindowInfo WindowInfo = NativeHelpers.GetWindowInfo(WindowHandle, Handle);
                GeneralInfo = WindowInfo.GeneralInfo;
                Styles = WindowInfo.Styles;
                ClassInfo = WindowInfo.ClassInfo;
                WindowProperties = WindowInfo.WindowProperties;
            }
            else
            {
                WindowInfo WindowInfo = NativeHelpers.GetWindowInfo(WindowHandle, null);
                GeneralInfo = WindowInfo.GeneralInfo;
                Styles = WindowInfo.Styles;
                ClassInfo = WindowInfo.ClassInfo;
                WindowProperties = WindowInfo.WindowProperties;
            }
        }

        /// <summary>
        /// Determina se l'handle specificato si riferisce alla finestra descritta da questa instanza.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'handle si riferisce alla finestra, false altrimenti.</returns>
        public bool IsHandleAssociatedWithWindow(IntPtr WindowHandle)
        {
            return this.WindowHandle == WindowHandle;
        }
    }
}