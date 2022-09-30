using ProcessManager.Commands.UserWindowCommands;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class UserInfoVM
    {
        /// <summary>
        /// Seleziona un utente locale.
        /// </summary>
        public ICommand SelectUserCommand { get; }

        /// <summary>
        /// Conferma l'utente selezionato.
        /// </summary>
        public ICommand ConfirmUserSettingsCommand { get; }

        /// <summary>
        /// Comando di conferma selezione utente.
        /// </summary>
        public ICommand ConfirmUserSelectionCommand { get; }

        public UserInfoVM()
        {
            SelectUserCommand = new SelectUserCommand(this);
            ConfirmUserSettingsCommand = new ConfirmUserSettingsCommand();
            ConfirmUserSelectionCommand = new ConfirmUserSelectionCommand(this);
        }
    }
}