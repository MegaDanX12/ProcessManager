using Microsoft.Win32.SafeHandles;
using System.Globalization;
using System.ComponentModel;
using ProcessManager.InfoClasses.ProcessStatistics;
using System;
using System.Xml.Linq;
using System.IO;

namespace ProcessManager.InfoClasses.ProcessStatisticsClasses
{
    /// <summary>
    /// Contiene le statistiche I/O di un processo.
    /// </summary>
    public class IOStatistics : StatisticsBase
    {
        private string ReadCountValue;

        /// <summary>
        /// Numero di operazioni di lettura eseguite.
        /// </summary>
        public string ReadCount
        {
            get => ReadCountValue;
            private set
            {
                if (ReadCountValue != value)
                {
                    ReadCountValue = value;
                    NotifyPropertyChanged(nameof(ReadCount));
                }
            }
        }

        private string WriteCountValue;

        /// <summary>
        /// Numero di operazioni di scrittura eseguite.
        /// </summary>
        public string WriteCount
        {
            get => WriteCountValue;
            private set
            {
                if (WriteCountValue != value)
                {
                    WriteCountValue = value;
                    NotifyPropertyChanged(nameof(WriteCount));
                }
            }
        }

        private string OtherCountValue;

        /// <summary>
        /// Numero di operazione eseguite diverse da lettura e scrittura.
        /// </summary>
        public string OtherCount
        {
            get => OtherCountValue;
            private set
            {
                if (OtherCountValue != value)
                {
                    OtherCountValue = value;
                    NotifyPropertyChanged(nameof(OtherCount));
                }
            }
        }

        private string ReadBytesValue;

        /// <summary>
        /// Quantità di byte letti durante operazioni di lettura.
        /// </summary>
        public string ReadBytes
        {
            get => ReadBytesValue;
            private set
            {
                if (ReadBytesValue != value)
                {
                    ReadBytesValue = value;
                    NotifyPropertyChanged(nameof(ReadBytes));
                }
            }
        }

        private string WriteBytesValue;

        /// <summary>
        /// Quantità di byte scritti durante operazioni di scrittura.
        /// </summary>
        public string WriteBytes
        {
            get => WriteBytesValue;
            private set
            {
                if (WriteBytesValue != value)
                {
                    WriteBytesValue = value;
                    NotifyPropertyChanged(nameof(WriteBytes));
                }
            }
        }

        private string OtherBytesValue;

        /// <summary>
        /// Quantità di byte elaborati durante operazioni diverse da lettura e scrittura.
        /// </summary>
        public string OtherBytes
        {
            get => OtherBytesValue;
            private set
            {
                if (OtherBytesValue != value)
                {
                    OtherBytesValue = value;
                    NotifyPropertyChanged(nameof(OtherBytes));
                }
            }
        }

        /// <summary>
        /// Dati I/O del processo.
        /// </summary>
        public ulong[] IOData { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="IOStatistics"/>.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public IOStatistics(SafeProcessHandle Handle)
        {
            IOData = NativeHelpers.GetProcessIOInfo(Handle);
            if (IOData != null)
            {
                string[] IODataAsString = ConvertIODataToString(IOData);
                ReadCountValue = IODataAsString[0];
                WriteCountValue = IODataAsString[1];
                OtherCountValue = IODataAsString[2];
                ReadBytesValue = IODataAsString[3];
                WriteBytesValue = IODataAsString[4];
                OtherBytesValue = IODataAsString[5];
            }
        }

        /// <summary>
        /// Converte i dati delle elaborazioni I/O del processo in stringa.
        /// </summary>
        /// <param name="IOData">Dati sulle operazioni I/O del processo.</param>
        /// <returns>Un array di stringhe con le informazioni convertite.</returns>
        private static string[] ConvertIODataToString(ulong[] IOData)
        {
            ulong CalculatedValue;
            string[] ConvertedData = new string[6];
            for (int i = 0; i < IOData.Length; i++)
            {
                if (i is >= 0 and <= 2)
                {
                    ConvertedData[i] = IOData[i].ToString("N0", CultureInfo.CurrentCulture);
                }
                else
                {
                    if (IOData[i] is >= 1048576 and < 1073741824)
                    {
                        CalculatedValue = IOData[i] / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                    }
                    else if (IOData[i] >= 1073741824)
                    {
                        CalculatedValue = IOData[i] / 1024 / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                    }
                    else if (IOData[i] < 1048576)
                    {
                        CalculatedValue = IOData[i] / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                    }
                }
            }
            return ConvertedData;
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public override void Update(SafeProcessHandle Handle)
        {
            IOData = NativeHelpers.GetProcessIOInfo(Handle);
            if (IOData != null)
            {
                string[] IODataAsString = ConvertIODataToString(IOData);
                ReadCount = IODataAsString[0];
                WriteCount = IODataAsString[1];
                OtherCount = IODataAsString[2];
                ReadBytes = IODataAsString[3];
                WriteBytes = IODataAsString[4];
                OtherBytes = IODataAsString[5];
            }
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public override void WriteToFileXML(string FilePath)
        {
            string DocumentString =
                "<IOStatistics>" + Environment.NewLine +
                "   <ReadOperationsCount>" + IOData[0].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <WriteOperationsCount>" + IOData[1].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <OtherOperationsCount>" + IOData[2].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <ReadBytes>" + IOData[3].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <WriteBytes>" + IOData[4].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <OtherBytes>" + IOData[5].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "</IOStatistics>";
            XDocument doc = XDocument.Parse(DocumentString);
            doc.Save(FilePath);
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file di testo.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public override void WriteToFileText(string FilePath)
        {
            using StreamWriter Writer = new(FilePath, false);
            Writer.WriteLine("I/O statistics on process termination");
            Writer.WriteLine();
            Writer.WriteLine("Read operations count: " + ReadCount);
            Writer.WriteLine("Write operations count: " + WriteCount);
            Writer.WriteLine("Operations different from reading or writing count: " + OtherCount);
            Writer.WriteLine("Read bytes: " + ReadBytes);
            Writer.WriteLine("Written bytes: " + WriteBytes);
            Writer.WriteLine("Processed bytes (during operations different from reading or writing): " + OtherBytes);
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public override void WriteToFileBinary(string FilePath)
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(IOData[0]);
            Writer.Write(IOData[1]);
            Writer.Write(IOData[2]);
            Writer.Write(IOData[3]);
            Writer.Write(IOData[4]);
            Writer.Write(IOData[5]);
        }
    }
}