using System.Globalization;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni base su una sezione.
    /// </summary>
    public class SectionBasicInfo
    {
        /// <summary>
        /// Attributi.
        /// </summary>
        public string Attributes { get; }

        /// <summary>
        /// Dimensione, in bytes.
        /// </summary>
        public string Size { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SectionBasicInfo"/>.
        /// </summary>
        /// <param name="Attributes">Attributi della sezione.</param>
        /// <param name="Size">Dimensione, in bytes, della sezione.</param>
        public SectionBasicInfo(string Attributes, ulong? Size)
        {
            this.Attributes = Attributes;
            double CalculatedValue;
            if (Size.HasValue)
            {
                if (Size.Value >= 1048576 && Size.Value < 1073741824)
                {
                    CalculatedValue = (double)Size.Value / 1024 / 1024;
                    this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                }
                else if (Size.Value >= 1073741824)
                {
                    CalculatedValue = (double)Size.Value / 1024 / 1024 / 1024;
                    this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                }
                else if (Size.Value < 1048576)
                {
                    CalculatedValue = (double)Size.Value / 1024;
                    this.Size = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                }
            }
            else
            {
                this.Size = Properties.Resources.UnavailableText;
            }
        }
    }
}