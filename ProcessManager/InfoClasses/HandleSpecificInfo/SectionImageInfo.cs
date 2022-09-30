using System;
using System.Globalization;
using ProcessManager.InfoClasses.PEFileStructures;

namespace ProcessManager.InfoClasses.HandleSpecificInfo
{
    /// <summary>
    /// Informazioni su una sezione reltive a un file.
    /// </summary>
    public class SectionImageInfo
    {
        /// <summary>
        /// Punto di entrata dell'immagine.
        /// </summary>
        public string EntryPoint { get; }

        /// <summary>
        /// Numero di bit dalla sinistra dell'indirizzo dello stack che devono avere valore 0.
        /// </summary>
        public string StackZeroBits { get; }

        /// <summary>
        /// Dimensione totale dello stack, in bytes.
        /// </summary>
        public string StackReserved { get; }

        /// <summary>
        /// Dimensione iniziale mappata dello stack, in bytes.
        /// </summary>
        public string StackCommit { get; }

        /// <summary>
        /// Sottosistema.
        /// </summary>
        public string ImageSubsystem { get; }

        /// <summary>
        /// Versione del sottosistema.
        /// </summary>
        public string SubsystemVersion { get; }

        /// <summary>
        /// Caratteristiche DLL dell'immagine.
        /// </summary>
        public string ImageDLLCharacteristics { get; }

        /// <summary>
        /// Tipo di macchina.
        /// </summary>
        public string ImageMachineType { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SectionImageInfo"/>.
        /// </summary>
        /// <param name="EntryPoint">Punto di entrata.</param>
        /// <param name="StackZeroBits">Numbero di bit il cui valore deve essere 0 a partire dalla sinistra dell'indirizzo.</param>
        /// <param name="StackReserved">Dimensione, in bytes, dello stack.</param>
        /// <param name="StackCommit">Dimensione mappata dello stack, in bytes.</param>
        /// <param name="Subsystem">Sottosistema.</param>
        /// <param name="SubsystemVersion">Versione del sottosistema.</param>
        /// <param name="DllCharacteristics">Caratteristiche DLL dell'immagine.</param>
        /// <param name="MachineType">Tipo di macchina.</param>
        public SectionImageInfo(IntPtr EntryPoint, uint? StackZeroBits, uint? StackReserved, uint? StackCommit, Subsystem? Subsystem, string SubsystemVersion, DllCharacteristics? DllCharacteristics, MachineType? MachineType)
        {
            this.EntryPoint = "0x" + EntryPoint.ToString("X");
            if (StackZeroBits.HasValue)
            {
                this.StackZeroBits = Convert.ToString(StackZeroBits.Value, CultureInfo.CurrentCulture);
            }
            else
            {
                this.StackZeroBits = Properties.Resources.UnavailableText;
            }
            if (StackReserved.HasValue)
            {
                this.StackReserved = Convert.ToString(StackReserved.Value, CultureInfo.CurrentCulture);
            }
            else
            {
                this.StackReserved = Properties.Resources.UnavailableText;
            }
            if (StackCommit.HasValue)
            {
                this.StackCommit = Convert.ToString(StackCommit.Value, CultureInfo.CurrentCulture);
            }
            else
            {
                this.StackCommit = Properties.Resources.UnavailableText;
            }
            if (Subsystem.HasValue)
            {
                ImageSubsystem = Subsystem.Value.ToString("f");
            }
            else
            {
                ImageSubsystem = Properties.Resources.UnavailableText;
            }
            this.SubsystemVersion = SubsystemVersion;
            if (DllCharacteristics.HasValue)
            {
                ImageDLLCharacteristics = DllCharacteristics.Value.ToString("f");
            }
            else
            {
                ImageDLLCharacteristics = Properties.Resources.UnavailableText;
            }
            if (MachineType.HasValue)
            {
                ImageMachineType = Convert.ToString(MachineType.Value.ToString("f"));
            }
            else
            {
                ImageMachineType = Properties.Resources.UnavailableText;
            }
        }
    }
}