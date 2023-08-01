using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ProcessManager.InfoClasses.PEFileStructures;

namespace ProcessManager.InfoClasses.PEInfo
{
    /// <summary>
    /// Informazioni generali sul file eseguibile.
    /// </summary>
    public class PEHeaderGeneralInfo
    {
        /// <summary>
        /// Tipo di architettura del computer.
        /// </summary>
        public string TargetMachine { get; }

        /// <summary>
        /// Data e ora di creazione dell'immagine da parte del linker.
        /// </summary>
        /// <remarks>Rappresenta il numero di secondi trascorsi dalla mezzanotte del 1° gennaio 1970 (UTC) secondo il clock di sistema.</remarks>
        public string Timestamp { get; }

        /// <summary>
        /// L'indirizzo preferito del primo byte dell'immagine quando viene caricata in memoria.
        /// </summary>
        /// <remarks>Questo valore è un multiplo di 64 KB.<br/>
        /// Il valore di default per le DLL è 0x10000000, per le applicazioni è 0x00400000 eccetto su Windows CE che è 0x00010000.</remarks>
        public string ImageBase { get; }

        /// <summary>
        /// Il checksum dell'immagine.
        /// </summary>
        public string Checksum { get; }

        /// <summary>
        /// Sottosistema richiesto dall'immagine.
        /// </summary>
        public string Subsystem { get; }

        /// <summary>
        /// Versione del sottosistema.
        /// </summary>
        public string SubsystemVersion { get; }

        /// <summary>
        /// Caratteristiche dell'immagine.
        /// </summary>
        public string Characteristics { get; }

        /// <summary>
        /// Caratteristiche DLL.
        /// </summary>
        public string DLLCharacteristics { get; }

        /// <summary>
        /// Sezioni.
        /// </summary>
        public List<PESectionInfo> Sections { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="PEHeaderGeneralInfo"/>.
        /// </summary>
        /// <param name="TargetMachine">Macchina.</param>
        /// <param name="Timestamp">Data e ora di creazione.</param>
        /// <param name="ImageBase">Indirizzo base preferito dell'immagine.</param>
        /// <param name="Checksum">Checksum dell'immagine.</param>
        /// <param name="IsChecksumValid">Indica se il checksum è valido.</param>
        /// <param name="Subsystem">Sottosistema richiesto dall'immagine.</param>
        /// <param name="MajorSubVersion">Versione maggiore del sottosistema.</param>
        /// <param name="MinorSubVersion">Versione minore del sottosistema.</param>
        /// <param name="Characteristics">Caratteristiche dell'immagine.</param>
        /// <param name="DLLCharacteristics">Caratteristiche DLL dell'immagine.</param>
        /// <param name="Sections">Sezioni.</param>
        public PEHeaderGeneralInfo(MachineType TargetMachine, uint Timestamp, IntPtr ImageBase, uint Checksum, bool IsChecksumValid, Subsystem Subsystem, ushort MajorSubVersion, ushort MinorSubVersion, FileCharacteristics Characteristics, DllCharacteristics DLLCharacteristics, List<PESectionInfo> Sections)
        {
            this.TargetMachine = TargetMachine.ToString("f");
            DateTime TimestampFirstStep = new(1970, 1, 1);
            DateTime TimestampComplete = TimestampFirstStep.AddSeconds(Timestamp);
            this.Timestamp = TimestampComplete.ToString(CultureInfo.CurrentCulture);
            this.ImageBase = "0x" + ImageBase.ToString("X");
            if (IsChecksumValid)
            {
                this.Checksum = "0x" + Checksum.ToString("X", CultureInfo.CurrentCulture) + " (valid)";
            }
            else
            {
                this.Checksum = "0x" + Checksum.ToString("X", CultureInfo.CurrentCulture) + " (invalid)";
            }
            this.Subsystem = Subsystem.ToString("f");
            SubsystemVersion = MajorSubVersion.ToString("D0", CultureInfo.CurrentCulture) + "." + MinorSubVersion.ToString("D0", CultureInfo.CurrentCulture);
            this.Characteristics = Characteristics.ToString("f");
            this.DLLCharacteristics = DLLCharacteristics.ToString("f");
            this.Sections = Sections;
        }
    }
}