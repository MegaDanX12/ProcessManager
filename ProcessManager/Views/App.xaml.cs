using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using static ProcessManager.NativeHelpers;

namespace ProcessManager
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Oggetto <see cref="Mutex"/> utilizzato per impedire l'esecuzione di più istanze dell'applicazione.
        /// </summary>
        private Mutex ApplicationMutex;

        /// <summary>
        /// Indica se riavviare l'applicazione.
        /// </summary>
        public static bool Restart { get; set; }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ApplicationMutex?.Close();
            _ = ChangeSystemWakeState(false, false, true);
            if (Restart)
            {
                _ = Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\ProcessManager.exe");
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Contains("/hung"))
            {

            }
            else
            {
                ApplicationMutex = new Mutex(true, "ProcessManagerSingleInstanceMutex", out bool MutexAcquired);
                if (!MutexAcquired)
                {
                    _ = MessageBox.Show(ProcessManager.Properties.Resources.ApplicationMultipleInstancesErrorMessage, ProcessManager.Properties.Resources.ApplicationMultipleInstancesErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown(-1);
                }
                else
                {
                    Exit += App_Exit;
                    Settings.LoadSettings();
                    Logger.Initialize(Settings.LogsPath);
                    Logger.WriteEntry(BuildLogEntryForInformation("Logger inizializzato", EventAction.ProgramStartup));
                    if (!EnableRequiredPrivilegesForCurrentProcess(out bool RequiredPrivilegesEnabled))
                    {
                        if (!RequiredPrivilegesEnabled)
                        {
                            Logger.WriteEntry(BuildLogEntryForWarning("Impossibile acquisire i privilegi necessari per il processo corrente", EventAction.ProgramStartup));
                            if (MessageBox.Show(ProcessManager.Properties.Resources.ApplicationDebugPrivilegesUnavailableMessage, ProcessManager.Properties.Resources.ApplicationDebugPrivilegesUnavailableTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                Restart = true;
                            }
                            Logger.WriteEntry(BuildLogEntryForInformation("Applicazione terminata", EventAction.ProgramTermination));
                            Current.Shutdown(-1);
                        }
                    }
                    else
                    {
                        Logger.WriteEntry(BuildLogEntryForInformation("Privilegi necessari acquisiti per il processo corrente", EventAction.ProgramStartup));
                    }
                }
            }
        }
    }
}