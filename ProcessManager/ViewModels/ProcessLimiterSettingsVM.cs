using ProcessManager.Commands.ProcessLimiterSettingsWindowCommands;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class ProcessLimiterSettingsVM
    {
        /// <summary>
        /// Comando per aggiungere un limite.
        /// </summary>
        public ICommand AddLimitCommand { get; }

        /// <summary>
        /// Comando per rimuovere un limite.
        /// </summary>
        public ICommand RemoveLimitCommand { get; }

        /// <summary>
        /// Comando per aggiungere il percorso di un'applicazione a un limite.
        /// </summary>
        public ICommand AddPathCommand { get; }

        /// <summary>
        /// Comando per rimuovere il percorso di un'applicazione da un limite.
        /// </summary>
        public ICommand RemovePathCommand { get; }

        /// <summary>
        /// Istanza del viewmodel principale.
        /// </summary>
        public ProcessInfoVM MainViewModel { get; }


        public ProcessLimiterSettingsVM(ProcessInfoVM MainViewModel)
        {
            this.MainViewModel = MainViewModel;
            AddLimitCommand = new AddLimitCommand();
            RemoveLimitCommand = new RemoveLimitCommand();
            AddPathCommand = new AddPathCommand();
            RemovePathCommand = new RemovePathCommand();
        }
    }
}