using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni sull'hardware.
    /// </summary>
    public class HardwareInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Architettura del processore.
        /// </summary>
        public string ProcessorArchitecture { get; }

        /// <summary>
        /// Dimensione di una pagina in memoria.
        /// </summary>
        public string MemoryPageSize { get; }

        /// <summary>
        /// Processori attivi.
        /// </summary>
        public byte ActiveProcessors { get; }

        /// <summary>
        /// Livello del processore.
        /// </summary>
        public ushort ProcessorLevel { get; }

        /// <summary>
        /// Revisione del processore.
        /// </summary>
        public ushort ProcessorRevision { get; }

        /// <summary>
        /// Nome del computer.
        /// </summary>
        public string ComputerName { get; }

        /// <summary>
        /// GUID del profilo hardware corrente.
        /// </summary>
        public string HardwareProfileGUID { get; }

        /// <summary>
        /// Nome del profilo hardware corrente.
        /// </summary>
        public string HardwareProfileName { get; }

        /// <summary>
        /// Tipo di firmware.
        /// </summary>
        public string FirmwareType { get; }

        /// <summary>
        /// Funzionalità del processore.
        /// </summary>
        public List<string> ProcessorFeatures { get; }

        /// <summary>
        /// Numero totale di core per ogni processore presente nel sistema.
        /// </summary>
        public uint ProcessorNumberOfCores { get; }

        /// <summary>
        /// Numero di processori fisici presenti nel sistema.
        /// </summary>
        public uint ProcessorNumberOfPackages { get; }

        /// <summary>
        /// Quantità di memoria fisica.
        /// </summary>
        public string PhysicalMemorySize { get; }

        private string MemoryLoadPercentageValue;

        /// <summary>
        /// Percentuale di utilizzo della memoria.
        /// </summary>
        public string MemoryLoadPercentage
        {
            get => MemoryLoadPercentageValue;
            private set
            {
                if (MemoryLoadPercentageValue != value)
                {
                    MemoryLoadPercentageValue = value;
                    NotifyPropertyChanged(nameof(MemoryLoadPercentage));
                }
            }
        }

        /// <summary>
        /// Quantità di memoria fisica disponibile all'uso del sistema operativo.
        /// </summary>
        public string TotalMemoryAvailable { get; }

        private string CurrentMemoryAvailableValue;

        /// <summary>
        /// Quantità di memoria attualmente disponibile.
        /// </summary>
        public string CurrentMemoryAvailable
        {
            get => CurrentMemoryAvailableValue;
            private set
            {
                if (CurrentMemoryAvailableValue != value)
                {
                    CurrentMemoryAvailableValue = value;
                    NotifyPropertyChanged(nameof(CurrentMemoryAvailable));
                }
            }
        }

        private string TotalCommittedPagesValue;

        /// <summary>
        /// Numero di pagine mappate dal sistema.
        /// </summary>
        public string TotalCommittedPages
        {
            get => TotalCommittedPagesValue;
            private set
            {
                if (TotalCommittedPagesValue != value)
                {
                    TotalCommittedPagesValue = value;
                    NotifyPropertyChanged(nameof(TotalCommittedPages));
                }
            }
        }

        private string MemoryCommitLimitPagesValue;

        /// <summary>
        /// Massimo numero di pagine che possono essere mappate dal sistema senza estendere il file di paging.
        /// </summary>
        public string MemoryCommitLimitPages
        {
            get => MemoryCommitLimitPagesValue;
            private set
            {
                if (MemoryCommitLimitPagesValue != value)
                {
                    MemoryCommitLimitPagesValue = value;
                    NotifyPropertyChanged(nameof(MemoryCommitLimitPages));
                }
            }
        }

        private string MemoryCommitPeakPagesValue;

        /// <summary>
        /// Massimo numero di pagine che sono simultaneamente mappate dall'ultimo riavvio del sistema.
        /// </summary>
        public string MemoryCommitPeakPages
        {
            get => MemoryCommitPeakPagesValue;
            private set
            {
                if (MemoryCommitPeakPagesValue != value)
                {
                    MemoryCommitPeakPagesValue = value;
                    NotifyPropertyChanged(nameof(MemoryCommitPeakPages));
                }
            }
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="HardwareInfo"/>.
        /// </summary>
        public HardwareInfo()
        {
            Dictionary<string, object> ProcessorInfo = NativeHelpers.GetSystemProcessorInfo();
            Dictionary<string, string> HardwareProfileInfo = NativeHelpers.GetHardwareProfileInfo();
            Dictionary<string, object> MemoryInfo = NativeHelpers.GetPhysicalMemoryInfo();
            Dictionary<string, string> OtherInfo = NativeHelpers.GetOtherComputerInfo();
            ActiveProcessors = (byte)ProcessorInfo["ActiveProcessors"];
            ProcessorArchitecture = (string)ProcessorInfo["Architecture"];
            ProcessorLevel = (ushort)ProcessorInfo["Level"];
            ProcessorRevision = (ushort)ProcessorInfo["Revision"];
            ProcessorFeatures = (List<string>)ProcessorInfo["Features"];
            ProcessorNumberOfCores = (uint)ProcessorInfo["NumberOfCores"];
            ProcessorNumberOfPackages = (uint)ProcessorInfo["NumberOfPackages"];
            HardwareProfileGUID = HardwareProfileInfo["HardwareProfileGuid"];
            HardwareProfileName = HardwareProfileInfo["HardwareProfileName"];
            if (MemoryInfo["PageSize"] is not string)
            {
                MemoryPageSize = UtilityMethods.ConvertSizeValueToString((ulong)MemoryInfo["PageSize"]);
            }
            else
            {
                MemoryPageSize = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["PhysicalMemorySize"] is not string)
            {
                PhysicalMemorySize = UtilityMethods.ConvertSizeValueToString2((ulong)MemoryInfo["PhysicalMemorySize"]);
            }
            else
            {
                PhysicalMemorySize = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["MemoryLoadPercentage"] is not string)
            {
                MemoryLoadPercentageValue = Convert.ToString((uint)MemoryInfo["MemoryLoadPercentage"], CultureInfo.CurrentCulture) + "%";
            }
            else
            {
                MemoryLoadPercentageValue = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["OSAvailableMemory"] is not string)
            {
                TotalMemoryAvailable = UtilityMethods.ConvertSizeValueToString((ulong)MemoryInfo["OSAvailableMemory"]);
            }
            else
            {
                TotalMemoryAvailable = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["CurrentlyAvailableMemory"] is not string)
            {
                CurrentMemoryAvailableValue = UtilityMethods.ConvertSizeValueToString((ulong)MemoryInfo["CurrentlyAvailableMemory"]);
            }
            else
            {
                CurrentMemoryAvailableValue = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["TotalPagesCommitted"] is not string)
            {
                TotalCommittedPagesValue = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["TotalPagesCommitted"]);
            }
            else
            {
                TotalCommittedPagesValue = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["PageCommitLimit"] is not string)
            {
                MemoryCommitLimitPagesValue = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["PageCommitLimit"]);
            }
            else
            {
                MemoryCommitLimitPagesValue = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["PageCommitPeak"] is not string)
            {
                MemoryCommitPeakPagesValue = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["PageCommitPeak"]);
            }
            else
            {
                MemoryCommitPeakPagesValue = Properties.Resources.UnavailableText;
            }
            ComputerName = OtherInfo["ComputerName"];
            FirmwareType = OtherInfo["FirmwareType"];
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            Dictionary<string, object> MemoryInfo = NativeHelpers.GetPhysicalMemoryInfo();
            if (MemoryInfo["MemoryLoadPercentage"] is not string)
            {
                MemoryLoadPercentage = Convert.ToString((uint)MemoryInfo["MemoryLoadPercentage"], CultureInfo.CurrentCulture) + "%";
            }
            else
            {
                MemoryLoadPercentage = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["CurrentlyAvailableMemory"] is not string)
            {
                CurrentMemoryAvailable = UtilityMethods.ConvertSizeValueToString((ulong)MemoryInfo["CurrentlyAvailableMemory"]);
            }
            else
            {
                CurrentMemoryAvailable = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["TotalPagesCommitted"] is not string)
            {
                TotalCommittedPages = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["TotalPagesCommitted"]);
            }
            else
            {
                TotalCommittedPages = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["PageCommitLimit"] is not string)
            {
                MemoryCommitLimitPages = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["PageCommitLimit"]);
            }
            else
            {
                MemoryCommitLimitPages = Properties.Resources.UnavailableText;
            }
            if (MemoryInfo["PageCommitPeak"] is not string)
            {
                MemoryCommitPeakPages = UtilityMethods.PagesCountToString((ulong)MemoryInfo["PageSize"], (ulong)MemoryInfo["PageCommitPeak"]);
            }
            else
            {
                MemoryCommitPeakPages = Properties.Resources.UnavailableText;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}