using ProcessManager.Models;
using System.Threading;

namespace ProcessManager.ViewModels
{
    public class ComputerInfoVM
    {
        /// <summary>
        /// Informazioni sull'hardware.
        /// </summary>
        public HardwareInfo Hardware { get; }

        /// <summary>
        /// Informazioni sul sistema operativo.
        /// </summary>
        public OSInfo OperatingSystem { get; }

        /// <summary>
        /// Timer per l'aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateDataTimer;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ComputerInfoVM"/>.
        /// </summary>
        public ComputerInfoVM()
        {
            Hardware = new();
            OperatingSystem = new();
            UpdateDataTimer = new((state) => UpdateData(), null, 500, Timeout.Infinite);
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        private void UpdateData()
        {
            Hardware.UpdateData();
            OperatingSystem.UpdateData();
            _ = UpdateDataTimer.Change(500, Timeout.Infinite);
        }
    }
}