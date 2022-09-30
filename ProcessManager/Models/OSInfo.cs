using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace ProcessManager.Models
{
    /// <summary>
    /// Informazioni sul sistema operativo.
    /// </summary>
    public class OSInfo : INotifyPropertyChanged
    {
        private string SystemRunTimeValue;

        /// <summary>
        /// Tempo passato dall'avvio del sistema.
        /// </summary>
        public string SystemRunTime
        {
            get => SystemRunTimeValue;
            private set
            {
                if (SystemRunTimeValue != value)
                {
                    SystemRunTimeValue = value;
                    NotifyPropertyChanged(nameof(SystemRunTime));
                }
            }
        }

        /// <summary>
        /// Percorso della directory di sistema.
        /// </summary>
        public string SystemDirectoryPath { get; }

        /// <summary>
        /// Percorso della directory condivisa di sistema (sistemi multi utente).
        /// </summary>
        public string SystemSharedWindowsDirectoryPath { get; }

        /// <summary>
        /// Percorso della directory di sistema usata da WOW64.
        /// </summary>
        public string SystemWow64DirectoryPath { get; }

        private string SystemRegistryCurrentSizeValue;

        /// <summary>
        /// Dimensiona attuale del registro di sistema.
        /// </summary>
        public string SystemRegistryCurrentSize
        {
            get => SystemRegistryCurrentSizeValue;
            private set
            {
                if (SystemRegistryCurrentSizeValue != value)
                {
                    SystemRegistryCurrentSizeValue = value;
                    NotifyPropertyChanged(nameof(SystemRegistryCurrentSize));
                }
            }
        }

        /// <summary>
        /// Dimensione massima del registro di sistema.
        /// </summary>
        public string SystemRegistryMaximumSize { get; }

        /// <summary>
        /// Nome del prodotto.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Versione del sistema.
        /// </summary>
        public string SystemVersion { get; }

        /// <summary>
        /// Indica se il sistema è stato avviato da un contenitore VHD.
        /// </summary>
        public string BootFromVHD { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="OSInfo"/>.
        /// </summary>
        public OSInfo()
        {
            Dictionary<string, object> OSInfo = NativeHelpers.GetOSInfo();
            if (OSInfo["SystemUpTimeTicks"] is not string)
            {
                TimeSpan RunTime = new(0, 0, 0, 0, Convert.ToInt32((ulong)OSInfo["SystemUpTimeTicks"]));
                SystemRunTimeValue = BuildUpTimeString(RunTime);
            }
            else
            {
                SystemRunTimeValue = Properties.Resources.UnavailableText;
            }
            SystemDirectoryPath = (string)OSInfo["SystemDirectoryPath"];
            SystemSharedWindowsDirectoryPath = (string)OSInfo["SharedSystemDirectoryPath"];
            SystemWow64DirectoryPath = (string)OSInfo["WOW64SystemDirectoryPath"];
            if (OSInfo["SystemRegistryMaximumSize"] is not string)
            {
                SystemRegistryMaximumSize = UtilityMethods.ConvertSizeValueToString(Convert.ToUInt64((uint)OSInfo["SystemRegistryMaximumSize"]));
            }
            else
            {
                SystemRegistryMaximumSize = Properties.Resources.UnavailableText;
            }
            if (OSInfo["SystemRegistryCurrentSize"] is not string)
            {
                SystemRegistryCurrentSizeValue = UtilityMethods.ConvertSizeValueToString(Convert.ToUInt64((uint)OSInfo["SystemRegistryCurrentSize"]));
            }
            else
            {
                SystemRegistryCurrentSizeValue = Properties.Resources.UnavailableText;
            }
            SystemVersion = BuildSystemVersionString((uint)OSInfo["SystemMajorVersionNumber"], (uint)OSInfo["SystemMinorVersionNumber"], (uint)OSInfo["SystemBuildNumber"]);
            ProductName = (string)OSInfo["ProductName"];
            if (OSInfo["BootFromVHD"] is not string)
            {
                bool VHDBoot = (bool)OSInfo["BootFromVHD"];
                BootFromVHD = VHDBoot ? Properties.Resources.YesText : "No";
            }
            else
            {
                BootFromVHD = Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Costruisce la stringa del tempo di esecuzione del sistema operativo.
        /// </summary>
        /// <param name="RunTime">Struttura <see cref="TimeSpan"/> che rappresenta il tempo di esecuzione.</param>
        /// <returns>Una stringa che </returns>
        private static string BuildUpTimeString(TimeSpan RunTime)
        {
            StringBuilder UpTimeBuilder = new();
            #region Days
            if (RunTime.Days > 0)
            {
                if (RunTime.Days == 1)
                {
                    UpTimeBuilder.Append(RunTime.Days.ToString("N0", CultureInfo.CurrentCulture));
                    UpTimeBuilder.Append(" " + Properties.Resources.DayText);
                }
                else
                {
                    UpTimeBuilder.Append(RunTime.Days.ToString("N0", CultureInfo.CurrentCulture));
                    UpTimeBuilder.Append(" " + Properties.Resources.DaysText);
                }
            }
            #endregion
            #region Hours
            if (RunTime.Hours > 0)
            {
                if (RunTime.Hours == 1)
                {
                    if (UpTimeBuilder.Length > 0)
                    {
                        UpTimeBuilder.Append(", " + RunTime.Hours.ToString("D0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.HourText);
                    }
                    else
                    {
                        UpTimeBuilder.Append(RunTime.Hours.ToString("D0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.HourText);
                    }

                }
                else
                {
                    if (UpTimeBuilder.Length > 0)
                    {
                        UpTimeBuilder.Append(", " + RunTime.Hours.ToString("D0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.HoursText);
                    }
                    else
                    {
                        UpTimeBuilder.Append(RunTime.Hours.ToString("D0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.HoursText);
                    }
                }
            }
            #endregion
            #region Minutes
            if (RunTime.Minutes > 0)
            {
                if (RunTime.Minutes == 1)
                {
                    if (UpTimeBuilder.Length > 0)
                    {
                        UpTimeBuilder.Append(", " + RunTime.Minutes.ToString("N0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.MinuteText);
                    }
                    else
                    {
                        UpTimeBuilder.Append(RunTime.Minutes.ToString("N0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.MinuteText);
                    }

                }
                else
                {
                    if (UpTimeBuilder.Length > 0)
                    {
                        UpTimeBuilder.Append(", " + RunTime.Minutes.ToString("N0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.MinutesText);
                    }
                    else
                    {
                        UpTimeBuilder.Append(RunTime.Minutes.ToString("N0", CultureInfo.CurrentCulture));
                        UpTimeBuilder.Append(" " + Properties.Resources.MinutesText);
                    }
                }
            }
            #endregion
            return UpTimeBuilder.ToString();
        }

        /// <summary>
        /// Costruisce la stringa di versione del sistema operativo.
        /// </summary>
        /// <param name="MajorVersion">Numero di versione maggiore.</param>
        /// <param name="MinorVersion">Numero di versione minore.</param>
        /// <param name="BuildNumber">Numero della build.</param>
        /// <returns>La stringa di versione.</returns>
        private static string BuildSystemVersionString(uint MajorVersion, uint MinorVersion, uint BuildNumber)
        {
            StringBuilder VersionStringBuilder = new();
            _ = VersionStringBuilder.Append(MajorVersion.ToString("D0", CultureInfo.CurrentCulture) + ".");
            _ = VersionStringBuilder.Append(MinorVersion.ToString("D0", CultureInfo.CurrentCulture) + ".");
            _ =VersionStringBuilder.Append(BuildNumber.ToString("D0", CultureInfo.CurrentCulture));
            return VersionStringBuilder.ToString();
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        public void UpdateData()
        {
            Dictionary<string, object> OSInfo = NativeHelpers.GetOSInfo();
            if (OSInfo["SystemUpTimeTicks"] is not string)
            {
                TimeSpan RunTime = new(0, 0, 0, 0, Convert.ToInt32((ulong)OSInfo["SystemUpTimeTicks"]));
                SystemRunTime = BuildUpTimeString(RunTime);
            }
            else
            {
                SystemRunTime = Properties.Resources.UnavailableText;
            }
            if (OSInfo["SystemRegistryCurrentSize"] is not string)
            {
                SystemRegistryCurrentSize = UtilityMethods.ConvertSizeValueToString(Convert.ToUInt64((uint)OSInfo["SystemRegistryCurrentSize"]));
            }
            else
            {
                SystemRegistryCurrentSize = Properties.Resources.UnavailableText;
            }
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}