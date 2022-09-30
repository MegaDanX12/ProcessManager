using Microsoft.Win32.SafeHandles;
using System.Globalization;
using System;
using System.Xml.Linq;
using System.IO;
using ProcessManager.InfoClasses.ProcessStatistics;

namespace ProcessManager.InfoClasses.ProcessStatisticsClasses
{
    /// <summary>
    /// Contiene le statistiche degli handle di un processo.
    /// </summary>
    public class HandleStatistics : StatisticsBase
    {
        private string HandleCountValue;

        /// <summary>
        /// Numero di handle aperti.
        /// </summary>
        public string HandleCount 
        {
            get
            {
                return HandleCountValue;
            }
            private set
            {
                if (HandleCountValue != value)
                {
                    HandleCountValue = value;
                    NotifyPropertyChanged(nameof(HandleCount));
                }
            }
        }

        /// <summary>
        /// Conteggio handle totale.
        /// </summary>
        public uint HandleCountNumber { get; private set; }

        private string GDIHandlesCountValue;

        /// <summary>
        /// Numero di handle GDI aperti.
        /// </summary>
        public string GDIHandlesCount 
        {
            get
            {
                return GDIHandlesCountValue;
            }
            private set
            {
                if (GDIHandlesCountValue != value)
                {
                    GDIHandlesCountValue = value;
                    NotifyPropertyChanged(nameof(GDIHandlesCount));
                }
            }
        }

        /// <summary>
        /// Conteggio handle GDI.
        /// </summary>
        public int GDIHandlesCountNumber { get; private set; }

        private string USERHandlesCountValue;

        /// <summary>
        /// Numero di handle USER aperti.
        /// </summary>
        public string USERHandlesCount 
        {
            get
            {
                return USERHandlesCountValue;
            }
            private set
            {
                if (USERHandlesCountValue != value)
                {
                    USERHandlesCountValue = value;
                    NotifyPropertyChanged(nameof(USERHandlesCount));
                }
            }
        }

        /// <summary>
        /// Conteggio handle USER.
        /// </summary>
        public int USERHandlesCountNumber { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="HandleStatistics"/>.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public HandleStatistics(SafeProcessHandle Handle)
        {
            HandleCountNumber = NativeHelpers.GetProcessHandleCount(Handle);
            GDIHandlesCountNumber = NativeHelpers.GetProcessGDIHandlesCount(Handle);
            USERHandlesCountNumber = NativeHelpers.GetProcessUSERHandlesCount(Handle);
            HandleCountValue = HandleCountNumber != 0 ? HandleCountNumber.ToString("N0", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            GDIHandlesCountValue = GDIHandlesCountNumber < 0 ? Properties.Resources.UnavailableText : GDIHandlesCountNumber.ToString("N0", CultureInfo.CurrentCulture);
            USERHandlesCountValue = USERHandlesCountNumber < 0 ? Properties.Resources.UnavailableText : USERHandlesCountNumber.ToString("N0", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public override void Update(SafeProcessHandle Handle)
        {
            HandleCountNumber = NativeHelpers.GetProcessHandleCount(Handle);
            GDIHandlesCountNumber = NativeHelpers.GetProcessGDIHandlesCount(Handle);
            USERHandlesCountNumber = NativeHelpers.GetProcessUSERHandlesCount(Handle);
            HandleCount = HandleCountNumber != 0 ? HandleCountNumber.ToString("N0", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            GDIHandlesCount = GDIHandlesCountNumber < 0 ? Properties.Resources.UnavailableText : GDIHandlesCountNumber.ToString("N0", CultureInfo.CurrentCulture);
            USERHandlesCount = USERHandlesCountNumber < 0 ? Properties.Resources.UnavailableText : USERHandlesCountNumber.ToString("N0", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Scrive i dati di questa istanza in un file XML.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileXML(string FilePath)
        {
            string DocumentString =
                "<Handle statistics>" + Environment.NewLine +
                "   <TotalHandleCount>" + HandleCount + Environment.NewLine +
                "   <GDIHandleCount>" + GDIHandlesCount + Environment.NewLine +
                "   USERHandleCount>" + USERHandlesCount + Environment.NewLine +
                "</Handle statistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
        }

        /// <summary>
        /// Scrive i dati di questa istanza in un file di testo.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileText(string FilePath)
        {
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("Handle statistics on process termination");
            Writer.WriteLine();
            Writer.WriteLine("Total handle count: " + HandleCount);
            Writer.WriteLine("GDI handle count: " + GDIHandlesCount);
            Writer.WriteLine("USER handle count: " + USERHandlesCount);
        }

        /// <summary>
        /// Scrive i dati di questa istanza in un file binario.
        /// </summary>
        /// <param name="FilePath">Percorso dove salvare il file.</param>
        public override void WriteToFileBinary(string FilePath)
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(HandleCountNumber);
            Writer.Write(GDIHandlesCountNumber);
            Writer.Write(USERHandlesCountNumber);
        }
    }
}