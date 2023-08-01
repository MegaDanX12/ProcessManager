using System;
using System.ComponentModel;
using static ProcessManager.NativeMethods;

namespace ProcessManager.InfoClasses.SystemParametersInfoStructures
{
    /// <summary>
    /// Metriche icone.
    /// </summary>
    public struct IconMetrics
    {
        /// <summary>
        /// Spazio orizzontale, in pixel, per ogni icona.
        /// </summary>
        public int HorizontalSpacing { get; }
        /// <summary>
        /// Spazio verticale, in pixel, per ogni icona.
        /// </summary>
        public int VerticalSpacing { get; }
        /// <summary>
        /// Indica se i titoli delle icone vanno a capo automticamente.
        /// </summary>
        public bool TitleWrapEnabled { get; }
        /// <summary>
        /// Informazioni sul font utilizzato dalle icone.
        /// </summary>
        public FontData IconFont { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="IconMetrics"/>.
        /// </summary>
        /// <param name="HorizontalSpacing">Spazio orizzontale, in pixel, per ogni icona.</param>
        /// <param name="VerticalSpacing">Spazio verticale, in pixel, per ogni icona.</param>
        /// <param name="TitleWrapEnabled">Indica se i titoli delle icone vanno a capo automticamente.</param>
        /// <param name="IconFont">Informazioni sul font utilizzato dalle icone.</param>
        public IconMetrics(int HorizontalSpacing, int VerticalSpacing, bool TitleWrapEnabled, FontData IconFont)
        {
            this.HorizontalSpacing = HorizontalSpacing;
            this.VerticalSpacing = VerticalSpacing;
            this.TitleWrapEnabled = TitleWrapEnabled;
            this.IconFont = IconFont;
        }
    }

    /// <summary>
    /// Informazioni su un font.
    /// </summary>
    public struct FontData
    {
        /// <summary>
        /// Altezza, in unità logiche, del carattere.
        /// </summary>
        public int Height { get; }
        /// <summary>
        /// La larghezza media, in unità logiche, del carattere.
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// Angolo, in decimi di grado, tra il vettore di uscita e l'asse x del dispositivo.
        /// </summary>
        public int EscapementAngle { get; }
        /// <summary>
        /// Angolo, in decimi di grado, tra la base di ogni carattere e l'asse x del dispositivo.
        /// </summary>
        public int Orientation { get; }
        /// <summary>
        /// Peso del font.
        /// </summary>
        public FontWeight FontWeight { get; }
        /// <summary>
        /// Indica se il font è corsivo.
        /// </summary>
        public bool IsFontItalic { get; }
        /// <summary>
        /// Indica se il font è sottolineato.
        /// </summary>
        public bool IsFontUnderlined { get; }
        /// <summary>
        /// Indica se il font è sbarrato.
        /// </summary>
        public bool IsFontStrikeout { get; }
        /// <summary>
        /// Set di caratteri.
        /// </summary>
        public Charset FontCharset { get; }
        /// <summary>
        /// Precisione dell'output per il font.
        /// </summary>
        public FontOutputPrecision FontOutputPrecision { get; }
        /// <summary>
        /// Precisione del clipping per il font.
        /// </summary>
        public FontClippingPrecision FontClippingPrecision { get; }
        /// <summary>
        /// Qualità del font.
        /// </summary>
        public FontQuality FontQuality { get; }
        /// <summary>
        /// Inclinazione del font.
        /// </summary>
        public FontPitch FontPitch { get; }
        /// <summary>
        /// Famiglia del font.
        /// </summary>
        public FontFamily FontFamily { get; }
        /// <summary>
        /// Nome del font.
        /// </summary>
        public string FaceName { get; }

