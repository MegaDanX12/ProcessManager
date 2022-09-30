using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.ProcessStatistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace ProcessManager.InfoClasses.ProcessStatisticsClasses
{
    /// <summary>
    /// Contiene le statistiche di memoria del processo.
    /// </summary>
    public class MemoryStatistics : StatisticsBase
    {
        private string PrivateBytesValue;

        /// <summary>
        /// Memoria privata.
        /// </summary>
        public string PrivateBytes 
        {
            get
            {
                return PrivateBytesValue;
            }
            private set
            {
                if (PrivateBytesValue != value)
                {
                    PrivateBytesValue = value;
                    NotifyPropertyChanged(nameof(PrivateBytes));
                }
            }
        }

        private string PeakPrivateBytesValue;

        /// <summary>
        /// Memoria privata massima.
        /// </summary>
        public string PeakPrivateBytes 
        {
            get
            {
                return PeakPrivateBytesValue;
            }
            private set
            {
                if (PeakPrivateBytesValue != value)
                {
                    PeakPrivateBytesValue = value;
                    NotifyPropertyChanged(nameof(PeakPrivateBytes));
                }
            }
        }

        private string VirtualSizeValue;

        /// <summary>
        /// Memoria virtuale.
        /// </summary>
        public string VirtualSize 
        {
            get
            {
                return VirtualSizeValue;
            }
            private set
            {
                if (VirtualSizeValue != value)
                {
                    VirtualSizeValue = value;
                    NotifyPropertyChanged(nameof(VirtualSize));
                }
            }
        }

        private string PeakVirtualSizeValue;

        /// <summary>
        /// Memoria virtuale massima.
        /// </summary>
        public string PeakVirtualSize 
        {
            get
            {
                return PeakVirtualSizeValue;
            }
            private set
            {
                if (PeakVirtualSizeValue != value)
                {
                    PeakVirtualSizeValue = value;
                    NotifyPropertyChanged(nameof(PeakVirtualSize));
                }
            }
        }

        private string PageFaultCountValue;

        /// <summary>
        /// Numero di page fault.
        /// </summary>
        public string PageFaultCount 
        {
            get
            {
                return PageFaultCountValue;
            }
            private set
            {
                if (PageFaultCountValue != value)
                {
                    PageFaultCountValue = value;
                    NotifyPropertyChanged(nameof(PageFaultCount));
                }
            }
        }

        private string WorkingSetSizeValue;

        /// <summary>
        /// Working set.
        /// </summary>
        public string WorkingSetSize 
        {
            get
            {
                return WorkingSetSizeValue;
            }
            private set
            {
                if (WorkingSetSizeValue != value)
                {
                    WorkingSetSizeValue = value;
                    NotifyPropertyChanged(nameof(WorkingSetSize));
                }
            }
        }

        private string PrivateWorkingSetSizeValue;

        /// <summary>
        /// Working set.
        /// </summary>
        public string PrivateWorkingSetSize
        {
            get
            {
                return PrivateWorkingSetSizeValue;
            }
            private set
            {
                if (PrivateWorkingSetSizeValue != value)
                {
                    PrivateWorkingSetSizeValue = value;
                    NotifyPropertyChanged(nameof(PrivateWorkingSetSize));
                }
            }
        }

        private string ShareableWorkingSetSizeValue;

        /// <summary>
        /// Working set.
        /// </summary>
        public string ShareableWorkingSetSize
        {
            get
            {
                return ShareableWorkingSetSizeValue;
            }
            private set
            {
                if (ShareableWorkingSetSizeValue != value)
                {
                    ShareableWorkingSetSizeValue = value;
                    NotifyPropertyChanged(nameof(ShareableWorkingSetSize));
                }
            }
        }

        private string SharedWorkingSetSizeValue;

        /// <summary>
        /// Working set.
        /// </summary>
        public string SharedWorkingSetSize
        {
            get
            {
                return SharedWorkingSetSizeValue;
            }
            private set
            {
                if (SharedWorkingSetSizeValue != value)
                {
                    SharedWorkingSetSizeValue = value;
                    NotifyPropertyChanged(nameof(SharedWorkingSetSizeValue));
                }
            }
        }

        private string PeakWorkingSetSizeValue;

        /// <summary>
        /// Working set massimo.
        /// </summary>
        public string PeakWorkingSetSize 
        {
            get
            {
                return PeakWorkingSetSizeValue;
            }
            private set
            {
                if (PeakWorkingSetSizeValue != value)
                {
                    PeakWorkingSetSizeValue = value;
                    NotifyPropertyChanged(nameof(PeakWorkingSetSize));
                }
            }
        }

        /// <summary>
        /// Dati sull'uso della memoria.
        /// </summary>
        public ulong[] MemoryData { get; private set; }

        /// <summary>
        /// Informazioni dettagliate sul working set.
        /// </summary>
        public ulong[] WsData { get; private set; }

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="MemoryStatistics"/>.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        public MemoryStatistics(SafeProcessHandle Handle)
        {
            uint MemoryPageSize = NativeHelpers.GetMemoryPageSize();
            MemoryData = NativeHelpers.GetProcessMemorySizes(Handle);
            WsData = NativeHelpers.GetWorkingSetDetailedInfo(Handle, MemoryPageSize);
            if (MemoryData != null)
            {
                string[] MemoryDataAsString = ConvertMemoryDataToString();
                PrivateBytesValue = MemoryDataAsString[0];
                PeakPrivateBytesValue = MemoryDataAsString[1];
                VirtualSizeValue = MemoryDataAsString[2];
                PeakVirtualSizeValue = MemoryDataAsString[3];
                PageFaultCountValue = MemoryDataAsString[4];
                WorkingSetSizeValue = MemoryDataAsString[5];
                PrivateWorkingSetSizeValue = MemoryDataAsString[6];
                ShareableWorkingSetSizeValue = MemoryDataAsString[7];
                SharedWorkingSetSizeValue = MemoryDataAsString[8];
                PeakWorkingSetSizeValue = MemoryDataAsString[9];
            }
            else
            {
                PrivateBytesValue = Properties.Resources.UnavailableText;
                PeakPrivateBytesValue = Properties.Resources.UnavailableText;
                VirtualSizeValue = Properties.Resources.UnavailableText;
                PeakVirtualSizeValue = Properties.Resources.UnavailableText;
                PageFaultCountValue = Properties.Resources.UnavailableText;
                WorkingSetSizeValue = Properties.Resources.UnavailableText;
                PrivateWorkingSetSizeValue = Properties.Resources.UnavailableText;
                ShareableWorkingSetSizeValue = Properties.Resources.UnavailableText;
                SharedWorkingSetSizeValue = Properties.Resources.UnavailableText;
                PeakWorkingSetSizeValue = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Converte i valori numerici delle informazioni sulla memoria del processo in stringhe.
        /// </summary>
        /// <param name="MemoryData">Dati generici sulla memoria del processo.</param>
        /// <param name="WsData">Dati specifici sul working set del processo.</param>
        /// <returns>Una array di stringhe con le informazioni convertite.</returns>
        private string[] ConvertMemoryDataToString()
        {
            string[] ConvertedData = new string[10];
            double CalculatedValue;
            string SingleConvertedData = null;
            for (int i = 0; i < ConvertedData.Length; i++)
            {
                if (i == 6)
                {
                    if (WsData != null)
                    {
                        for (int j = 0; j < WsData.Length; j++)
                        {
                            if (WsData[j] >= 1048576 && WsData[j] < 1073741824)
                            {
                                CalculatedValue = (double)WsData[j] / 1024 / 1024;
                                SingleConvertedData = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                            }
                            else if (WsData[j] >= 1073741824)
                            {
                                CalculatedValue = (double)WsData[j] / 1024 / 1024 / 1024;
                                SingleConvertedData = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                            }
                            else if (WsData[j] < 1048576)
                            {
                                CalculatedValue = (double)WsData[j] / 1024;
                                SingleConvertedData = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                            }
                            if (j == 0)
                            {
                                ConvertedData[6] = SingleConvertedData;
                            }
                            else if (j == 1)
                            {
                                ConvertedData[7] = SingleConvertedData;
                            }
                            else if (j == 2)
                            {
                                ConvertedData[8] = SingleConvertedData;
                            }
                        }
                    }
                    else
                    {
                        ConvertedData[6] = ConvertedData[7] = ConvertedData[8] = Properties.Resources.UnavailableText;
                    }
                    i = 8;
                }
                else if (i == 4)
                {
                    ConvertedData[i] = MemoryData[i].ToString("N0", CultureInfo.InvariantCulture);
                }
                else if (i == 9)
                {
                    if (MemoryData[6] >= 1048576 && MemoryData[6] < 1073741824)
                    {
                        CalculatedValue = (double)MemoryData[6] / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                    }
                    else if (MemoryData[6] >= 1073741824)
                    {
                        CalculatedValue = (double)MemoryData[6] / 1024 / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                    }
                    else if (MemoryData[6] < 1048576)
                    {
                        CalculatedValue = (double)MemoryData[6] / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                    }
                }
                else
                {
                    if (MemoryData[i] >= 1048576 && MemoryData[i] < 1073741824)
                    {
                        CalculatedValue = (double)MemoryData[i] / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                    }
                    else if (MemoryData[i] >= 1073741824)
                    {
                        CalculatedValue = (double)MemoryData[i] / 1024 / 1024 / 1024;
                        ConvertedData[i] = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                    }
                    else if (MemoryData[i] < 1048576)
                    {
                        CalculatedValue = (double)MemoryData[i] / 1024;
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
            uint MemoryPageSize = NativeHelpers.GetMemoryPageSize();
            MemoryData = NativeHelpers.GetProcessMemorySizes(Handle);
            WsData = NativeHelpers.GetWorkingSetDetailedInfo(Handle, MemoryPageSize);
            if (MemoryData != null)
            {
                string[] MemoryDataAsString = ConvertMemoryDataToString();
                PrivateBytes = MemoryDataAsString[0];
                PeakPrivateBytes = MemoryDataAsString[1];
                VirtualSize = MemoryDataAsString[2];
                PeakVirtualSize = MemoryDataAsString[3];
                PageFaultCount = MemoryDataAsString[4];
                WorkingSetSize = MemoryDataAsString[5];
                PrivateWorkingSetSize = MemoryDataAsString[6];
                ShareableWorkingSetSize = MemoryDataAsString[7];
                SharedWorkingSetSize = MemoryDataAsString[8];
                PeakWorkingSetSize = MemoryDataAsString[9];
            }
            else
            {
                PrivateBytes = Properties.Resources.UnavailableText;
                PeakPrivateBytes = Properties.Resources.UnavailableText;
                VirtualSize = Properties.Resources.UnavailableText;
                PeakVirtualSize = Properties.Resources.UnavailableText;
                PageFaultCount = Properties.Resources.UnavailableText;
                WorkingSetSize = Properties.Resources.UnavailableText;
                PrivateWorkingSetSize = Properties.Resources.UnavailableText;
                ShareableWorkingSetSize = Properties.Resources.UnavailableText;
                SharedWorkingSetSize = Properties.Resources.UnavailableText;
                PeakWorkingSetSize = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file XML.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public override void WriteToFileXML(string FilePath)
        {
            string DocumentString =
                "<MemoryStatistics>" + Environment.NewLine +
                "   <PrivateBytes>" + MemoryData[0].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <PeakPrivateBytes>" + MemoryData[1].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <VirtualSize>" + MemoryData[2].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <PeakVirtualSize>" + MemoryData[3].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <PageFaultCount>" + MemoryData[4].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <WorkingSetSize>" + MemoryData[5].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <PrivateWorkingSetSize>" + WsData[0].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <ShareableWorkingSetSize>" + WsData[1].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <SharedWorkingSetSize>" + WsData[2].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "   <PeakWorkingSetSize>" + MemoryData[6].ToString("N0", CultureInfo.InvariantCulture) + Environment.NewLine +
                "</MemoryStatistics>";
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
            Writer.WriteLine("Memory statistics on process termination");
            Writer.WriteLine();
            Writer.WriteLine("Private memory: " + PrivateBytes);
            Writer.WriteLine("Peak private memory: " + PeakPrivateBytes);
            Writer.WriteLine("Virtual memory size: " + VirtualSize);
            Writer.WriteLine("Peak virtual memory size: " + PeakVirtualSize);
            Writer.WriteLine("Page fault count: " + PageFaultCount);
            Writer.WriteLine("Working set size: " + WorkingSetSize);
            Writer.WriteLine("Private working set size: " + PrivateWorkingSetSize);
            Writer.WriteLine("Shareable working set size: " + ShareableWorkingSetSize);
            Writer.WriteLine("Shared working set size: " + SharedWorkingSetSize);
            Writer.WriteLine("Peak working set size: " + PeakWorkingSetSize);
        }

        /// <summary>
        /// Scrive i dati del monitoraggio in un file binario.
        /// </summary>
        /// <param name="FilePath">Percorso del file.</param>
        public override void WriteToFileBinary(string FilePath)
        {
            using FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter Writer = new(fs);
            Writer.Write(MemoryData[0]);
            Writer.Write(MemoryData[1]);
            Writer.Write(MemoryData[2]);
            Writer.Write(MemoryData[3]);
            Writer.Write(MemoryData[4]);
            Writer.Write(MemoryData[5]);
            Writer.Write(WsData[0]);
            Writer.Write(WsData[1]);
            Writer.Write(WsData[2]);
            Writer.Write(MemoryData[6]);
        }
    }
}