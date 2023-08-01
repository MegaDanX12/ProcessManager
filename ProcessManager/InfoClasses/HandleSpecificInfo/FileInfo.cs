using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su un file.
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Data di creazione.
        /// </summary>
        public string CreationTime { get; }

        /// <summary>
        /// Data ultimo accesso.
        /// </summary>
        public string LastAccessTime { get; }

        /// <summary>
        /// Data ultima modifica.
        /// </summary>
        public string LastWriteTime { get; }

        /// <summary>
        /// Attributi.
        /// </summary>
        public string Attributes { get; }

        /// <summary>
        /// Tipo di file.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Dimensione del file.
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Dimensione effettiva del file.
        /// </summary>
        public string CompressedSize { get; }

        /// <summary>
        /// Quantità di spazio allocato.
        /// </summary>
        public string AllocationSize { get; }

        /// <summary>
        /// Numero collegamenti al file.
        /// </summary>
        public string LinksCount { get; }

        /// <summary>
        /// Indica se il file deve essere cancellato.
        /// </summary>
        public string DeletePending { get; }

        /// <summary>
        /// Indica se l'oggetto è una directory.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// Numero seriale del volume che contiene il file.
        /// </summary>
        public string VolumeSerialNumber { get; }

        /// <summary>
        /// Identificatore del file.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="FileInfo"/>.
        /// </summary>
        /// <param name="CreationTime">Data di creazione.</param>
        /// <param name="LastAccessTime">Data ultimo accesso.</param>
        /// <param name="LastWriteTime">Data ultima modifica.</param>
        /// <param name="Attributes">Attributi.</param>
        /// <param name="Type">Tipo.</param>
        /// <param name="Size">Dimensione.</param>
        /// <param name="CompressedSize">Dimensione effettiva.</param>
        /// <param name="AllocationSize">Spazio allocato.</param>
        /// <param name="NumberOfLinks">Numero di collegamenti.</param>
        /// <param name="DeletePending">true se il file deve essere eliminato, false altrimenti.</param>
        /// <param name="Directory">true se l'oggetto è una directory, false altrimenti.</param>
        /// <param name="VolumeSerialNumber">Numero seriale del volume che contiene il file.</param>
        /// <param name="Identifier">Identificatore del file.</param>
        public FileInfo(DateTime? CreationTime, DateTime? LastAccessTime, DateTime? LastWriteTime, string Attributes, string Type, long? Size, ulong? CompressedSize, long? AllocationSize, uint? NumberOfLinks, bool? DeletePending, bool? Directory, ulong? VolumeSerialNumber, string Identifier)
        {
            if (CreationTime.HasValue)
            {
                this.CreationTime = CreationTime.Value.ToString("d", CultureInfo.CurrentCulture);
            }
            else
            {
                this.CreationTime = Properties.Resources.UnavailableText;
            }
            if (LastAccessTime.HasValue)
            {
                this.LastAccessTime = LastAccessTime.Value.ToString("d", CultureInfo.CurrentCulture);
            }
            else
            {
                this.LastAccessTime = Properties.Resources.UnavailableText;
            }
            if (LastWriteTime.HasValue)
            {
                this.LastWriteTime = LastWriteTime.Value.ToString("d", CultureInfo.CurrentCulture);
            }
            else
            {
                this.LastWriteTime = Properties.Resources.UnavailableText;
            }
            this.Attributes = Attributes ?? Properties.Resources.UnavailableText;
            this.Type = Type ?? Properties.Resources.UnavailableText;
            double CalculatedValue;
            if (Size.HasValue)
            {
                this.Size = ConvertSizeToString(Size.Value);
            }
            else
            {
                this.Size = Properties.Resources.UnavailableText;
            }
            if (CompressedSize.HasValue)
            {
                if (CompressedSize.Value >= 1048576 && CompressedSize.Value < 1073741824)
                {
                    CalculatedValue = (double)CompressedSize.Value / 1024 / 1024;
                    this.CompressedSize = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                }
                else if (CompressedSize.Value >= 1073741824)
                {
                    CalculatedValue = (double)CompressedSize.Value / 1024 / 1024 / 1024;
                    this.CompressedSize = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                }
                else if (CompressedSize.Value < 1048576)
                {
                    CalculatedValue = (double)CompressedSize.Value / 1024;
                    this.CompressedSize = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                }
            }
            else
            {
                this.CompressedSize = Properties.Resources.UnavailableText;
            }
            if (AllocationSize.HasValue)
            {
                this.AllocationSize = ConvertSizeToString(AllocationSize.Value);
            }
            else
            {
                this.AllocationSize = Properties.Resources.UnavailableText;
            }
            if (NumberOfLinks.HasValue)
            {
                LinksCount = NumberOfLinks.Value.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                LinksCount = Properties.Resources.UnavailableText;
            }
            if (DeletePending.HasValue)
            {
                if (DeletePending.Value)
                {
                    this.DeletePending = Properties.Resources.YesText;
                }
                else
                {
                    this.DeletePending = "No";
                }
            }
            else
            {
                this.DeletePending = Properties.Resources.UnavailableText;
            }
            if (Directory.HasValue)
            {
                if (Directory.Value)
                {
                    this.Directory = Properties.Resources.YesText;
                }
                else
                {
                    this.Directory = "No";
                }
            }
            else
            {
                this.Directory = Properties.Resources.UnavailableText;
            }
            if (VolumeSerialNumber.HasValue)
            {
                this.VolumeSerialNumber = VolumeSerialNumber.Value.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                this.VolumeSerialNumber = Properties.Resources.UnavailableText;
            }
            this.Identifier = Identifier ?? Properties.Resources.UnavailableText;
        }

        /// <summary>
        /// Converte un valore numerico in una stringa.
        /// </summary>
        /// <param name="Size">Valore da convertire.</param>
        /// <returns>La stringa equivalente al valore fornito.</returns>
        private static string ConvertSizeToString(long Size)
        {
            string SizeString = null;
            double CalculatedValue;
            if (Size >= 1048576 && Size < 1073741824)
            {
                CalculatedValue = (double)Size / 1024 / 1024;
                SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (Size >= 1073741824)
            {
                CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (Size < 1048576)
            {
                CalculatedValue = (double)Size / 1024;
                SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            return SizeString;
        }
    }
}