        /// <summary>
        /// Inizializza i membri della struttura <see cref="FontData"/>.
        /// </summary>
        /// <param name="Height">Altezza, in unità logiche, del carattere.</param>
        /// <param name="Width">Larghezza media, in unità logiche, del carattere.</param>
        /// <param name="Escapement">Angolo, in decimi di grado, tra il vettore di uscita e l'asse x del dispositivo.</param>
        /// <param name="Orientation">Angolo, in decimi di grado, tra la base di ogni carattere e l'asse x del dispositivo.</param>
        /// <param name="Weight">Peso del font.</param>
        /// <param name="Italic">Indica se il font è corsivo.</param>
        /// <param name="Underline">Indica se il font è sottolineato.</param>
        /// <param name="Strikeout">Indica se il font è sbarrato.</param>
        /// <param name="Charset">Set di caratteri.</param>
        /// <param name="OutputPrecision">Precisione dell'output per il font.</param>
        /// <param name="ClippingPrecision">Precisione del clipping per il font.</param>
        /// <param name="Quality">Qualità del font.</param>
        /// <param name="Pitch">Inclinazione del font.</param>
        /// <param name="Family">Famiglia del font.</param>
        /// <param name="FaceName">Nome del font.</param>
        public FontData(int Height, int Width, int Escapement, int Orientation, Win32Enumerations.FontWeight Weight, bool Italic, bool Underline, bool Strikeout, Win32Enumerations.Charset Charset, Win32Enumerations.OutputPrecision OutputPrecision, Win32Enumerations.ClipPrecision ClippingPrecision, Win32Enumerations.FontQuality Quality, Win32Enumerations.FontPitch Pitch, Win32Enumerations.FontFamily Family, string FaceName)
        {
            this.Height = Height;
            this.Width = Width;
            EscapementAngle = Escapement;
            this.Orientation = Orientation;
            FontWeight = (FontWeight)Weight;
            IsFontItalic = Italic;
            IsFontUnderlined = Underline;
            IsFontStrikeout = Strikeout;
            FontCharset = (Charset)Charset;
            FontOutputPrecision = (FontOutputPrecision)OutputPrecision;
            FontClippingPrecision = (FontClippingPrecision)ClippingPrecision;
            FontQuality = (FontQuality)Quality;
            FontPitch = (FontPitch)Pitch;
            FontFamily = (FontFamily)Family;
            this.FaceName = FaceName;
        }
    }

    /// <summary>
    /// Peso di un font.
    /// </summary>
    public enum FontWeight
    {
        [Description("Default")]
        DontCare,
        Thin = 100,
        Extralight = 200,
        Ultralight = Extralight,
        Light = 300,
        Normal = 400,
        Regular = Normal,
        Medium = 500,
        Semibold = 600,   
        Demibold = Semibold,
        Bold = 700,
        Extrabold = 800,
        Ultrabold = Extrabold,
        Heavy = 900,
        Black = Heavy
    }

    /// <summary>
    /// Set di caratteri.
    /// </summary>
    public enum Charset
    {
        [Description("Ansi charset")]
        Ansi,
        [Description("Default charset")]
        Default,
        [Description("Symbol charset")]
        Symbol,
        [Description("Japanese charset")]
        ShiftJis = 128,
        [Description("Korean charset")]
        Hangeul = 129,
        [Description("Korean charset")]
        Hangul = Hangeul,
        [Description("Simplified Chinese charset")]
        GB2312 = 134,
        [Description("Traditional chinese charset")]
        ChineseBig5 = 136,
        [Description("OEM charset")]
        Oem = 255,
        [Description("Korean charset")]
        Johab = 130,
        [Description("Hebrew charset")]
        Hebrew = 177,
        [Description("Arabic charset")]
        Arabic = 178,
        [Description("Greek charset")]
        Greek = 161,
        [Description("Turkish charset")]
        Turkish = 162,
        [Description("Vietnamese charset")]
        Vietnamese = 163,
        [Description("Thai charset")]
        Thai = 222,
        [Description("East Europe charset")]
        EastEurope = 238,
        [Description("Russian charset")]
        Russian = 204,
        [Description("Western European charset")]
        Mac = 77,
        [Description("Baltic charset")]
        Baltic = 186
    }

    /// <summary>
    /// Precisione dell'output per un font.
    /// </summary>
    public enum FontOutputPrecision
    {
        Default,
        String,
        Stroke = 3,
        TrueType,
        Device,
        Raster,
        [Description("TrueType only")]
        TrueTypeOnly,
        Outline,
        [Description("Screen outline")]
        ScreenOutline,
        [Description("PostScript only")]
        PostScriptOnly
    }

    /// <summary>
    /// Precisione clipping font.
    /// </summary>
    [Flags]
    public enum FontClippingPrecision
    {
        Default,
        Stroke,
        [Description("Depends on coordinate system orientation")]
        LeftHandedAngles = 1 << 4,
        [Description("Font association disabled")]
        Disabled = 4 << 4,
        [Description("Use embedded read-only font")]
        Embedded = 8 << 4
    }

    /// <summary>
    /// Qualità di un font.
    /// </summary>
    public enum FontQuality
    {
        Default,
        [Description("Low quality")]
        Draft,
        [Description("High quality")]
        Proof,
        [Description("Antialias disabled")]
        NonAntialised,
        [Description("Antialias enabled")]
        Antialised,
        [Description("ClearType quality")]
        ClearType,
        [Description("ClearType natural quality")]
        ClearTypeNatural
    }

    /// <summary>
    /// Inclinazione di un font.
    /// </summary>
    public enum FontPitch
    {
        Default,
        Fixed,
        Variable,
        MonoFont = 8
    }

    /// <summary>
    /// Famiglia del font.
    /// </summary>
    public enum FontFamily
    {
        [Description("Undefined")]
        DontCare = 0 << 4,
        Roman = 1 << 4,
        Swiss = 2 << 4,
        Modern = 3 << 4,
        Script = 4 << 4,
        Decorative = 5 << 4
    }
}