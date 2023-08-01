using ProcessManager.Commands.ProcessWindowsInfoWindowCommands;
using ProcessManager.Commands.WindowPropertiesWindowCommands;
using ProcessManager.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using static ProcessManager.NativeHelpers;

namespace ProcessManager.ViewModels
{
    public class ProcessWindowsInfoVM : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// Evento che notifica dell'aggiornamento di una proprietà.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Informazioni sulle finestre.
        /// </summary>
        private ObservableCollection<WindowInfo> WindowsInfoList;
        private bool disposedValue;

        /// <summary>
        /// Informazioni sulle finestre.
        /// </summary>
        public ObservableCollection<WindowInfo> WindowsInfo
        {
            get => WindowsInfoList;
            private set
            {
                if (WindowsInfoList != value)
                {
                    WindowsInfoList = value;
                    NotifyPropertyChanged(nameof(WindowsInfo));
                }
            }
        }

        /// <summary>
        /// Istanza di <see cref="WindowInfo"/> di cui vanno visualizzate le proprietà.
        /// </summary>
        public WindowInfo CurrentInfo { get; private set; }

        /// <summary>
        /// Processo associato.
        /// </summary>
        private readonly ProcessInfo AssociatedProcess;

        /// <summary>
        /// Comando "Porta in primo piano".
        /// </summary>
        public ICommand BringToFrontCommand { get; }

        /// <summary>
        /// Comando di ripristino di una finestra.
        /// </summary>
        public ICommand RestoreCommand { get; }

        /// <summary>
        /// Comando di riduzione a icona di una finestra.
        /// </summary>
        public ICommand MinimizeCommand { get; }

        /// <summary>
        /// Comando di ingrandimento di una finestra.
        /// </summary>
        public ICommand MaximizeCommand { get; }

        /// <summary>
        /// Comando di chiusura di una finestra.
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Comando di cambio stato di visibilità di una finestra.
        /// </summary>
        public ICommand VisibleCommand { get; }

        /// <summary>
        /// Comando di cambio stato di attivazione di una finestra.
        /// </summary>
        public ICommand EnabledCommand { get; }

        /// <summary>
        /// Comando di cambio stato "Sempre in primo piano" di una finestra.
        /// </summary>
        public ICommand AlwaysOnTopCommand { get; }

        /// <summary>
        /// Comando di visualizzazione delle proprietà di una finestra.
        /// </summary>
        public ICommand WindowPropertiesCommand { get; }

        /// <summary>
        /// Comando di aggiornamento proprietà di una finestra.
        /// </summary>
        public ICommand WindowPropertiesRefreshCommand { get; }

        /// <summary>
        /// Comando di aggiornamento dati per tutte le finestre.
        /// </summary>
        public ICommand WindowsDataRefreshCommand { get; }

        /// <summary>
        /// Informazioni sul thread associato alle finestre.
        /// </summary>
        private readonly ThreadInfo ThreadInfo;

        /// <summary>
        /// Handle nativo all'hook eventi.
        /// </summary>
        private IntPtr WindowEventHookHandle;

