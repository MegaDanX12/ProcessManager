using ProcessManager.Commands.ServicesInfoWindowCommands;
using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class HostedServicesDataVM
    {
        /// <summary>
        /// Servizi.
        /// </summary>
        public ObservableCollection<Service> Services { get; }

        /// <summary>
        /// Istanza di <see cref="ProcessInfo"/> associata al processo che ospita i servizi.
        /// </summary>
        public ProcessInfo Info { get; }

        /// <summary>
        /// Comando per mostrare lo stato di un servizio.
        /// </summary>
        public ICommand ShowServiceStatusInfoCommand { get; }

        /// <summary>
        /// Comando per mostrare la configurazione di un servizio.
        /// </summary>
        public ICommand ShowServiceConfigurationCommand { get; }

        /// <summary>
        /// Comando per avviare il monitoraggio dello stato di un servizio.
        /// </summary>
        public ICommand StartServiceMonitoringCommand { get; }

        /// <summary>
        /// Comando per interrompere il monitoraggio dello stato di un servizio.
        /// </summary>
        public ICommand StopServiceMonitoringCommand { get; }

        /// <summary>
        /// Istanza di <see cref="InfoClasses.ServicesInfo.Services"/> che gestisce le informazioni sui servizi.
        /// </summary>
        private readonly Services ServicesManager;

        /// <summary>
        /// Servizio attualmente selezionato.
        /// </summary>
        public Service CurrentService { get; set; }

        public HostedServicesDataVM(Service[] Services, ProcessInfo Info, Services ServicesManager)
        {
            if (Services == null)
            {
                this.Services = new(ServicesManager.GetServices());
            }
            else
            {
                this.Services = new(Services);
            }
            ShowServiceStatusInfoCommand = new ShowServiceStatusCommand(this);
            ShowServiceConfigurationCommand = new ShowServiceConfigurationCommand(this);
            StartServiceMonitoringCommand = new ListenForServiceEventsCommand(this);
            StopServiceMonitoringCommand = new StopServiceStatusMonitoringCommand(this);
            this.Info = Info;
            this.ServicesManager = ServicesManager;
        }

        /// <summary>
        /// Avvia il monitoraggio dello stato di un servizio.
        /// </summary>
        /// <param name="ServiceData">Istanza di <see cref="Service"/> che rappresenta il servizio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool StartServiceMonitoring(Service ServiceData, string[] Reasons)
        {
            return ServicesManager.StartServiceMonitoring(ServiceData.Name, Reasons);
        }

        /// <summary>
        /// Interrompe il monitoraggio dello stato di un servizio.
        /// </summary>
        /// <param name="ServiceData">Istanza di <see cref="Service"/> che rappresenta il servizio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool StopServiceMonitoring(Service ServiceData)
        {
            return ServicesManager.StopServiceMonitoring(ServiceData.Name);
        }
    }
}