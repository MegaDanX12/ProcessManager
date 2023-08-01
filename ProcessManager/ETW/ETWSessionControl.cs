using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using ProcessManager.Properties;
using ProcessManager.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;

namespace ProcessManager.ETW
{
    /// <summary>
    /// Controlla la sessione ETW.
    /// </summary>
    public static class ETWSessionControl
    {
        /// <summary>
        /// Sessione ETW.
        /// </summary>
        private static TraceEventSession Session;

        /// <summary>
        /// Crea e avvia la sessione ETW .
        /// </summary>
        public static void StartSession(ProcessInfoVM VM)
        {
            try
            {
                Session = new(KernelTraceEventParser.KernelSessionName);
                KernelTraceEventParser.Keywords Keywords = KernelTraceEventParser.Keywords.Process | KernelTraceEventParser.Keywords.Thread | KernelTraceEventParser.Keywords.ImageLoad | KernelTraceEventParser.Keywords.Handle | KernelTraceEventParser.Keywords.VirtualAlloc;
                _ = Session.EnableKernelProvider(Keywords);
                ETWEventParser.InitializeMainEventsParser(Session.Source.Kernel, VM);
            }
            catch (COMException)
            {
                Settings.DataSource = ProcessDataSource.WMI;
                Settings.UpdateSettingsFile();
                App.Restart = true;
                _ = MessageBox.Show(Resources.ETWInitializationFailureErrorMessage, Resources.ETWInitializationFailureErroreTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(-1);
            }
        }

        /// <summary>
        /// Arresta la sessione ETW.
        /// </summary>
        public static void StopSession()
        {
            Session.Dispose();
        }

        /// <summary>
        /// Inizia l'elaborazione degli eventi della sessione ETW.
        /// </summary>
        public static void StartSessionEventsProcessing()
        {
            _ = Session.Source.Process();
        }
    }
}