        /// <summary>
        /// <see cref="Dispatcher"/> collegato alla finestra che visualizza i dati presenti in questo viewmodel.
        /// </summary>
        public Dispatcher WindowDispatcher { get; set; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="ProcessWindowsInfoVM"/>.
        /// </summary>
        /// <param name="ThreadInfo">Istanza di <see cref="Models.ThreadInfo"/> associata alle finestre.</param>
        public ProcessWindowsInfoVM(ProcessInfo ProcessInfo, ThreadInfo ThreadInfo = null)
        {
            if (ProcessInfo is not null)
            {
                WindowsInfoList = new(ProcessInfo.GetWindowsInfo());
            }
            else
            {
                WindowsInfoList = ThreadInfo is not null ? (new(ThreadInfo.GetWindowsInfo())) : (new(GetWindowsInfo()));
            }
            this.ThreadInfo = ThreadInfo;
            AssociatedProcess = ProcessInfo;
            BringToFrontCommand = new BringToFrontCommand(this);
            RestoreCommand = new RestoreWindowCommand(this);
            MinimizeCommand = new MinimizeCommand(this);
            MaximizeCommand = new MaximizeCommand(this);
            CloseCommand = new CloseCommand(this);
            VisibleCommand = new VisibleCommand(this);
            EnabledCommand = new EnabledCommand(this);
            AlwaysOnTopCommand = new AlwaysOnTopCommand(this);
            WindowPropertiesCommand = new WindowPropertiesCommand(this);
            WindowPropertiesRefreshCommand = new RefreshWindowDataCommand(this);
        }

        /// <summary>
        /// Crea un hook relativo agli eventi di apertura e chiusura delle finestre.
        /// </summary>
        /// <returns>Handle nativo all'istanza dell'hook creata, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public void CreateHook()
        {
            if (AssociatedProcess is not null)
            {
                IntPtr HookFunctionInstanceHandle = HookObjectCreationDestructionEvents(AssociatedProcess.PID);
                if (HookFunctionInstanceHandle != IntPtr.Zero)
                {
                    WindowCreatedEvent += AddNewWindow;
                    WindowDestroyedEvent += RemoveWindow;
                    WindowEventHookHandle = HookFunctionInstanceHandle;
                }
                else
                {
                    WindowEventHookHandle = IntPtr.Zero;
                }
            }
            else
            {
                if (ThreadInfo is not null)
                {
                    IntPtr HookFunctionInstanceHandle = HookObjectCreationDestructionEvents(TID: ThreadInfo.TID);
                    if (HookFunctionInstanceHandle != IntPtr.Zero)
                    {
                        WindowCreatedEvent += AddNewWindow;
                        WindowDestroyedEvent += RemoveWindow;
                        WindowEventHookHandle = HookFunctionInstanceHandle;
                    }
                    else
                    {
                        WindowEventHookHandle = IntPtr.Zero;
                    }
                }
                else
                {
                    IntPtr HookFunctionInstanceHandle = HookObjectCreationDestructionEvents();
                    if (HookFunctionInstanceHandle != IntPtr.Zero)
                    {
                        WindowCreatedEvent += AddNewWindow;
                        WindowDestroyedEvent += RemoveWindow;
                        WindowEventHookHandle = HookFunctionInstanceHandle;
                    }
                    else
                    {
                        WindowEventHookHandle = IntPtr.Zero;
                    }
                }
            }
        }

        /// <summary>
        /// Rimuove l'hook creato.
        /// </summary>
        private void RemoveHook()
        {
            if (WindowEventHookHandle != IntPtr.Zero)
            {
                UnhookEvent(WindowEventHookHandle);
                WindowCreatedEvent -= AddNewWindow;
                WindowDestroyedEvent -= RemoveWindow;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        #region Window Operation Methods
        /// <summary>
        /// Porta una finestra in primo piano.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool BringWindowToFront(WindowInfo Info)
        {
            Contract.Requires(Info != null);
            return Info is not null && Info.BringToFront();
        }

        /// <summary>
        /// Rispristina lo stato di visibilità di una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool RestoreWindow(WindowInfo Info)
        {
            return Info is not null && Info.Restore();
        }

        /// <summary>
        /// Riduce a icona una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool MinimizeWindow(WindowInfo Info)
        {
            return Info is not null && Info.Minimize();
        }

        /// <summary>
        /// Ingrandisce una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool MaximizeWindow(WindowInfo Info)
        {
            return Info is not null && Info.Maximize();
        }

        /// <summary>
        /// Nasconde una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool CloseWindow(WindowInfo Info)
        {
            return Info is not null && Info.Close();
        }

        /// <summary>
        /// Rende visibile o nasconde una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool ChangeWindowVisibility(WindowInfo Info)
        {
            return Info is not null && Info.ChangeVisibility();
        }

        /// <summary>
        /// Abilita o disabilita una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool ChangeWindowEnableStatus(WindowInfo Info)
        {
            return Info is not null && Info.ChangeEnableStatus();
        }

        /// <summary>
        /// Cambia lo stato topmost di una finestra.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Contrassegnare i membri come static", Justification = "<In sospeso>")]
        public bool ChangeWindowTopMostStatus(WindowInfo Info)
        {
            return Info is not null && Info.ChangeTopMostStatus();
        }
        #endregion
        /// <summary>
        /// Imposta la finestra di cui vanno visualizzate le proprietà.
        /// </summary>
        /// <param name="Info">Istanza di <see cref="WindowInfo"/> associata alla finestra.</param>
        public void SetCurrentWindowInfo(WindowInfo Info)
        {
            CurrentInfo = Info;
        }

        /// <summary>
        /// Aggiorna i dati sulle finestre.
        /// </summary>
        public void UpdateWindowsData()
        {
            WindowsInfo = ThreadInfo == null
                ? new ObservableCollection<WindowInfo>(AssociatedProcess.GetWindowsInfo())
                : new ObservableCollection<WindowInfo>(ThreadInfo.GetWindowsInfo());
        }

        /// <summary>
        /// Aggiorna i dati di una finestra.
        /// </summary>
        public void UpdateSingleWindowData()
        {
            if (AssociatedProcess is not null)
            {
                CurrentInfo.UpdateData(AssociatedProcess.PID);
            }
            else if (ThreadInfo is not null)
            {
                CurrentInfo.UpdateData(0, ThreadInfo);
            }
            else
            {
                CurrentInfo.UpdateData(0);
            }
        }

        /// <summary>
        /// Aggiunge una nuova finestra all'elenco.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        private void AddNewWindow(IntPtr WindowHandle)
        {
            WindowInfo Info;
            if (AssociatedProcess is not null)
            {
                Info = GetWindowInfo(WindowHandle, null, AssociatedProcess.PID);
                WindowDispatcher.Invoke(() => WindowsInfo.Add(Info));
            }
            else
            {
                Info = GetWindowInfo(WindowHandle, null);
                WindowDispatcher.Invoke(() => WindowsInfo.Add(Info));
            }
        }

        /// <summary>
        /// Rimuove una finestra dall'elenco.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo che si riferisce alla finestra da rimuovere.</param>
        private void RemoveWindow(IntPtr WindowHandle)
        {
            WindowInfo Info = WindowsInfo.FirstOrDefault(windowinfo => windowinfo.IsHandleAssociatedWithWindow(WindowHandle));
            if (Info is not null)
            {
                _ = WindowDispatcher.Invoke(() => _ = WindowsInfo.Remove(Info));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                RemoveHook();
                disposedValue = true;
            }
        }

        ~ProcessWindowsInfoVM()
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