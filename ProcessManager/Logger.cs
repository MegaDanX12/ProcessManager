using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace ProcessManager
{
    /// <summary>
    /// Rappresenta una voce di log.
    /// </summary>
    public struct LogEntry
    {
        /// <summary>
        /// Informazioni specifiche dell'evento.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Fonte dell'evento.
        /// </summary>
        public EventSource Source { get; }

        /// <summary>
        /// Gravità dell'evento.
        /// </summary>
        public EventSeverity Severity { get; }

        /// <summary>
        /// Azione.
        /// </summary>
        public EventAction Action { get; }

        /// <summary>
        /// Data dell'evento.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="LogEntry"/> con le informazioni fornite.
        /// </summary>
        /// <param name="Text">Informazioni specifiche.</param>
        /// <param name="Source">Fonte delle voce di log.</param>
        /// <param name="Severity">Gravità della voce di log.</param>
        public LogEntry(string Text, EventSource Source, EventSeverity Severity, EventAction Action)
        {
            this.Text = Text;
            this.Source = Source;
            this.Severity = Severity;
            this.Action = Action;
            Date = DateTime.Now;
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="LogEntry"/> con le informazioni fornite.
        /// </summary>
        /// <param name="Text">Informazioni specifiche.</param>
        /// <param name="Source">Fonte delle voce di log.</param>
        /// <param name="Severity">Gravità della voce di log.</param>
        /// <param name="Date">Data di registrazione della voce di log.</param>
        public LogEntry(string Text, EventSource Source, EventSeverity Severity, EventAction Action, DateTime Date)
        {
            this.Text = Text;
            this.Source = Source;
            this.Severity = Severity;
            this.Action = Action;
            this.Date = Date;
        }
    }

    /// <summary>
    /// Fonte di un evento.
    /// </summary>
    public enum EventSource
    {
        /// <summary>
        /// Evento di applicazione.
        /// </summary>
        Application,
        /// <summary>
        /// Evento generato da un chiamata alla Windows API.
        /// </summary>
        WindowsAPI,
        /// <summary>
        /// Evento generato da un processo diverso da quello corrente.
        /// </summary>
        Process
    }

    /// <summary>
    /// Gravità di un evento.
    /// </summary>
    public enum EventSeverity
    {
        /// <summary>
        /// Informazioni, nessun errore si è verificato.
        /// </summary>
        Information,
        /// <summary>
        /// Avviso, nessun errore si è verificato ma la stabilità dell'applicazione non è garantita.
        /// </summary>
        Warning,
        /// <summary>
        /// Errore, si è verificato un errore non fatale, non è stato possibile eseguire un'operazione o un componente dell'applicazione ha riscontrato un errore recuperabile.
        /// </summary>
        Error,
        /// <summary>
        /// Critico, si è verificato un errore fatale, l'applicazione non può continuare.
        /// </summary>
        Critical,
        /// <summary>
        /// Informazioni di debug.
        /// </summary>
        Debug
    }

    /// <summary>
    /// Azione.
    /// </summary>
    public enum EventAction
    {
        /// <summary>
        /// Terminazione di un processo.
        /// </summary>
        ProcessTermination,
        /// <summary>
        /// Terminazione di un processo e dei suoi figli.
        /// </summary>
        ProcessTreeTermination,
        /// <summary>
        /// Debug di un processo
        /// </summary>
        DebugProcess,
        /// <summary>
        /// Interruzione del debug di un processo.
        /// </summary>
        StopDebugProcess,
        /// <summary>
        /// Controllo se un processo è in corso in debug.
        /// </summary>
        DebugCheck,
        /// <summary>
        /// Recupero informazioni sulle finestre.
        /// </summary>
        WindowsInfoGeneral,
        /// <summary>
        /// Enumerazione delle finestre associate a un thread.
        /// </summary>
        ThreadWindowsEnum,
        /// <summary>
        /// Recupero informazioni su una finestra specifica.
        /// </summary>
        WindowInfoSpecific,
        /// <summary>
        /// Operazione su una finestra.
        /// </summary>
        WindowOperation,
        /// <summary>
        /// Modifica delle proprietà di un processo.
        /// </summary>
        ProcessPropertiesManipulation,
        /// <summary>
        /// Lettura delle proprietà di un processo.
        /// </summary>
        ProcessPropertiesRead,
        /// <summary>
        /// Lettura proprietà di un handle.
        /// </summary>
        HandlePropertiesRead,
        /// <summary>
        /// Manipolazione di un handle remoto.
        /// </summary>
        HandleManipulation,
        /// <summary>
        /// Enumerazione dei thread di un processo.
        /// </summary>
        ThreadEnumeration,
        /// <summary>
        /// Lettura proprietà di un thread.
        /// </summary>
        ThreadPropertiesRead,
        /// <summary>
        /// Modifica delle proprietà di un thread.
        /// </summary>
        ThreadPropertiesManipulation,
        /// <summary>
        /// Terminazione di un thread.
        /// </summary>
        ThreadTermination,
        /// <summary>
        /// Sospensione di un thread.
        /// </summary>
        ThreadSuspension,
        /// <summary>
        /// Ripresa di un thread.
        /// </summary>
        ThreadResume,
        /// <summary>
        /// Enumerazione dei moduli di un processo.
        /// </summary>
        ModulesEnumeration,
        /// <summary>
        /// Enumerazione di moduli caricati (generale).
        /// </summary>
        ModulesEnumeration2,
        /// <summary>
        /// Lettura proprietà di un modulo.
        /// </summary>
        ModulesPropertiesRead,
        /// <summary>
        /// Lettura di informazioni sulla memoria di un processo.
        /// </summary>
        MemoryInfoRead,
        /// <summary>
        /// Modifica delle informazioni sulla memoria di un processo.
        /// </summary>
        MemoryInfoManipulation,
        /// <summary>
        /// Lettura di altre proprietà di un processo.
        /// </summary>
        OtherPropertiesRead,
        /// <summary>
        /// Enumerazione dei processi.
        /// </summary>
        ProcessEnumeration,
        /// <summary>
        /// Altre azioni.
        /// </summary>
        OtherActions,
        /// <summary>
        /// Modifica delle informazioni di un token.
        /// </summary>
        TokenInfoManipulation,
        /// <summary>
        /// Lettura di informazioni da un token.
        /// </summary>
        TokenInfoRead,
        /// <summary>
        /// Avvio dell'applicazione.
        /// </summary>
        ProgramStartup,
        /// <summary>
        /// Arresto dell'applicazione.
        /// </summary>
        ProgramTermination,
        /// <summary>
        /// Azioni su un semaforo.
        /// </summary>
        SemaphoreOperation,
        /// <summary>
        /// Monitoraggio delle unità disco.
        /// </summary>
        VolumeMonitoring,
        /// <summary>
        /// Enumerazione dei servizi.
        /// </summary>
        ServicesEnumeration,
        /// <summary>
        /// Monitoraggio dello stato dei servizi.
        /// </summary>
        ServicesMonitoring,
        /// <summary>
        /// Azione relativa ai servizi.
        /// </summary>
        ServicesGeneral,
        /// <summary>
        /// Blocco del computer.
        /// </summary>
        LockMachine,
        /// <summary>
        /// Spegnimento del computer.
        /// </summary>
        ShutdownMachine,
        /// <summary>
        /// Riavvio del computer.
        /// </summary>
        RestartMachine,
        /// <summary>
        /// Passaggio allo stato in standby.
        /// </summary>
        SleepMachine,
        /// <summary>
        /// Ibernazione del computer.
        /// </summary>
        HibernateMachine,
        /// <summary>
        /// Disconnessione dell'utente.
        /// </summary>
        LogOffUser,
        /// <summary>
        /// Lettura di informazioni sul computer.
        /// </summary>
        ComputerInfoRead,
        /// <summary>
        /// Manipolazione del regole del watchdog.
        /// </summary>
        WatchdogRulesManipulation,
        /// <summary>
        /// Inizializzazione del watchdog.
        /// </summary>
        WatchdogInizialization,
        /// <summary>
        /// Arresto del watchdog.
        /// </summary>
        WatchdogShutdown,
        /// <summary>
        /// Inizializzazione del watchdog per un processo.
        /// </summary>
        ProcessWatchdogInizialization,
        /// <summary>
        /// Arresto del watchdog per un processo.
        /// </summary>
        ProcessWatchdogShutdown,
        /// <summary>
        /// Inizializzazione del limitatore processi.
        /// </summary>
        ProcessLimiterInitialization,
        /// <summary>
        /// Arresto del limitatore processi.
        /// </summary>
        ProcessLimiterShutdown,
        /// <summary>
        /// Aggiunta di un limite al limitatore processi.
        /// </summary>
        ProcessLimiterLimitAdd,
        /// <summary>
        /// Rimozione di un limite al limitatore processi.
        /// </summary>
        ProcessLimiterLimitRemove,
        /// <summary>
        /// Percorso di un eseguibile aggiunto a un limite impostato nel limitatore processi.
        /// </summary>
        ProcessLimiterPathAdd,
        /// <summary>
        /// Percorso di un eseguibile rimosso da un limite impostato nel limitatore processi.
        /// </summary>
        ProcessLimiterPathRemove,
        /// <summary>
        /// Richiesta di informazioni su un job del limitatore processi.
        /// </summary>
        ProcessLimiterJobQuery,
        /// <summary>
        /// Aggiunta di un processo a un job del limitatore processi.
        /// </summary>
        ProcessLimiterJobAdd,
        /// <summary>
        /// Enumerazione dei file di paging.
        /// </summary>
        PageFileEnumeration,
        /// <summary>
        /// Creazione di un file di paging.
        /// </summary>
        PageFileCreation,
        /// <summary>
        /// Modifica file di paging.
        /// </summary>
        PageFileManipulation,
        /// <summary>
        /// Monitoraggio dei file di paging.
        /// </summary>
        PageFileMonitoring,
        /// <summary>
        /// Eliminazione di un file di paging.
        /// </summary>
        PageFileDeletion,
        /// <summary>
        /// Connessione a un computer remoto.
        /// </summary>
        RemoteConnection,
        /// <summary>
        /// Monitoraggio della memoria di sistema.
        /// </summary>
        SystemMemoryMonitoring,
        /// <summary>
        /// Pulizia della memoria di sistema.
        /// </summary>
        SystemMemoryCleaning,
        /// <summary>
        /// Modifica dello stato energetico del sistema.
        /// </summary>
        SystemEnergyStateManipulation,
        /// <summary>
        /// Avvio di un processo.
        /// </summary>
        ProcessStart,
        /// <summary>
        /// Pulizia del working set di un processo.
        /// </summary>
        ProcessWorkingSetCleaning
    }

    /// <summary>
    /// Livello di logging.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Nessuno.
        /// </summary>
        None,
        /// <summary>
        /// Solo informazioni.
        /// </summary>
        Low,
        /// <summary>
        /// Informazioni e avvisi.
        /// </summary>
        Medium,
        /// <summary>
        /// Informazioni, avvisi ed errori.
        /// </summary>
        High
    }

    /// <summary>
    /// Esegue il logging delle attività del programma.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Oggetto <see cref="StreamWriter"/> per la scrittura sul file di log.
        /// </summary>
        private static StreamWriter Writer;

        /// <summary>
        /// Indica se è possibile leggere e scrivere sul log.
        /// </summary>
        private static bool IsInitialized;

        /// <summary>
        /// Percorso del file di log.
        /// </summary>
        private static string LogFilePath;

        /// <summary>
        /// Evento che indica se il log è disponibile.
        /// </summary>
        public static AutoResetEvent LogAvailable { get; private set; }

        /// <summary>
        /// Prepara <see cref="Logger"/> per la scrittura e la lettura del file di log.
        /// </summary>
        /// <param name="LogPath">Percorso del file di log.</param>
        /// <remarks>Se <see cref="Logger"/> è già stato inizializzato o se il logging è disattivato, questo metodo non esegue alcuna operazione.</remarks>
        public static void Initialize(string LogPath)
        {
            if (Settings.LogProgramActivity)
            {
                if (!IsInitialized)
                {
                    if (!Directory.Exists(LogPath))
                    {
                        try
                        {
                            _ = Directory.CreateDirectory(LogPath);
                        }
                        catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException or NotSupportedException)
                        {
                            LogPath = AppDomain.CurrentDomain.BaseDirectory;
                        }
                    }
                    FileStream LogFileStream = new(LogPath + "AppLog.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.RandomAccess);
                    Writer = new StreamWriter(LogFileStream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                    if (Writer.BaseStream.Length is 0 or 3)
                    {
                        CreateHeader();
                    }
                    else
                    {
                        _ = Writer.BaseStream.Seek(0, SeekOrigin.End);
                        Writer.WriteLine();
                    }
                    LogFilePath = LogPath + "AppLog.log";
                    LogAvailable = new(true);
                    IsInitialized = true;
                }
            }
        }

        /// <summary>
        /// Scrive l'intestazione del file di log.
        /// </summary>
        private static void CreateHeader()
        {
            Writer.WriteLine("Application Log File (" + DateTime.Now.ToShortDateString() + ")");
            Writer.WriteLine();
            Writer.WriteLine();
        }

        /// <summary>
        /// Scrive una voce di log.
        /// </summary>
        /// <param name="Entry">Informazioni sull'evento che si è verificato.</param>
        public static void WriteEntry(LogEntry Entry)
        {
            if (Settings.LogProgramActivity)
            {
                if (Settings.LoggingLevel == LogLevel.Low)
                {
                    if (Entry.Severity > EventSeverity.Information)
                    {
                        return;
                    }
                }
                else if (Settings.LoggingLevel == LogLevel.Medium)
                {
                    if (Entry.Severity > EventSeverity.Warning)
                    {
                        return;
                    }
                }
                else if (Settings.LoggingLevel == LogLevel.High)
                {
                    if (Entry.Severity > EventSeverity.Critical)
                    {
                        return;
                    }
                }
                if (Writer != null)
                {
                    StringBuilder sb = new();
                    _ = sb.Append(Entry.Date.ToShortDateString());
                    _ = sb.Append(' ');
                    _ = sb.Append(Entry.Date.ToShortTimeString());
                    _ = sb.Append(" [");
                    _ = sb.Append(Enum.GetName(typeof(EventSeverity), Entry.Severity));
                    _ = sb.Append("] [");
                    _ = sb.Append(Enum.GetName(typeof(EventSource), Entry.Source));
                    _ = sb.Append("] [");
                    _ = sb.Append(Enum.GetName(typeof(EventAction), Entry.Action));
                    _ = sb.Append("] ");
                    _ = sb.Append(Entry.Text);
                    _ = LogAvailable.WaitOne();
                    Writer.WriteLine(sb.ToString());
                    long CurrentSize = Writer.BaseStream.Length / 1024 / 1024;
                    if (CurrentSize >= Settings.MaxLogSize)
                    {
                        ClearLog();
                    }
                    _ = LogAvailable.Set();
                }
            }
        }

        /// <summary>
        /// Legge una voce di log dal file.
        /// </summary>
        /// <returns><see cref="LogEntry"/> con le informazioni sulla voce di log.</returns>
        private static LogEntry ReadEntry(string TextEntry)
        {
            string DateString = TextEntry.Remove(TextEntry.IndexOf("[", StringComparison.CurrentCulture) - 1);
            DateTime Date = DateTime.Parse(DateString, CultureInfo.CurrentCulture);
            string SeverityStringStep1 = TextEntry.Replace(DateString + " ", "");
            string SeverityString = SeverityStringStep1.Remove(SeverityStringStep1.IndexOf("]", StringComparison.CurrentCulture)).Replace("[", "");
            EventSeverity Severity = (EventSeverity)Enum.Parse(typeof(EventSeverity), SeverityString);
            string SourceStringStep1 = TextEntry.Replace(DateString + " ", "");
            string SourceStringStep2 = SourceStringStep1.Replace("[" + SeverityString + "]", "");
            string SourceStringStep3 = SourceStringStep2.Remove(0, 1);
            string SourceString = SourceStringStep3.Remove(SourceStringStep3.IndexOf("]", StringComparison.CurrentCulture)).Replace("[", "");
            EventSource Source = (EventSource)Enum.Parse(typeof(EventSource), SourceString);
            string EventStringStep1 = TextEntry.Replace(DateString + " ", "");
            string EventStringStep2 = EventStringStep1.Replace("[" + SeverityString + "] ", "");
            string EventStringStep3 = EventStringStep2.Replace("[" + SourceString + "] ", "");
            string EventString = EventStringStep3.Remove(EventStringStep3.IndexOf("]", StringComparison.CurrentCulture)).Replace("[", "");
            EventAction Action = (EventAction)Enum.Parse(typeof(EventAction), EventString);
            string Text = TextEntry.Remove(0, TextEntry.LastIndexOf("]", StringComparison.CurrentCulture) + 2);
            return new LogEntry(Text, Source, Severity, Action, Date);
        }

        /// <summary>
        /// Legge le voci di log.
        /// </summary>
        /// <returns>Un array con le voci di log lette.</returns>
        public static LogEntry[] GetLogEntries()
        {
            _ = LogAvailable.WaitOne();
            List<LogEntry> Entries = new();
            _ = Writer.BaseStream.Seek(0, SeekOrigin.Begin);
            string Line = string.Empty;
            int SkippedLines = 0;
            using (StreamReader sr = new(Writer.BaseStream, Encoding.UTF8, false, 4096, true))
            {
                _ = sr.ReadLine();
                SkippedLines += 1;
                while (Line != null)
                {
                    if (SkippedLines < 3)
                    {
                        _ = sr.ReadLine();
                        SkippedLines += 1;
                    }
                    else
                    {
                        Line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(Line))
                        {
                            Entries.Add(ReadEntry(Line));
                        }
                    }
                }
            }
            _ = Writer.BaseStream.Seek(0, SeekOrigin.End);
            _ = LogAvailable.Set();
            return Entries.ToArray();
        }

        /// <summary>
        /// Trasferisce i dati del file di log corrente in un altro file, elimina il file di log corrente e reinizializza il logger.
        /// </summary>
        public static void ClearLog()
        {
            _ = LogAvailable.WaitOne();
            if (Settings.KeepOldLogs)
            {
                string LogDirectoryPath = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(LogDirectoryPath + "\\Old logs"))
                {
                    _ = Directory.CreateDirectory(LogDirectoryPath + "\\Old logs");
                }
                using StreamReader sr = new(LogFilePath);
                string Header = sr.ReadLine();
                string CreationDateFull = Header.Substring(Header.IndexOf('('));
                string CreationDate = CreationDateFull.Remove(0, 1).Remove(CreationDateFull.Length - 1);
                using FileStream OldLogData = new(LogDirectoryPath + "\\Old logs\\" + CreationDate + "log", FileMode.Create, FileAccess.Write);
                sr.BaseStream.Position = 0;
                sr.BaseStream.CopyTo(OldLogData);
                sr.BaseStream.Flush();
            }
            Writer.Close();
            File.Delete(LogFilePath);
            FileStream LogFileStream = new(LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.RandomAccess);
            Writer = new StreamWriter(LogFileStream, Encoding.UTF8)
            {
                AutoFlush = true
            };
            CreateHeader();
            _ = LogAvailable.Set();
        }
    }
}