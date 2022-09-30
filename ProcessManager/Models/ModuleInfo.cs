using System.Globalization;
using System.IO;

namespace ProcessManager.Models
{
    /// <summary>
    /// Contiene informazioni su un modulo.
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// Percorso completo del modulo.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Nome del modulo.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indirizzo di memoria di base del modulo.
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Dimensione del modulo.
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Descrizione del modulo.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ModuleInfo"/>.
        /// </summary>
        /// <param name="BaseAddress">Indirizzo di memoria di base del modulo.</param>
        /// <param name="Size">Dimensione del modulo.</param>
        /// <param name="Description">Descrizione del modulo.</param>
        public ModuleInfo(string FullPath, string BaseAddress, uint Size, string Description)
        {
            this.FullPath = FullPath;
            Name = Path.GetFileName(FullPath);
            this.BaseAddress = BaseAddress;
            double CalculatedValue;
            if (Size is >= 1048576 and < 1073741824)
            {
                CalculatedValue = (double)Size / 1024 / 1024;
                this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (Size >= 1073741824)
            {
                CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (Size < 1048576)
            {
                CalculatedValue = (double)Size / 1024;
                this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            this.Description = Description;
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="ModuleInfo"/>.
        /// </summary>
        /// <param name="BaseAddress">Indirizzo di memoria di base del modulo.</param>
        /// <param name="Size">Dimensione del modulo.</param>
        /// <param name="Description">Descrizione del modulo.</param>
        public ModuleInfo(string FullPath, string BaseAddress, string Size, string Description)
        {
            this.FullPath = FullPath;
            Name = Path.GetFileName(FullPath);
            this.BaseAddress = BaseAddress;
            this.Size = Size;
            this.Description = Description;
        }

        /// <summary>
        /// Visualizza la finestra delle proprietà del modulo.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public bool ShowModulePropertiesWindow()
        {
            return NativeHelpers.OpenModulePropertiesWindow(FullPath);
        }
    }
}