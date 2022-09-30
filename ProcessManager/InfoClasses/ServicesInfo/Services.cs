using System;
using System.Linq;
using static ProcessManager.NativeMethods;
using static ProcessManager.NativeHelpers;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using Microsoft.Win32;

namespace ProcessManager.InfoClasses.ServicesInfo
{
    /// <summary>
    /// Contiene informazioni sui servizi presenti nel sistema.
    /// </summary>
    public class Services : IDisposable
    {
        /// <summary>
        /// Handle a Gestione Controllo Servizi.
        /// </summary>
        private IntPtr SCMHandle;

        /// <summary>
        /// Informazioni sui servizi.
        /// </summary>
        private readonly Dictionary<Service, bool> ServicesData;

        /// <summary>
        /// Delegato per l'elaborazione di un evento relativo al cambio dello stato di un servizio.
        /// </summary>
        private static ServiceStatusChangeCallback ServiceStatusChangeCallback;

        /// <summary>
        /// Delegato per l'elaborazione di un evento relativo alla creazione o alla eliminazione di un servizio.
        /// </summary>
        private static ServiceStatusChangeCallback ServiceCreationDeletionCallback;

        private bool disposedValue;

        /// <summary>
        /// Oggetto di blocco.
        /// </summary>
        private readonly object Locker = new();

        /// <summary>
        /// Indica se il monitoraggio dei servizi è disponibile.
        /// </summary>
        public bool FeatureAvailable { get; private set; }

        /// <summary>
        /// Indica se la creazione di un servizio deve generare una notifica.
        /// </summary>
        public bool ServiceCreationNotificationsEnabled { get; private set; }

        /// <summary>
        /// Indica se l'eliminazione di un servizio deve generare una notifica.
        /// </summary>
        public bool ServiceDeletionNotificationsEnabled { get; private set; }

        /// <summary>
        /// Servizi monitorati.
        /// </summary>
        public Dictionary<string, string[]> MonitoredServices { get; } = new();

        /// <summary>
        /// Gruppi di servizi di sistema e relativi nomi dei servizi.
        /// </summary>
        private readonly Dictionary<string, string[]> ServiceGroups;

        /// <summary>
        /// Handle nativo a un evento che segnala di interrompere il monitoraggio dello stato dei servizi.
        /// </summary>
        private readonly IntPtr MonitoringStopEventHandle;

        /// <summary>
        /// Indica se un nuovo servizio è stato aggiunto.
        /// </summary>
        private bool NewServiceAdded = false;

        /// <summary>
        /// Evento che segnala l'eliminazione di un servizio.
        /// </summary>
        public static event EventHandler<Service> ServiceDeleted;

        /// <summary>
        /// Evento che segnala la creazione di un servizio.
        /// </summary>
        public static event EventHandler<Service> ServiceAdded;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="Services"/>.
        /// </summary>
        public Services()
        {
            if (Settings.ServiceMonitoringEnabled)
            {
                ServiceGroups = GetServiceGroups();
                if (ServiceGroups != null)
                {
                    Logger.WriteEntry(BuildLogEntryForInformation("Recuperati i nomi dei gruppi di servizi e relativi nomi dei servizi contenuti in essi", EventAction.ServicesGeneral));
                    SCMHandle = OpenServiceControlManager();
                    if (SCMHandle != IntPtr.Zero)
                    {
                        List<Service> ServicesData = EnumerateServices(SCMHandle);
                        if (ServicesData == null || ServicesData.Count == 0)
                        {
                            CloseServiceHandle(SCMHandle);
                            FeatureAvailable = false;
                        }
                        else
                        {
                            this.ServicesData = new();
                            foreach (Service service in ServicesData)
                            {
                                this.ServicesData.Add(service, false);
                            }
                            Logger.WriteEntry(BuildLogEntryForInformation("Recuperate informazioni sui servizi presenti nel sistema", EventAction.ServicesEnumeration));
                            ServiceStatusChangeCallback = new(ProcessServiceStatusChange);
                            ServiceCreationDeletionCallback = new(ProcessServiceCreationDeletion);
                            Thread SCMEventThread = new(new ThreadStart(new Action(RegisterAndWaitSCM)))
                            {
                                IsBackground = true,
                                Name = "ServiceControlManagerEventThread"
                            };
                            Thread ServicesMonitoringThread = new(new ThreadStart(new Action(RegisterAndWaitService)))
                            {
                                IsBackground = true,
                                Name = "ServiceEventThread"
                            };
                            MonitoringStopEventHandle = CreateEvent();
                            if (MonitoringStopEventHandle != IntPtr.Zero)
                            {
                                SCMEventThread.Start();
                                ServicesMonitoringThread.Start();
                                FeatureAvailable = true;
                            }
                            else
                            {
                                FeatureAvailable = false;
                            }
                        }
                    }
                    else
                    {
                        FeatureAvailable = false;
                    }
                }
                else
                {
                    FeatureAvailable = false;
                }
            }
            else
            {
                FeatureAvailable = false;
            }
        }

