using ProcessManager.Commands.WatchdogSettingsWindowCommands;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class WatchdogSettingVM
    {
        /// <summary>
        /// Dimensione della memoria di sistema in MB.
        /// </summary>
        public static uint? SystemMemorySizeMB { get; } = NativeHelpers.GetSystemMemorySize();

        /// <summary>
        /// Numero di core del processore.
        /// </summary>
        public static uint? ProcessorCoresCount { get; } = NativeHelpers.GetNumberOfProcessorCores();

        /// <summary>
        /// Aggiunge una regola del watchdog.
        /// </summary>
        public ICommand AddRuleCommand { get; }

        /// <summary>
        /// Rimuove una regola del watchdog.
        /// </summary>
        public ICommand RemoveRuleCommand { get; }

        /// <summary>
        /// Aggiungi il nome di un processo alla lista di processi da terminare in caso di uso eccessivo della memoria.
        /// </summary>
        public ICommand AddProcessNameCommand { get; }

        /// <summary>
        /// Aggiungi il nome di un processo alla lista di processi da terminare in caso di uso eccessivo della memoria.
        /// </summary>
        public ICommand RemoveProcessNameCommand { get; }

        /// <summary>
        /// Aggiunge un'impostazione predefinita della CPU per un processo.
        /// </summary>
        public ICommand AddProcessDefaultCPUSetting { get; }

        /// <summary>
        /// Rimuove un'impostazione predefinità della CPU per un processo.
        /// </summary>
        public ICommand RemoveProcessDefaultCPUSetting { get; }

        /// <summary>
        /// Aggiunge una regola di utilizzo energetico per un processo.
        /// </summary>
        public ICommand AddProcessEnergyUsageRuleCommand { get; }

        /// <summary>
        /// Rimuove una regola di utilizzo energetico per un processo.
        /// </summary>
        public ICommand RemoveProcessEnergyUsageRuleCommand { get; }

        /// <summary>
        /// Aggiunge un limite alle istanze di un processo.
        /// </summary>
        public ICommand AddProcessInstanceLimitCommand { get; }

        /// <summary>
        /// Rimuove il limite alle istanze di un processo.
        /// </summary>
        public ICommand RemoveProcessInstanceLimitCommand { get; }

        /// <summary>
        /// Aggiunge un processo la cui esecuzione non è permessa.
        /// </summary>
        public ICommand AddDisallowedProcessCommand { get; }

        /// <summary>
        /// Rimuove un processo la cui esecuzione non è permessa.
        /// </summary>
        public ICommand RemoveDisallowedProcessCommand { get; }

        /// <summary>
        /// Abilita la notifica relativa alla terminazione di un processo non permesso.
        /// </summary>
        public ICommand EnableNotificationForDisallowedProcessTermination { get; }

        /// <summary>
        /// Disabilita la notifica relativa alla terminazione di un processo non permesso.
        /// </summary>
        public ICommand DisableNotificationForDisallowedProcessTermination { get; }

        /// <summary>
        /// Abilita la notifica relativa all'avvio dei processi permanenti.
        /// </summary>
        public ICommand EnableNotificationForPermanentProcess { get; }

        /// <summary>
        /// Disabilita la notifica relativa all'avvio dei processi permanenti.
        /// </summary>
        public ICommand DisableNotificationForPermanentProcess { get; }


        public WatchdogSettingVM()
        {
            AddRuleCommand = new AddRuleCommand();
            RemoveRuleCommand = new RemoveRuleCommand();
            AddProcessNameCommand = new AddProcessNameCommand();
            RemoveProcessNameCommand = new RemoveProcessNameCommand();
            AddProcessDefaultCPUSetting = new AddCPUDefaultSettingsCommand();
            RemoveProcessDefaultCPUSetting = new RemoveCPUDefaultSettingsCommand();
            AddProcessEnergyUsageRuleCommand = new AddProcessEnergyUsageRuleCommand();
            RemoveProcessEnergyUsageRuleCommand = new RemoveProcessEnergyUsageRuleCommand();
            AddProcessInstanceLimitCommand = new AddProcessInstanceLimitCommand();
            RemoveProcessInstanceLimitCommand = new RemoveProcessInstanceLimitCommand();
            AddDisallowedProcessCommand = new AddDisallowedProcessCommand();
            RemoveDisallowedProcessCommand = new RemoveDisallowedProcessCommand();
            EnableNotificationForDisallowedProcessTermination = new EnableNotificationForDisallowedProcessTerminationCommand();
            DisableNotificationForDisallowedProcessTermination = new DisableNotificationForDisallowedProcessCommand();
            EnableNotificationForPermanentProcess = new EnableNotificationForPermanentProcessesCommand();
            DisableNotificationForPermanentProcess = new DisableNotificationForPermanentProcessesCommand();
        }
    }
}