using static ProcessManager.NativeMethods;

namespace ProcessManager.InfoClasses.SystemParametersInfoStructures
{
    /// <summary>
    /// Impostazioni dell'antialias dei font.
    /// </summary>
    public struct FontSmoothingSettings
    {
        /// <summary>
        /// Indica se l'antialias dei font è abilitato.
        /// </summary>
        public bool? IsEnabled { get; }
        /// <summary>
        /// Valore di contrasto usato dall'antialias ClearType.
        /// </summary>
        public uint? Contrast { get; }
        /// <summary>
        /// Orientamento dell'antialias.
        /// </summary>
        public string Orientation { get; }
        /// <summary>
        /// Tipo di antialias.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="FontSmoothingSettings"/>.
        /// </summary>
        /// <param name="Enabled">Indica se l'antialias è abilitato.</param>
        /// <param name="Contrast">Valore di contrasto usato dall'antialias ClearType.</param>
        /// <param name="Orientation">Orientamento dell'antialias.</param>
        /// <param name="Type">Tipo di antialias.</param>
        public FontSmoothingSettings(bool? Enabled, uint? Contrast, Win32Enumerations.FontSmoothingOrientation? Orientation, Win32Enumerations.FontSmoothingType? Type)
        {
            IsEnabled = Enabled;
            this.Contrast = Contrast;
            if (Orientation.HasValue)
            {
                this.Orientation = Orientation switch
                {
                    Win32Enumerations.FontSmoothingOrientation.FE_FONTSMOOTHINGORIENTATIONBGR => "BGR",
                    Win32Enumerations.FontSmoothingOrientation.FE_FONTSMOOTHINGORIENTATIONRGB => "RGB",
                    _ => null
                };
            }
            else
            {
                this.Orientation = Properties.Resources.UnavailableText;
            }
            if (Type.HasValue)
            {
                this.Type = Type switch
                {
                    Win32Enumerations.FontSmoothingType.FE_FONTSMOOTHINGCLEARTYPE => "ClearType",
                    Win32Enumerations.FontSmoothingType.FE_FONTSMOOTHINGSTANDARD => "Standard",
                    _ => null
                };
            }
            else
            {
                this.Type = Properties.Resources.UnavailableText;
            }
        }
    }
}