        /// <summary>
        /// Interrompe il monitoraggio dei servizi.
        /// </summary>
        public void ShutdownMonitor()
        {
            FeatureAvailable = false;
            StopAllServicesMonitoring();
            Dispose();
        }
        #region Notifications Management Methods
        /// <summary>
        /// Attiva le notifiche relative alla creazione dei servizi.
        /// </summary>
        public void ActivateServiceCreationNotifications()
        {
            ServiceCreationNotificationsEnabled = true;
        }

        /// <summary>
        /// Disattiva le notifiche relative alla creazione dei servizi.
        /// </summary>
        public void DeactivateServiceCreationNotifications()
        {
            ServiceCreationNotificationsEnabled = false;
        }

        /// <summary>
        /// Attiva le notifiche relative alla creazione dei servizi.
        /// </summary>
        public void ActivateServiceDeletionNotifications()
        {
            ServiceDeletionNotificationsEnabled = true;
        }

        /// <summary>
        /// Disattiva le notifiche relative alla creazione dei servizi.
        /// </summary>
        public void DeactivateServiceDeletionNotifications()
        {
            ServiceDeletionNotificationsEnabled = false;
        }
        #endregion
        #region Services Info Getter Methods
        /// <summary>
        /// Recupera i gruppi di servizi definiti nel registro di sistema.
        /// </summary>
        /// <returns>Un dizionario che contiene i nomi dei servizi divisi per gruppi.</returns>
        private static Dictionary<string, string[]> GetServiceGroups()
        {
            Dictionary<string, string[]> ServiceGroups = new();
            RegistryKey GroupsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Svchost");
            if (GroupsKey != null)
            {
                string[] ValueNames = GroupsKey.GetValueNames();
                foreach (string name in ValueNames)
                {
                    string[] ServiceNames = (string[])GroupsKey.GetValue(name);
                    ServiceGroups.Add(name, ServiceNames);
                }
                return ServiceGroups;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Recupera i dati sui servizi ospitati da un processo.
        /// </summary>
        /// <param name="PID">ID del processo.</param>
        /// <returns>Una lista di istanze di <see cref="Service"/> che rappresentano i servizi ospitati.</returns>
        public List<Service> GetProcessHostedServices(uint PID, string ServiceGroup = null)
        {
            List<Service> Services = new();
            if (string.IsNullOrWhiteSpace(ServiceGroup))
            {
                lock (Locker)
                {
                    foreach (Service service in ServicesData.Keys)
                    {
                        if (service.PID != 0 && service.PID == PID)
                        {
                            Services.Add(service);
                        }
                    }
                }
            }
            else
            {
                lock (Locker)
                {
                    foreach (string name in ServiceGroups[ServiceGroup])
                    {
                        Service Service = ServicesData.FirstOrDefault(service => service.Key.Name == name).Key;
                        if (Service != null)
                        {
                            Services.Add(Service);
                        }
                    }
                }
            }
            return Services;
        }

        /// <summary>
        /// Recupera i dati sui servizi e li inserisce in una nuova lista.
        /// </summary>
        /// <returns>Una lista con i dati sui servizi.</returns>
        public List<Service> GetServices()
        {
            return new(ServicesData.Keys);
        }

        /// <summary>
        /// Recupera i dati su un servizio.
        /// </summary>
        /// <param name="ServiceName">Nome del servizio nel database.</param>
        /// <returns>Istanza di <see cref="Service"/> con le informazioni.</returns>
        public Service GetServiceData(string ServiceName)
        {
            return ServicesData.Keys.First(service => service.Name == ServiceName);
        }
        #endregion
        #region Service Monitoring Management Methods
        /// <summary>
        /// Registra un callback per la creazione e l'eliminazione di un servizio e resta in attesa di un evento.
        /// </summary>
        private void RegisterAndWaitSCM()
        {
            byte CallbackRegistrationAttempts = 0;
            while (!MainWindow.ProgramTerminating)
            {
                bool RegistrationSuccess = RegisterCallbackForServiceCreationAndDeletion(SCMHandle, ServiceCreationDeletionCallback, out bool ClientLagging);
                if (RegistrationSuccess)
                {
                    Logger.WriteEntry(BuildLogEntryForInformation("Registrato callback per la gestione di eventi relativi alla creazione o eliminazione di servizi", EventAction.ServicesMonitoring));
                }
                else
                {
                    while (CallbackRegistrationAttempts <= 3 && !RegistrationSuccess)
                    {
                        if (ClientLagging)
                        {
                            CloseServiceHandle(SCMHandle);
                            SCMHandle = OpenServiceControlManager();
                            if (SCMHandle != IntPtr.Zero)
                            {
                                RegistrationSuccess = RegisterCallbackForServiceCreationAndDeletion(SCMHandle, ServiceCreationDeletionCallback, out ClientLagging);
                            }
                            else
                            {
                                break;
                            }
                            CallbackRegistrationAttempts += 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (CallbackRegistrationAttempts == 3)
                    {
                        if (RegistrationSuccess)
                        {
                            Logger.WriteEntry(BuildLogEntryForInformation("Registrato callback per la gestione di eventi relativi alla creazione o eliminazione di servizi", EventAction.ServicesMonitoring));
                        }
                    }
                    else
                    {
                        if (RegistrationSuccess)
                        {
                            Logger.WriteEntry(BuildLogEntryForInformation("Registrato callback per la gestione di eventi relativi alla creazione o eliminazione di servizi", EventAction.ServicesMonitoring));
                        }
                    }
                }
                WaitForServiceEvent();
                CallbackRegistrationAttempts = 0;
            }
        }

        /// <summary>
        /// Inizia il monitoraggio dello stato di un servizio.
        /// </summary>
        /// <param name="ServiceName">Nome del servizio nel database.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenit.</returns>
        public bool StartServiceMonitoring(string ServiceName, string[] NotificationsReasons)
        {
            MonitoredServices.Add(ServiceName, NotificationsReasons);
            LogEntry Entry = BuildLogEntryForInformation("Monitoraggio di un servizio avviato, nome del servizio: " + ServiceName, EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
            return true;
        }

        /// <summary>
        /// Interrompre il monitoraggio dello stato di un servizio.
        /// </summary>
        /// <param name="ServiceName">Nome del servizio nel database.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenit.</returns>
        public bool StopServiceMonitoring(string ServiceName)
        {
            MonitoredServices.Remove(ServiceName);
            Service Service = ServicesData.FirstOrDefault(pair => pair.Key.Name == ServiceName).Key;
            LogEntry Entry;
            if (Service == null)
            {
                Entry = BuildLogEntryForWarning("Non è stato possibile terminare il monitoraggio di un servizio, nome del servizio: " + ServiceName + ", servizio non trovato", EventAction.ServicesMonitoring);
                Logger.WriteEntry(Entry);
                return false;
            }
            CloseServiceHandle(Service.Handle);
            Service.OpenNewHandle(SCMHandle);
            Entry = BuildLogEntryForInformation("Sottoscrizione agli eventi di un servizio annullata, nome del servizio: " + ServiceName, EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
            return true;
        }

        /// <summary>
        /// Registra un callback per un evento relativo a ogni servizio presente nel sistema e resta in attesa.
        /// </summary>
        private void RegisterAndWaitService()
        {
            while (!MainWindow.ProgramTerminating)
            {
                foreach (Service service in ServicesData.Keys)
                {
                    if (!ServicesData[service])
                    {
                        bool RegistrationSuccess = RegisterCallbackForServiceEvent(service.Handle, ServiceStatusChangeCallback, service.State, out bool MarkedForDeletion, service.Name);
                        if (RegistrationSuccess)
                        {
                            ServicesData[service] = true;
                            Logger.WriteEntry(BuildLogEntryForInformation("Registrato callback per la gestione di eventi relativi a un servizio, nome servizio: " + service.Name, EventAction.ServicesMonitoring));
                        }
                        else
                        {
                            if (MarkedForDeletion)
                            {
                                ServicesData.Remove(service);
                                service.Dispose();
                            }
                            break;
                        }
                    }
                }
                if (!WaitForServiceEvent(MonitoringStopEventHandle))
                {
                    if (!NewServiceAdded)
                    {
                        LogEntry Entry = BuildLogEntryForInformation("Monitoraggio dei servizi terminato", EventAction.ServicesMonitoring);
                        Logger.WriteEntry(Entry);
                        break;
                    }
                    else
                    {
                        NewServiceAdded = false;
                    }
                }
            }
        }

        /// <summary>
        /// Interrompe il monitoraggio di tutti i servizi.
        /// </summary>
        public void StopAllServicesMonitoring()
        {
            SetEventAsSignaled(MonitoringStopEventHandle);
        }
        #endregion
        #region Service Event Processing Methods
        /// <summary>
        /// Elabora le notifiche relative al cambio di stato di un servizio.
        /// </summary>
        /// <param name="NotificationStructure">Puntatore a una struttura che contiene le informazioni.</param>
        private void ProcessServiceStatusChange(IntPtr NotificationStructure)
        {
            string[] ServiceData = GetServiceNameAndState(NotificationStructure);           
            Service Service = ServicesData.FirstOrDefault(service => service.Key.Name == ServiceData[0]).Key;
            if (Service != null)
            {
                if (!IsServicePendingDeletion(NotificationStructure))
                {
                    UpdateList("EditState", Service, null, ServiceData[1], null);
                    LogEntry Entry = BuildLogEntryForInformation("Ricevuta notifica cambio di stato servizio, nome servizio: " + Service.Name + ", nuovo stato: " + Service.State, EventAction.ServicesMonitoring);
                    Logger.WriteEntry(Entry);
                    if (MonitoredServices[Service.Name].Contains(ServiceData[1]))
                    {
                        if (ServiceData[1] == Properties.Resources.ServiceRunningText)
                        {
                            object[] ServiceData2 = GetServicePIDAndState(Service.Handle);
                            UpdateList("EditPID", Service, null, null, (uint)ServiceData2[0]);
                            new ToastContentBuilder().AddText(Properties.Resources.ServiceStartedToastText, hintMaxLines: 1).AddText(Service.DisplayName).Show();
                        }
                        else if (ServiceData[1] == Properties.Resources.ServiceStoppedText)
                        {
                            UpdateList("EditPID", Service, null, null, 0);
                            new ToastContentBuilder().AddText(Properties.Resources.ServiceStoppedToastText, hintMaxLines: 1).AddText(Service.DisplayName).Show();
                        }
                        else if (ServiceData[1] == Properties.Resources.ServiceStatePausedText)
                        {
                            new ToastContentBuilder().AddText(Properties.Resources.ServicePausedToastText, hintMaxLines: 1).AddText(Service.DisplayName).Show();
                        }
                    }
                }
                else
                {
                    CloseServiceHandle(Service.Handle);
                }
            }
        }

        /// <summary>
        /// Elabora le notifiche relative alla creazione e alla eliminazione dei servizi.
        /// </summary>
        /// <param name="NotificationStructure">Puntatore a una struttura che contiene le informazioni.</param>
        private void ProcessServiceCreationDeletion(IntPtr NotificationStructure)
        {
            LogEntry Entry = BuildLogEntryForInformation("Ricevuta notifica da Gestione Controllo Servizi", EventAction.ServicesMonitoring);
            Logger.WriteEntry(Entry);
            (string EventType, string[] Names)[] Data = GetCreatedDeletedServiceNames(NotificationStructure);
            foreach (string name in Data[1].Names)
            {
                Service ServiceData = ServicesData.FirstOrDefault(service => service.Key.Name == name).Key;
                if (ServiceDeletionNotificationsEnabled)
                {
                    new ToastContentBuilder().AddText(Properties.Resources.ServiceRemovedToastText, hintMaxLines: 1).AddText(ServiceData.DisplayName).Show();
                }
                UpdateList("Remove", ServiceData, null, null, null);
                Entry = BuildLogEntryForInformation("Servizio rimosso, nome servizio: " + name, EventAction.ServicesMonitoring);
                Logger.WriteEntry(Entry);
            }
            foreach (string name in Data[0].Names)
            {
                IntPtr ServiceHandle = OpenService(SCMHandle, name);
                Service NewService = NativeHelpers.GetServiceData(ServiceHandle, name);
                UpdateList("Add", NewService, null, null, null);
                Entry = BuildLogEntryForInformation("Servizio creato, nome servizio: " + name, EventAction.ServicesMonitoring);
                Logger.WriteEntry(Entry);
                if (ServiceCreationNotificationsEnabled)
                {
                    new ToastContentBuilder().AddText(Properties.Resources.ServiceAddedToastText, hintMaxLines: 1).AddText(NewService.DisplayName).Show();
                }
            }
        }
        #endregion
        /// <summary>
        /// Aggiorna la lista dei servizi.
        /// </summary>
        /// <param name="Operation">Operazione da eseguire.</param>
        /// <param name="ServiceData">Dati sul servizio da aggiungere.</param>
        /// <param name="ServiceName">Nome del servizio da rimuovere.</param>
        /// <param name="NewStatus">Nuovo stato del servizio.</param>
        /// <param name="PID">Nuovo PID del processo che ospita il servizio.</param>
        private void UpdateList(string Operation, Service ServiceData, string ServiceName, string NewStatus, uint? PID)
        {
            lock (Locker)
            {
                switch (Operation)
                {
                    case "Add":
                        ServicesData.Add(ServiceData, false);
                        SetEventAsSignaled(MonitoringStopEventHandle);
                        OnServiceCreated(ServiceData);
                        break;
                    case "Remove":
                        ServicesData.Remove(ServiceData);
                        OnServiceDeleted(ServiceData);
                        break;
                    case "EditState":
                        if (ServiceData != null)
                        {
                            ServiceData.UpdateServiceState(NewStatus);
                        }
                        else
                        {
                            foreach (Service service in ServicesData.Keys)
                            {
                                if (service.Name == ServiceName)
                                {
                                    service.UpdateServiceState(NewStatus);
                                    break;
                                }
                            }
                        }
                        break;
                    case "EditPID":
                        ServiceData.UpdateServicePID(PID.Value);
                        break;
                }
            }
        }

        private void OnServiceDeleted(Service ServiceData)
        {
            ServiceDeleted?.Invoke(this, ServiceData);
        }

        private void OnServiceCreated(Service ServiceData)
        {
            ServiceAdded?.Invoke(this, ServiceData);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (FeatureAvailable)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        foreach (Service service in ServicesData.Keys)
                        {
                            service.Dispose();
                        }
                    }
                    CloseServiceHandle(SCMHandle);
                    CloseHandle(MonitoringStopEventHandle);
                    disposedValue = true;
                }
            }
        }

         ~Services()
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