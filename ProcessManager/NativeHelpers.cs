using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.HandleSpecificInfo;
using ProcessManager.InfoClasses.OtherInfo;
using ProcessManager.InfoClasses.PEFileStructures;
using ProcessManager.InfoClasses.PEInfo;
using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.InfoClasses.TokensInfo;
using ProcessManager.InfoClasses.WindowsInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.WMI;
using ProcessManager.Watchdog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ProcessManager.NativeMethods;
using Microsoft.Toolkit.Uwp.Notifications;
using ProcessManager.InfoClasses.SystemParametersInfoStructures;

namespace ProcessManager
{
    /// <summary>
    /// Contiene metodi per l'interazione con la Windows API.
    /// </summary>
    public static class NativeHelpers
    {
        /// <summary>
        /// Delegato per l'enumerazione delle finestre di un processo.
        /// </summary>
        private static readonly EnumWindowsCallback WindowsEnumerationCallback = new(GetWindowHandle);

        /// <summary>
        /// Delegato per l'enumerazione delle finestre figlie di una finestra.
        /// </summary>
        private static readonly EnumWindowsCallback ChildWindowsEnumerationCallback = new(GetChildWindowHandle);

        /// <summary>
        /// Delegato per l'enumerazione delle proprietà di una finestra.
        /// </summary>
        private static EnumWindowPropsCallback WindowPropsEnumerationCallback;

        /// <summary>
        /// Delegato per l'elaborazione di un evento relativo alle finestre.
        /// </summary>
        private static WindowEventCallback WindowEventProcessingCallback;

        /// <summary>
        /// Delegato per l'elaborazione di un evento relativo al cambio della finestra in primo piano.
        /// </summary>
        private static WindowEventCallbackForeground ForegroundWindowChangeCallback;

        /// <summary>
        /// Lista di handle di finestre.
        /// </summary>
        private static readonly List<IntPtr> WindowHandles = new();

        /// <summary>
        /// Lista di proprietà di una finestra.
        /// </summary>
        public static readonly List<WindowProperty> WindowProperties = new();

        /// <summary>
        /// Delegato per l'evento di creazione di una finestra da parte di un processo.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        public delegate void NewWindowCreated(IntPtr WindowHandle);

        /// <summary>
        /// Delegato per l'evento di eliminazione di una finestra da parte di un processo.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        public delegate void WindowDestroyed(IntPtr WindowHandle);

        /// <summary>
        /// Delegato per l'evento di cambiamento della finestra in primo piano.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        public delegate void ForegroundWindowChanged(IntPtr WindowHandle);

        /// <summary>
        /// Creazione di una nuova finestra da parte di un processo.
        /// </summary>
        public static event NewWindowCreated WindowCreatedEvent;

        /// <summary>
        /// Distruzione di una finestra da parte di un processo.
        /// </summary>
        public static event WindowDestroyed WindowDestroyedEvent;

        /// <summary>
        /// Oggetto di sincronizzazione.
        /// </summary>
        private static readonly object LockObject = new();
        #region Process Start Methods
        /// <summary>
        /// Crea un nuovo processo.
        /// </summary>
        /// <param name="FilePath">Percorso dell'eseguibile.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool StartProcess(string FilePath)
        {
            Win32Structures.STARTUPINFO StartupInfo = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.STARTUPINFO)),
                Flags = 0
            };
            if (Win32ProcessFunctions.CreateProcess(FilePath, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, false, Win32Enumerations.ProcessCreationOptions.NONE, IntPtr.Zero, null, StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation))
            {
                LogEntry Entry = BuildLogEntryForInformation("Processo avviato", EventAction.ProcessStart, new SafeProcessHandle(ProcessInformation.ProcessHandle, false));
                Logger.WriteEntry(Entry);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ProcessHandle);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ThreadHandle);
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un processo", EventAction.ProcessStart, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Avvia un processo con una linea di comando specificata.
        /// </summary>
        /// <param name="FilePath">Percorso del processo.</param>
        /// <param name="CommandLine">Linea di comando.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool StartProcess(string FilePath, string CommandLine)
        {
            Win32Structures.STARTUPINFO StartupInfo = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.STARTUPINFO)),
                Flags = 0
            };
            IntPtr StringPointer = Marshal.StringToHGlobalUni(CommandLine);
            if (Win32ProcessFunctions.CreateProcess(FilePath, StringPointer, IntPtr.Zero, IntPtr.Zero, false, Win32Enumerations.ProcessCreationOptions.NONE, IntPtr.Zero, null, StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation))
            {
                Marshal.FreeHGlobal(StringPointer);
                LogEntry Entry = BuildLogEntryForInformation("Processo avviato", EventAction.ProcessStart, new SafeProcessHandle(ProcessInformation.ProcessHandle, false));
                Logger.WriteEntry(Entry);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ProcessHandle);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ThreadHandle);
                return true;
            }
            else
            {
                Marshal.FreeHGlobal(StringPointer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un processo", EventAction.ProcessStart, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Crea un nuovo processo nel contesto dell'utente indicato.
        /// </summary>
        /// <param name="Username">Nome utente.</param>
        /// <param name="Password">Password dell'utente.</param>
        /// <param name="FilePath">Percorso dell'eseguibile.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool StartProcessAsUser(string Username, SecureString Password, string FilePath)
        {
            Win32Structures.STARTUPINFO StartupInfo = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.STARTUPINFO)),
                Flags = 0
            };
            IntPtr PasswordBuffer = Marshal.SecureStringToGlobalAllocUnicode(Password);
            if (Win32ProcessFunctions.CreateProcessWithLogon(Username, Environment.MachineName, PasswordBuffer, 0, FilePath, IntPtr.Zero, Win32Enumerations.ProcessCreationOptions.NONE, IntPtr.Zero, null, StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation))
            {
                Marshal.ZeroFreeGlobalAllocUnicode(PasswordBuffer);
                LogEntry Entry = BuildLogEntryForInformation("Processo avviato", EventAction.ProcessStart, new SafeProcessHandle(ProcessInformation.ProcessHandle, false));
                Logger.WriteEntry(Entry);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ProcessHandle);
                _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ThreadHandle);
                return true;
            }
            else
            {
                Marshal.ZeroFreeGlobalAllocUnicode(PasswordBuffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un processo", EventAction.ProcessStart, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Crea un nuovo processo limitato.
        /// </summary>
        /// <param name="Limit">Limite di utilizzo della CPU da associare al processo.</param>
        /// <param name="FilePath">Percorso dell'eseguibile.</param>
        /// <param name="TerminationError">Indica se il processo è stato terminato a causa di un errore.</param>
        /// <param name="ThreadResumeError">Indica se non è stato possibile riprendere l'esecuzione del processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool StartLimitedProcess(CpuUsageLimitsData Limit, string FilePath, out bool? TerminationError, out bool? ThreadResumeError)
        {
            Win32Structures.STARTUPINFO StartupInfo = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.STARTUPINFO)),
                Flags = 0
            };
            if (Win32ProcessFunctions.CreateProcess(FilePath, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, false, Win32Enumerations.ProcessCreationOptions.CREATE_BREAKAWAY_FROM_JOB | Win32Enumerations.ProcessCreationOptions.CREATE_SUSPENDED, IntPtr.Zero, null, StartupInfo, out Win32Structures.PROCESS_INFORMATION ProcessInformation))
            {
                TerminationError = null;
                ThreadResumeError = null;
                using SafeProcessHandle SafeHandle = new(ProcessInformation.ProcessHandle, true);
                if (!Limit.AddProcess(SafeHandle))
                {
                    TerminationError = !TerminateProcess(SafeHandle);
                    ThreadResumeError = false;
                }
                else
                {
                    if (!ResumeThread(SafeHandle, ProcessInformation.ThreadHandle))
                    {
                        ThreadResumeError = true;
                        TerminationError = !TerminateProcess(SafeHandle);
                    }
                }
                if (TerminationError is not null && ThreadResumeError is not null)
                {
                    _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ThreadHandle);
                    LogEntry? Entry = null;
                    if (TerminationError.Value && !ThreadResumeError.Value)
                    {
                        Entry = BuildLogEntryForWarning("Processo avviato ma non è stato possibile limitarlo, il processo è ancora in esecuzione", EventAction.ProcessStart, SafeHandle);
                    }
                    else
                    {
                        if (ThreadResumeError.Value)
                        {
                            if (TerminationError.Value)
                            {
                                Entry = BuildLogEntryForWarning("Processo limitato avviato ma non è stato possibile riprendere la sua esecuzione, il processo è sospeso", EventAction.ProcessStart, SafeHandle);
                            }
                            else
                            {
                                Entry = BuildLogEntryForWarning("Processo limitato avviato ma non è stato possibile riprendere la sua esecuzione, il processo è stato terminato", EventAction.ProcessStart, SafeHandle);
                            }
                        }
                    }
                    Logger.WriteEntry(Entry.Value);
                    return false;
                }
                else
                {
                    _ = Win32OtherFunctions.CloseHandle(ProcessInformation.ThreadHandle);
                    LogEntry Entry = BuildLogEntryForInformation("Processo limitato avviato", EventAction.ProcessStart, SafeHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
            }
            else
            {
                TerminationError = null;
                ThreadResumeError = null;
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un processo limitato", EventAction.ProcessStart, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Process Termination Methods
        /// <summary>
        /// Termina un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false in caso di fallimento oppure se l'handle si riferisce a un processo di sistema o al processo corrente.</returns>
        public static bool TerminateProcess(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                //Controlla se l'handle fornito si riferisce al processo corrente.
                if (Win32OtherFunctions.CompareObjectHandles(Win32OtherFunctions.GetCurrentProcess(), Handle.DangerousGetHandle()))
                {
                    //Se l'handle si riferisce al processo corrente non viene eseguita alcuna azione e l'operazione viene considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un processo, azioni sul processo corrente non sono permesse", EventAction.ProcessTermination, null);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    //Controlla se l'handle si riferisce a un processo di sistema.
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non ha successo l'evento viene messo a log.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ProcessTermination, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        if (IsCritical)
                        {
                            //Se l'handle si riferisce a un processo di sistema non viene eseguita alcuna azione e l'operazione viene considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un processo, azioni su processi di sistema non sono permesse", EventAction.ProcessTermination, Handle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    //Recupera il codice di uscita del processo per determinare se il processo è ancora in esecuzione.
                    if (Win32ProcessFunctions.GetExitCodeProcess(Handle.DangerousGetHandle(), out uint ExitCode))
                    {
                        //Se il processo è in esecuzione lo termina.
                        if (ExitCode == Win32Constants.STILL_ACTIVE)
                        {
                            ExitCode = uint.MaxValue;
                            if (Win32ProcessFunctions.TerminateProcess(Handle.DangerousGetHandle(), ExitCode))
                            {
                                return true;
                            }
                            else
                            {
                                //Se l'operazione non ha successo l'evento viene messo a log.
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile terminare un processo", EventAction.ProcessTermination, Handle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                        else
                        {
                            //Se il processo è già terminato l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un processo, il processo è già terminato", EventAction.ProcessTermination, null);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        //Se l'operazione non ha successo l'evento viene messo a log.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il codice di uscita di un processo", EventAction.ProcessTermination, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un processo, handle al processo non valido", EventAction.ProcessTermination, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Termina il processo a cui l'handle fornito fa riferimento e tutti i suoi figli.
        /// </summary>
        /// <param name="ParentProcessHandle">Handle al processo padre.</param>
        /// <param name="ParentProcessID">ID del processo padre.</param>
        /// <returns>true se l'operazione è riuscita, false in caso di fallimento oppure se l'handle si riferisce a un processo di sistema.</returns>
        public static bool TerminateProcessTree(SafeProcessHandle ParentProcessHandle, uint ParentProcessID)
        {
            if (ParentProcessHandle != null && !ParentProcessHandle.IsInvalid)
            {
                //Controlla se l'handle fornito si riferisce al processo corrente.
                if (Win32OtherFunctions.CompareObjectHandles(Win32OtherFunctions.GetCurrentProcess(), ParentProcessHandle.DangerousGetHandle()))
                {
                    //Se l'handle si riferisce al processo corrente non viene eseguita alcuna azione e l'operazione viene considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, azioni sul processo corrente non sono permesse", EventAction.ProcessTreeTermination);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    //Controlla se l'handle si riferisce a un processo di sistema.
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(ParentProcessHandle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non ha successo l'evento viene messo a log.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ProcessTreeTermination, ParentProcessHandle, ex.NativeErrorCode, ex.Message, ParentProcessID);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se l'handle si riferisce a un processo di sistema, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, azioni su processi di sistema non sono permesse", EventAction.ProcessTreeTermination, ParentProcessHandle, ParentProcessID);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    //Indica se tutti i processi figlio sono stati terminati e rappresenta il valore restituito dal metodo.
                    bool AllChildrenTerminated = true;
                    //Crea uno snapshot per l'enumerazione dei processi figli.
                    IntPtr SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPPROCESS, 0);
                    if (SnapshotHandle != IntPtr.Zero)
                    {
                        //Determina la data e l'ora di avvio del processo.
                        DateTime? ParentProcessStartTime = GetProcessStartTime(ParentProcessHandle);
                        if (ParentProcessStartTime.HasValue)
                        {
                            //Lista di handle ai processi figli.
                            List<IntPtr> ChildProcessHandles = new();
                            //Enumera i processi figli.
                            List<uint> ChildProcesses = FindProcessChildren(ParentProcessStartTime.Value, ParentProcessID, SnapshotHandle);
                            if (ChildProcesses == null)
                            {
                                //Se l'enumerazione non ha successo, l'operazione è fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, i processi figli non sono stati identificati", EventAction.ProcessTreeTermination, ParentProcessHandle, ParentProcessID);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                            else
                            {
                                List<uint> InnerChildProcesses;
                                IntPtr ChildHandle;
                                DateTime? ChildStartTime;
                                if (ChildProcesses.Count > 0)
                                {
                                    //Per ogni processo figlio vengono enumerati gli eventuali figli.
                                    for (int i = 0; i < ChildProcesses.Count; i++)
                                    {
                                        //Apre un handle al processo con i necessari permessi.
                                        //PROCESS_TERMINATE per terminare il processo.
                                        //PROCESS_QUERY_LIMITED_INFORMATION per determinare la data e l'ora di avvio.
                                        ChildHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_TERMINATE | Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ChildProcesses[i]);
                                        if (ChildHandle != IntPtr.Zero)
                                        {
                                            //Determina la data e l'ora di avvio.
                                            ChildStartTime = GetProcessStartTime(ChildHandle);
                                            if (!ChildStartTime.HasValue)
                                            {
                                                //Se non è possibile determinare la data e l'ora di avvio, l'operazione è fallita.
                                                SafeProcessHandle ChildHandleSafeProcess = new(ChildHandle, false);
                                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile enumerare i figli di un processo, non è stato possibile determinare la data di avvio del processo figlio", EventAction.ProcessTreeTermination, ChildHandleSafeProcess, ChildProcesses[i]);
                                                Logger.WriteEntry(Entry);
                                                ChildHandleSafeProcess.Dispose();
                                                Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                                if (ChildProcessHandles.Count > 0)
                                                {
                                                    foreach (IntPtr Handle in ChildProcessHandles)
                                                    {
                                                        Win32OtherFunctions.CloseHandle(Handle);
                                                    }
                                                }
                                                return false;
                                            }
                                            else
                                            {
                                                //Enumera i processi figli.
                                                InnerChildProcesses = FindProcessChildren(ChildStartTime.Value, ChildProcesses[i], SnapshotHandle);
                                                if (InnerChildProcesses == null)
                                                {
                                                    //Se l'enumerazione non ha successo, l'operazione è fallita.
                                                    SafeProcessHandle ChildHandleSafeProcess = new(ChildHandle, false);
                                                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, i processi figli non sono stati identificati", EventAction.ProcessTreeTermination, ChildHandleSafeProcess, ChildProcesses[i]);
                                                    Logger.WriteEntry(Entry);
                                                    ChildHandleSafeProcess.Dispose();
                                                    Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                                    if (ChildProcessHandles.Count > 0)
                                                    {
                                                        foreach (IntPtr Handle in ChildProcessHandles)
                                                        {
                                                            Win32OtherFunctions.CloseHandle(Handle);
                                                        }
                                                    }
                                                    return false;
                                                }
                                                else
                                                {
                                                    //Aggiunge l'handle alla lista, se il processo figlio ha avviato, a sua volta, altri processi verrà eseguita l'enumerazione dei figli anche di questi.
                                                    ChildProcessHandles.Add(ChildHandle);
                                                    if (InnerChildProcesses.Count > 0)
                                                    {
                                                        ChildProcesses.AddRange(InnerChildProcesses);
                                                        InnerChildProcesses.Clear();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Se l'enumerazione è fallita l'handle allo snapshot viene chiuso insieme agli handle ai processi figli già recuperati.
                                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un processo", EventAction.ProcessTreeTermination, null, ex.NativeErrorCode, ex.Message);
                                            Logger.WriteEntry(Entry);
                                            Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                            if (ChildProcessHandles.Count > 0)
                                            {
                                                foreach (IntPtr Handle in ChildProcessHandles)
                                                {
                                                    Win32OtherFunctions.CloseHandle(Handle);
                                                }
                                            }
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    //Se il processo non ha figli, l'handle allo snapshot viene chiuso e viene eseguito un tentativo di chiusura del processo padre.
                                    Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                    if (TerminateProcess(ParentProcessHandle))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        //Se la chiusura del processo non è riuscita, l'evento viene messo a log.
                                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile terminare un processo", EventAction.ProcessTreeTermination, ParentProcessHandle, ex.NativeErrorCode, ex.Message, ParentProcessID);
                                        Logger.WriteEntry(Entry);
                                        return false;
                                    }
                                }
                                Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                //Tenta la chiusura del processo padre e di tutti i processi figli individuati.
                                if (TerminateProcess(ParentProcessHandle))
                                {
                                    SafeProcessHandle SafeHandle;
                                    foreach (IntPtr Handle in ChildProcessHandles)
                                    {
                                        using (SafeHandle = new SafeProcessHandle(Handle, true))
                                        {
                                            if (!TerminateProcess(SafeHandle))
                                            {
                                                //Se la chiusura di un processo figlio non ha successo, la variabile che indica il successo viene impostata su false e viene messo a log l'evento.
                                                //La chiusura dei processi figli continua anche in caso di errore.
                                                AllChildrenTerminated = false;
                                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile terminare un processo", EventAction.ProcessTreeTermination, SafeHandle, ex.NativeErrorCode, ex.Message);
                                                Logger.WriteEntry(Entry);
                                            }
                                        }
                                    }
                                    return AllChildrenTerminated;
                                }
                                else
                                {
                                    //Se la chiusura del processo padre non ha successo, l'operazione è considerata fallita e viene messo a log l'evento.
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile terminare un processo", EventAction.ProcessTreeTermination, ParentProcessHandle, ex.NativeErrorCode, ex.Message, ParentProcessID);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //La data e ora di avvio del processo padre è necessaria per l'esecuzione del metodo, se non è stato possibile recuperarla l'operazione è fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, non è stato possibile determinare la data e ora di avvio di un processo figlio", EventAction.ProcessTreeTermination, ParentProcessHandle, ParentProcessID);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        //Se non è stato possibile eseguire uno snapshot del sistema, non è possibile continuare l'esecuzione del metodo, quindi l'operazione è fallita.
                        //L'evento viene messo a log.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare uno snapshot del sistema", EventAction.ProcessTreeTermination, ParentProcessHandle, ex.NativeErrorCode, ex.Message, ParentProcessID);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un albero di processi, handle al processo padre non valido", EventAction.ProcessTreeTermination, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Process Debugging Methods
        /// <summary>
        /// Aggancia un debugger a un processo in esecuzione.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AttachDebuggerToProcess(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                //Determina se l'handle si riferisce al processo corrente.
                if (!Win32OtherFunctions.CompareObjectHandles(Handle.DangerousGetHandle(), Win32OtherFunctions.GetCurrentProcess()))
                {
                    if (Settings.SafeMode)
                    {
                        //Determina se l'handle si riferisce a un processo di sistema.
                        if (Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            if (IsCritical)
                            {
                                //Se l'handle si riferisce a un processo di sistema non viene eseguita alcuna azione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile agganciare un debugger a un processo, azioni su processi di sistema non sono permesse", EventAction.DebugProcess, Handle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                        else
                        {
                            //Se il controllo non ha successo, l'evento viene messo a log, l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.DebugProcess, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    //Determina se il processo è già in corso di debug.
                    bool? IsDebugged = IsProcessBeingDebugged(Handle);
                    if (IsDebugged.HasValue)
                    {
                        if (IsDebugged.Value)
                        {
                            //Se il processo è in corso di debug, l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile agganciare un debugger a un processo, il processo ha già un debugger agganciato", EventAction.DebugProcess, Handle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            //Causa un eccezione breakpoint nel processo così che il debugger sia avviato per gestirla.
                            if (Win32ProcessFunctions.DebugBreakProcess(Handle.DangerousGetHandle()))
                            {
                                LogEntry Entry = BuildLogEntryForInformation("Debugger agganciato a un processo", EventAction.DebugProcess, Handle);
                                Logger.WriteEntry(Entry);
                                return true;
                            }
                            else
                            {
                                //Se l'operazione no è riuscita, l'evento viene messo a log.
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile agganciare un debugger a un processo", EventAction.DebugProcess, Handle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //Se non è stato possibile determinare se il processo è in corso di debug, l'operazione è considerata fallita.
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile agganciare un debugger a un processo, non è stato possibile determinare se il processo è già in corso di debug", EventAction.DebugProcess, Handle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    //Se l'handle si riferisce al processo corrente non viene eseguita alcuna azione e l'operazione viene considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile agganciare un debugger a un processo, azioni sul processo corrente non sono permesse", EventAction.DebugProcess, null);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile collegare un debugger a un processo, handle al processo non valido", EventAction.DebugProcess, null);
                Logger.WriteEntry(Entry);
                return false;
            }
            
        }

        /// <summary>
        /// Interrompe il debugging di un processo in esecuzione.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool DetachDebuggerFromProcess(SafeProcessHandle Handle, uint PID = 0)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                //Recupera l'ID del processo.
                if (PID == 0)
                {
                    //Se non è stato possibile recuperare l'ID del processo, non è possibile interrompere il debug, l'operazione è fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, PID non disponibile", EventAction.StopDebugProcess, Handle);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else if (PID != Win32ProcessFunctions.GetCurrentProcessId())
                {
                    if (Settings.SafeMode)
                    {
                        //Determina se l'handle si riferisce a un processo di sistema.
                        if (Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            if (IsCritical)
                            {
                                //Se l'handle si riferisce a un processo di sistema non viene eseguita alcuna azione e l'operazione è cosiderata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, azioni su processi di sistema non sono permesse", EventAction.StopDebugProcess, Handle, PID);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                        else
                        {
                            //Se il controllo non ha successo viene messo a log l'evento e l'operazione è fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.StopDebugProcess, Handle, ex.NativeErrorCode, ex.Message, PID);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    //Determina se il processo è in corso di debug.
                    bool? IsDebugged = IsProcessBeingDebugged(Handle, PID);
                    if (IsDebugged.HasValue)
                    {
                        if (IsDebugged.Value)
                        {
                            if (Win32ProcessFunctions.DebugActiveProcessStop(PID))
                            {
                                LogEntry Entry = BuildLogEntryForInformation("Interrotto debugging di un processo", EventAction.StopDebugProcess, Handle, PID);
                                Logger.WriteEntry(Entry);
                                return true;
                            }
                            else
                            {
                                //Se non è stato possibile imterrompere il debug l'evento viene messo a log e l'operazione è fallita.
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile interrompere il debug di un processo", EventAction.StopDebugProcess, Handle, ex.NativeErrorCode, ex.Message, PID);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                        else
                        {
                            //Se il processo non è in corso di debug non viene eseguita nessuna azione e l'operazione è cosiderata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, il processo non è in corso di debug", EventAction.StopDebugProcess, Handle, PID);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        //Se il controllo non ha successo, l'operazione è considerata fallita.
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, non è stato possibile determinare se il processo è già in corso di debug", EventAction.StopDebugProcess, Handle, PID);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    //Se l'ID del processo è lo stesso di quello corrente non viene eseguita alcuna azione e l'operazione è considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, azioni sul processo corrente non sono permesse", EventAction.StopDebugProcess);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile interrompere il debug di un processo, handle al processo non valido", EventAction.StopDebugProcess, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Determina se un processo è in corso di debug.
        /// </summary>
        /// <param name="Handle">Handle al processo</param>
        /// <returns>true se il processo è in corso di debug, false altrimenti, il valore di ritorno è nullo in caso di errore durante il controllo.</returns>
        public static bool? IsProcessBeingDebugged(SafeProcessHandle Handle, uint PID = 0)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                //Recupera l'ID del processo.
                if (PID == 0)
                {
                    //Se non è stato possibile recuperare l'ID del processo l'operazione è considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile determinare se il processo è in corso di debug, PID non disponibile", EventAction.DebugCheck, Handle, PID);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else if (PID != Win32ProcessFunctions.GetCurrentProcessId())
                {
                    //Determina se il processo esterno è in corso di debug.
                    if (Win32ProcessFunctions.CheckRemoteDebuggerPresent(Handle.DangerousGetHandle(), out bool DebuggerPresent))
                    {
                        return DebuggerPresent;
                    }
                    else
                    {
                        //Se il controllo non ha successo, viene messo a log l'errore e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è in corso di debug", EventAction.DebugCheck, Handle, ex.NativeErrorCode, ex.Message, PID);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    //Determina se il processo corrente è in corso di debug.
                    return Win32ProcessFunctions.IsDebuggerPresent();
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile determinare se il processo è in corso di debug, handle non valido", EventAction.DebugCheck, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Process Windows Info Getters
        /// <summary>
        /// Recupera informazioni sulle finestre di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array di istanze di <see cref="WindowInfo"/> con le informazioni, null in caso di errore.</returns>
        public static WindowInfo[] GetProcessWindowsInfo(SafeProcessHandle Handle, uint PID)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                List<WindowInfo> WindowsInfo = new();
                if (PID == 0)
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sulle finestre aperte da un processo, ID del processo non disponibile", EventAction.WindowsInfoGeneral, Handle);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                else
                {
                    //Recupera la lista di handle ai thread del processo, questi handle non vengono conservati.
                    List<IntPtr> ProcessThreadHandles = EnumerateProcessThreads(PID);
                    uint TID;
                    //Per ogni thread viene eseguita l'enumerazione delle finestre ad esso associate, se presenti.
                    foreach (IntPtr threadhandle in ProcessThreadHandles)
                    {
                        //Recupera l'ID del thread.
                        TID = Win32ProcessFunctions.GetThreadID(threadhandle);
                        if (TID != 0)
                        {
                            //Enumera le finestre associate.
                            EnumThreadWindows(TID);
                            if (WindowHandles.Count > 0)
                            {
                                //Ogni finestra trovata viene aggiunta alla lista.
                                foreach (IntPtr windowhandle in WindowHandles)
                                {
                                    WindowsInfo.Add(GetWindowInfo(windowhandle, Handle, PID));
                                }
                            }
                            WindowHandles.Clear();
                        }
                        else
                        {
                            //Se non è stato possibile recupera l'ID del thread, l'evento viene messo a log e il ciclo continua.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'ID di un thread", EventAction.WindowsInfoGeneral, Handle, ex.NativeErrorCode, ex.Message, PID);
                            Logger.WriteEntry(Entry);
                            continue;
                        }
                        _ = CloseHandle(threadhandle);
                    }
                }
                return WindowsInfo.ToArray();
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sulle finestre aperte da un processo, handle al processo non valido", EventAction.WindowsInfoGeneral, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Enumera le finestre associate a un thread.
        /// </summary>
        /// <param name="TID">ID del thread.</param>
        /// <returns>Una lista di handle alle finestre del thread.</returns>
        private static void EnumThreadWindows(uint TID)
        {
            //Questo metodo richiama la funzione EnumThreadWindows per eseguire l'enumerazione delle finestre associate a un thread.
            //La funzione richiama il metodo GetWindowHandle a cui fa riferimento il callback ad essa fornito.
            if (!Win32ProcessFunctions.EnumThreadWindows(TID, WindowsEnumerationCallback, IntPtr.Zero))
            {
                //Se l'enumerazione non riesce per qualunque motivo, l'evento viene messo a log.
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile enumerare le finestre associate a un thread", EventAction.ThreadWindowsEnum, null);
                Logger.WriteEntry(Entry);
            }
        }

        /// <summary>
        /// Recupera l'handle alla finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle alla finestra fornito dalla funzione <see cref="Win32ProcessFunctions.EnumThreadWindows(uint, EnumWindowsCallback, IntPtr)"/>.</param>
        /// <param name="Parameter">Parametro necessario al funzionamento del metodo, ha sempre <see cref="IntPtr.Zero"/> come valore.</param>
        /// <returns>Questo metodo ritorna sempre true.</returns>
        private static bool GetWindowHandle(IntPtr WindowHandle, IntPtr Parameter)
        {
            //Aggiunge l'handle alla lista.
            WindowHandles.Add(WindowHandle);
            //Enumera le finestre figlie della finestra a cui l'handle fa riferimento.
            //La funzione richiama il metodo GetChildWindowHandle a cui il callback fa riferimento.
            _ = Win32ProcessFunctions.EnumChildWindows(WindowHandle, ChildWindowsEnumerationCallback, IntPtr.Zero);
            return true;
        }

        /// <summary>
        /// Recupera l'handle di una finestra figlia.
        /// </summary>
        /// <param name="WindowHandle">Handle alla finestra fornito dalla funzione <see cref="Win32ProcessFunctions.EnumChildWindows(IntPtr, EnumWindowsCallback, IntPtr)(uint, EnumWindowsCallback, IntPtr)"/>.</param>
        /// <param name="Parameter">Parametro necessario al funzionamento del metodo, ha sempre <see cref="IntPtr.Zero"/> come valore.</param>
        /// <returns>Questo metodo ritorna sempre true.</returns>
        private static bool GetChildWindowHandle(IntPtr WindowHandle, IntPtr Parameter)
        {
            //Aggiunge l'handle alla lista.
            WindowHandles.Add(WindowHandle);
            return true;
        }

        /// <summary>
        /// Recupera informazioni su una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>Un istanza di <see cref="WindowInfo"/> con le informazioni.</returns>
        public static WindowInfo GetWindowInfo(IntPtr WindowHandle, SafeProcessHandle ProcessHandle, uint PID = 0)
        {
            List<string> Styles = new();
            List<string> ExtendedStyles = new();
            //Recupera il nome della classe a cui appartiene alla finestra.
            string ClassName = GetWindowClassName(WindowHandle) ?? Properties.Resources.UnavailableText;
            string HandleValue = "0x" + WindowHandle.ToString("X");
            //Recupera il titolo della finestra.
            string WindowTitle = GetWindowTitle(WindowHandle) ?? Properties.Resources.UnavailableText;
            //Recupera l'ID del thread a cui è associata la finestra, recupera inoltre il nome del processo e relativo PID a cui il thread appartiene.
            string ThreadProcessIDAndName = GetThreadProcessIDAndName(WindowHandle, ProcessHandle, PID) ?? Properties.Resources.UnavailableText;
            string WindowRectangleFullInfo;
            string WindowClientAreaRectangleFullInfo;
            //Recupera il valore dell'handle all'istanza dell'applicazione.
            string ApplicationInstanceHandleValue = GetWindowSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowsInfoOffsets.GWLP_HINSTANCE, true);
            string MenuHandleValue = "0x" + Win32ProcessFunctions.GetMenu(WindowHandle).ToString("X");
            //Recupera i dati utente associati alla finestra.
            string UserDataValue = GetWindowSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowsInfoOffsets.GWLP_USERDATA, true);
            //Recupera l'indirizzo di memoria dove si trova la funzione principale della finestra.
            string WindowProcedureMemoryAddress = GetWindowSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowsInfoOffsets.GWLP_WNDPROC, true);
            //Recupera il nome del modulo associato alla finestra.
            string WindowAssociatedModuleName = GetWindowAssociatedModuleName(WindowHandle);
            //Recupera l'indirizzo di memoria dove si trova la funzione principale della finestra di dialogo, se esiste.
            string DialogProcedureMemoryAddress = GetWindowSpecificInfo(WindowHandle, Marshal.SizeOf(typeof(IntPtr)), false);
            //Recupera l'ID di un controllo di una finestra di dialogo.
            string DialogControlID = GetDialogControlID(WindowHandle);
            //Determina se la finestra è una finestra Unicode.
            bool IsUnicode = Win32ProcessFunctions.IsWindowUnicode(WindowHandle);
            Win32Structures.WINDOWINFO Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.WINDOWINFO))
            };
            //Recupera diverse informazioni sulla finestra.
            if (Win32ProcessFunctions.GetWindowInfo(WindowHandle, ref Info))
            {
                //Recupera i dati sul rettangolo della finestra.
                //Il metodo recupera le coordinate del rettangolo, le coordinate dell'area client, la dimensione del rettangolo e dell'area client.
                //Tutte le coordinate sono relative allo schermo.
                //In caso di errore una o più delle informazioni recuperate sarà nulla.
                GetWindowRectangleData(WindowHandle, Info, out string RectangleData, out string ClientAreaRectangleData, out string RectangleSize, out string ClientAreaSize);
                if (!string.IsNullOrWhiteSpace(RectangleData) && !string.IsNullOrWhiteSpace(RectangleSize))
                {
                    WindowRectangleFullInfo = RectangleData + " " + RectangleSize;
                }
                else
                {
                    WindowRectangleFullInfo = Properties.Resources.UnavailableText;
                }
                if (!string.IsNullOrWhiteSpace(ClientAreaRectangleData) && !string.IsNullOrWhiteSpace(ClientAreaSize))
                {
                    WindowClientAreaRectangleFullInfo = ClientAreaRectangleData + " " + ClientAreaSize;
                }
                else
                {
                    WindowClientAreaRectangleFullInfo = Properties.Resources.UnavailableText;
                }
                string Style;
                //Recupera gli stili applicati alla finestra
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.WindowStyles)))
                {
                    if (Info.Styles.HasFlag((Win32Enumerations.WindowStyles)value))
                    {
                        Style = Enum.GetName(typeof(Win32Enumerations.WindowStyles), value);
                        if (!Styles.Contains(Style))
                        {
                            Styles.Add(Style);
                        }

                    }
                }
                //Recupera gli stili estesi applicati alla finestra.
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ExtendedWindowStyles)))
                {
                    if (Info.ExtendedStyles.HasFlag((Win32Enumerations.ExtendedWindowStyles)value))
                    {
                        Style = Enum.GetName(typeof(Win32Enumerations.ExtendedWindowStyles), value);
                        if (!ExtendedStyles.Contains(Style))
                        {
                            ExtendedStyles.Add(Style);
                        }
                    }
                }
            }
            else
            {
                //Se non è stato possibile recuperare le informazioni, l'evento viene messo a log e le informazioni vengono indicate come non disponibili.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul rettangolo e sugli stili di una finestra", EventAction.WindowsInfoGeneral, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                Logger.WriteEntry(Entry);
                WindowRectangleFullInfo = Properties.Resources.UnavailableText;
                WindowClientAreaRectangleFullInfo = Properties.Resources.UnavailableText;
            }
            //Questa classe raggruppa le informazioni fino ad ora recuperate sulla finestra eccetto gli stili.
            WindowGeneralInfo GeneralInfo = new(ClassName, HandleValue, ThreadProcessIDAndName, WindowRectangleFullInfo, WindowClientAreaRectangleFullInfo, ApplicationInstanceHandleValue, MenuHandleValue, UserDataValue, IsUnicode, WindowProcedureMemoryAddress, DialogProcedureMemoryAddress, DialogControlID, WindowTitle, WindowAssociatedModuleName);
            //Recupera il valore numerico che rappresenta tutti gli stili applicati alla finestra.
            string StylesValue = GetWindowSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowsInfoOffsets.GWL_STYLE, true);
            //Recupera il valore numerico che rappresenta tutti gli stili estesi applicati alla finestra.
            string ExtendedStylesValue = GetWindowSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowsInfoOffsets.GWL_EXSTYLE, true);
            //Questa classe raggruppa tutte le informazioni sugli stili della finestra recuperate.
            WindowStylesInfo StylesInfo = new(StylesValue, Styles, ExtendedStylesValue, ExtendedStyles);
            //Recupera l'atom della classe a cui appartiene la finestra.
            string WindowClassAtom = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCW_ATOM);
            //Recupera gli stili della classe a cui appartiene la finestra..
            string WindowClassStyles = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCL_STYLE);
            //Recupera il valore dell'handle al modulo che ha registrato la classe.
            string ClassModuleInstanceHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_HMODULE);
            //Recupera il valore dell'handle all'icona associata alla classe.
            string ClassIconHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_HICON);
            //Recupera il valore dell'handle all'icona piccola associata alla classe.
            string ClassSmallIconHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_HICONSM);
            //Recupera il valore dell'handle al cursore associato alla classe.
            string ClassCursorHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_HCURSOR);
            //Recupera il valore dell'handle al pennello di background associato alla classe.
            string ClassBackgroundBrushHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_HBRBACKGROUND);
            //Recupera il nome del menù associato alla classe.
            string ClassMenuNameHandleValue = GetWindowClassSpecificInfo(WindowHandle, (int)Win32Enumerations.WindowClassInfo.GCLP_MENUNAME);
            //Questa classe raggruppa le informazioni recuperate sulla classe a cui appartiene la finestra.
            WindowClassInfo ClassInfo = new(WindowClassAtom, WindowClassStyles, ClassModuleInstanceHandleValue, ClassIconHandleValue, ClassSmallIconHandleValue, ClassCursorHandleValue, ClassBackgroundBrushHandleValue, ClassModuleInstanceHandleValue);
            WindowPropsEnumerationCallback = new EnumWindowPropsCallback(GetPropertyData);
            //Enumera le proprietà della finestra.
            int Result = Win32ProcessFunctions.EnumPropsEx(WindowHandle, WindowPropsEnumerationCallback, IntPtr.Zero);
            if (Result == -1)
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sulle proprietà di una finestra, la finestra non ha proprietà da enumerare", EventAction.WindowsInfoGeneral, ProcessHandle, PID);
                Logger.WriteEntry(Entry);
            }
            //Questa classe riunisce tutte le informazioni recuperate sulla finestra e rappresenta il risultato del metodo.
            return new WindowInfo(WindowHandle, GeneralInfo, StylesInfo, ClassInfo, WindowProperties);
        }
        #region Window Info Getter Methods
        /// <summary>
        /// Recupera una specifica informazione su una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="Info">Informazione da recuperare.</param>
        /// <param name="IsEnumValue">Indica se il valore di <paramref name="Info"/> corrisponde a un valore dell'enumerazione <see cref="Win32Enumerations.WindowsInfoOffsets"/>.</param>
        /// <returns>Una stringa che rappresenta l'informazione richiesta.</returns>
        private static string GetWindowSpecificInfo(IntPtr WindowHandle, int Info, bool IsEnumValue)
        {
            //Recupera l'informazione richiesta sulla finestra.
            IntPtr WindowInfo = Win32ProcessFunctions.GetWindowLongPtr(WindowHandle, Info);
            if (WindowInfo != IntPtr.Zero)
            {
                //Se l'operazione ha successo, l'informazione recuperata viene restituita come numero esadecimale.
                return "0x" + WindowInfo.ToString("X");
            }
            else
            {
                string InfoName;
                //Se l'operazione è fallita e questo metodo è stato chiamato usando un valore dell'enumerazione WindowsInfoOffsets, l'evento viene messo a log indicando anche
                //l'informazione che si è tentato di recuperare.
                if (IsEnumValue)
                {
                    InfoName = Enum.GetName(typeof(Win32Enumerations.WindowsInfoOffsets), (Win32Enumerations.WindowsInfoOffsets)Info);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare un'informazione su una finestra, informazione richiesta: " + InfoName, EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare un'informazione su una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                }
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera un informazione specifica sulla classe di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="Info">Informazione da recuperare.</param>
        /// <returns>Una stringa che rappresenta l'informazione richiesta.</returns>
        private static string GetWindowClassSpecificInfo(IntPtr WindowHandle, int Info)
        {
            //Recupera l'informazione richiesta sulla classe di una finestra.
            IntPtr ClassInfo = Win32ProcessFunctions.GetClassLongPtr(WindowHandle, Info);
            if (ClassInfo != IntPtr.Zero)
            {
                //Se l'operazione ha successo, l'informazione recuperata viene restituita come numero esadecimale.
                return "0x" + ClassInfo.ToString("X");
            }
            else
            {
                //Se l'operazione è fallita, l'evento viene messo a log indicando anche l'informazione che si è tentato di recuperare.
                string InfoName = Enum.GetName(typeof(Win32Enumerations.WindowsInfoOffsets), (Win32Enumerations.WindowClassInfo)Info);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare un'informazione su una finestra, informazione richiesta: " + InfoName, EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera l'ID di un controllo di una finestra di dialogo.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>Una stringa che rappresenta l'ID del controllo.</returns>
        private static string GetDialogControlID(IntPtr WindowHandle)
        {
            //Recupera l'ID del controllo a cui l'handle si riferisce.
            int ID = Win32ProcessFunctions.GetDlgCtrlID(WindowHandle);
            if (ID == 0)
            {
                //Se non è stato possibile recuperare l'ID, l'evento viene messo a log.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'ID di un controllo di una finestra di dialogo", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
            else
            {
                return ID.ToString("N0", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Recupera il nome della classe di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>Il nome della classe, se non è stato possibile recuperare il nome il valore di ritorno è nullo.</returns>
        private static string GetWindowClassName(IntPtr WindowHandle)
        {
            StringBuilder ClassName = new(256);
            //Recupera il nome della classe della finestra a cui l'handle si riferisce.
            if (Win32ProcessFunctions.GetClassName(WindowHandle, ClassName, ClassName.Capacity) == 0)
            {
                //Se la funzione ritorna 0 significa che l'operazione è fallita, l'evento viene messo a log e il valore di ritorno è nullo.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome della classe associata a una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                return ClassName.ToString();
            }
        }

        /// <summary>
        /// Recupera il titolo di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>Il titolo della finestra, se la finestra non ha titolo o l'handle non è valido allora ritorna una stringa vuota.<br/>
        /// In caso di errore il valore di ritorno è nullo.</returns>
        private static string GetWindowTitle(IntPtr WindowHandle)
        {
            //Il valore dell'ultimo codice di errore Win32 viene impostato a 0 in quanto la documentazione di GetWindowTextLength indica che la funzione non
            //esegue questa operazione.
            Win32OtherFunctions.SetLastError(0);
            //Recupera la lunghezza, in caratteri, del titolo della finestra.
            int TextLength = Win32ProcessFunctions.GetWindowTextLength(WindowHandle);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (TextLength == 0 && ErrorCode != 0)
            {
                //Se l'ultimo codice di errore Win32 è diverso da zero è la lunghezza del titolo ha valore 0,
                //significa che l'operazione è fallita, l'evento viene messo a log, il valore di ritorno è nullo.
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la lunghezza del titolo di una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else if (TextLength == 0 && ErrorCode == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder WindowText = new(TextLength + 1);
                //Recupera il titolo della finestra.
                TextLength = Win32ProcessFunctions.GetWindowText(WindowHandle, WindowText, TextLength + 1);
                if (TextLength != 0)
                {
                    return WindowText.ToString();
                }
                else
                {
                    ErrorCode = Marshal.GetLastWin32Error();
                    //Se l'operazione è fallita, l'evento viene messo a log e il valore di ritorno è nullo.
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il titolo di una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
        }

        /// <summary>
        /// Recupera l'ID del thread che ha creato la finestra, oltre al nome e all'ID del processo.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>Una stringa che contiene il nome, il PID e il TID richiesti, il valore di ritorno è nullo in caso di errore.</returns>
        private static string GetThreadProcessIDAndName(IntPtr WindowHandle, SafeProcessHandle ProcessHandle, uint PID = 0)
        {
            bool TemporaryHandle = false;
            //Recupera l'ID del thread e del processo a cui la finestra appartiene.
            uint TID = PID == 0
                ? Win32ProcessFunctions.GetWindowThreadProcessID(WindowHandle, out PID)
                : Win32ProcessFunctions.GetWindowThreadProcessID(WindowHandle, out _);
            //Recupera il percorso completo del processo.
            string ProcessName;
            if (ProcessHandle is null || ProcessHandle.IsInvalid)
            {
                ProcessHandle = GetProcessHandle(PID);
                TemporaryHandle = true;
            }
            ProcessName = GetProcessName(ProcessHandle);
            if (TemporaryHandle)
            {
                ProcessHandle.Dispose();
            }
            if (ProcessName != Properties.Resources.UnavailableText)
            {
                //Il valore di ritorno del metodo è una stringa nel seguente formato:
                //<nome del processo> (<PID>): <TID>
                return ProcessName + " (" + PID.ToString("D0", CultureInfo.InvariantCulture) + "): " + TID.ToString("D0", CultureInfo.InvariantCulture);
            }
            else
            {
                //Se non è stato possibile recupera il nome del processo, il valore di ritorno è l'ID del processo e del thread nel seguente formato:
                //<PID>: <TID>
                return PID.ToString("D0", CultureInfo.InvariantCulture) + ": " + TID.ToString("D0", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Recupera i dati del rettagolo della finestra e del rettangolo della sua area client come stringa.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="Info">Struttura <see cref="Win32Structures.WINDOWINFO"/> che contiene i dati da convertire.</param>
        /// <param name="RectangleData">Dati sul rettangolo della finestra.</param>
        /// <param name="ClientAreaRectangleData">Dati sul rettangolo dell'area client della finestra.</param>
        /// <param name="RectangleSize">Dimensione della finestra.</param>
        /// <param name="ClientAreaSize">Dimensione dell'area client.</param>
        private static void GetWindowRectangleData(IntPtr WindowHandle, Win32Structures.WINDOWINFO Info, out string RectangleData, out string ClientAreaRectangleData, out string RectangleSize, out string ClientAreaSize)
        {
            ClientAreaSize = null;
            //Coordinate del rettangolo della finestra nel seguente formato:
            //(<coordinate dell'angolo superiore sinistro>) - (<coordinate dell'angolo inferiore destro)
            //Le coordinate x e y di entrambi gli angoli sono separate da una virgola.
            RectangleData = "(" + Info.Window.Left.ToString("D0", CultureInfo.InvariantCulture) + ", " + Info.Window.Top.ToString("D0", CultureInfo.InvariantCulture) + ") - (" + Info.Window.Right.ToString("D0", CultureInfo.InvariantCulture) + ", " + Info.Window.Bottom.ToString("D0", CultureInfo.InvariantCulture) + ")";
            int WindowWidth = Info.Window.Right - Info.Window.Left;
            int WindowHeight = Info.Window.Bottom - Info.Window.Top;
            //Dimensione del rettagolo nel seguente formato:
            //[<larghezza della finestra>x<altezza della finestra>]
            RectangleSize = "[" + WindowWidth.ToString("D0", CultureInfo.InvariantCulture) + "x" + WindowHeight.ToString("D0", CultureInfo.InvariantCulture) + "]";
            IntPtr ClientAreaRectangleStructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(Info.ClientArea));
            Marshal.StructureToPtr(Info.ClientArea, ClientAreaRectangleStructurePointer, false);
            //La chiamata a questa funzione è consigliata prima della chiamata alla funzione MapWindowPoints dalla documentazione di quest'ultima.
            Win32OtherFunctions.SetLastError(0);
            //Trasforma le coordinate dell'area client della finestra riferite al rettangolo di quest'ultima in coordinate riferite allo schermo.
            if (Win32OtherFunctions.MapWindowPoints(WindowHandle, IntPtr.Zero, ClientAreaRectangleStructurePointer, 2) != 0)
            {
                Win32Structures.RECT ConvertedPoints = (Win32Structures.RECT)Marshal.PtrToStructure(ClientAreaRectangleStructurePointer, typeof(Win32Structures.RECT));
                Marshal.FreeHGlobal(ClientAreaRectangleStructurePointer);
                //Coordinate dell'area client della finestra nel seguente formato:
                //(<coordinate dell'angolo superiore sinistro>) - (<coordinate dell'angolo inferiore destro)
                //Le coordinate x e y di entrambi gli angoli sono separate da una virgola.
                ClientAreaRectangleData = "(" + ConvertedPoints.Left.ToString("D0", CultureInfo.InvariantCulture) + ", " + ConvertedPoints.Top.ToString("D0", CultureInfo.InvariantCulture) + ") - (" + ConvertedPoints.Right.ToString("D0", CultureInfo.InvariantCulture) + ", " + ConvertedPoints.Bottom.ToString("D0", CultureInfo.InvariantCulture) + ")";
                int ClientAreaWidth = ConvertedPoints.Right - ConvertedPoints.Left;
                int ClientAreaHeight = ConvertedPoints.Bottom - ConvertedPoints.Top;
                //Dimensione dell'area client nel seguente formato:
                //[<larghezza della finestra>x<altezza della finestra>]
                ClientAreaSize = "[" + ClientAreaWidth.ToString("D0", CultureInfo.InvariantCulture) + "x" + ClientAreaHeight.ToString("D0", CultureInfo.InvariantCulture) + "]";
            }
            else
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                if (ErrorCode != 0)
                {
                    //Se l'operazione è fallita, l'evento viene messo a log, i dati relativi all'area client sono nulli.
                    ClientAreaRectangleData = null;
                    Marshal.FreeHGlobal(ClientAreaRectangleStructurePointer);
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sull'area client di una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                }
                else
                {
                    //Se la funzione ha restituito 0 ed è un risultato valido, i dati dell'area client vengono recuperati nello stesso modo.
                    //I dati hanno lo stesso formato indicato precedentemente.
                    Win32Structures.RECT ConvertedPoints = (Win32Structures.RECT)Marshal.PtrToStructure(ClientAreaRectangleStructurePointer, typeof(Win32Structures.RECT));
                    Marshal.FreeHGlobal(ClientAreaRectangleStructurePointer);
                    ClientAreaRectangleData = "(" + ConvertedPoints.Left.ToString("D0", CultureInfo.InvariantCulture) + ", " + ConvertedPoints.Top.ToString("D0", CultureInfo.InvariantCulture) + ") - (" + ConvertedPoints.Right.ToString("D0", CultureInfo.InvariantCulture) + ", " + ConvertedPoints.Bottom.ToString("D0", CultureInfo.InvariantCulture) + ")";
                    int ClientAreaWidth = ConvertedPoints.Right - ConvertedPoints.Left;
                    int ClientAreaHeight = ConvertedPoints.Bottom - ConvertedPoints.Top;
                    ClientAreaSize = "[" + ClientAreaWidth.ToString("D0", CultureInfo.InvariantCulture) + "x" + ClientAreaHeight.ToString("D0", CultureInfo.InvariantCulture) + "]";
                }
            }
        }

        /// <summary>
        /// Recupera il nome del modulo associato a una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>Il nome del modulo, se l'informazione non può essere recuperata il valore di ritorno è nullo.</returns>
        private static string GetWindowAssociatedModuleName(IntPtr WindowHandle)
        {
            StringBuilder ModuleName = new(260);
            uint MaxChars = (uint)ModuleName.Capacity;
            //Recupera il nome del modulo associato alla finestra.
            if (Win32ProcessFunctions.GetWindowModuleFileName(WindowHandle, ModuleName, MaxChars) != 0)
            {
                return Path.GetFileName(ModuleName.ToString());
            }
            else
            {
                //Se non è stato possibile recuperare il nome del modulo l'operazione è fallita, l'evento viene messo a log.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome del modulo associato a una finestra", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera i dati di una proprietà di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="PropertyName">Nome della proprietà.</param>
        /// <param name="DataHandle">Handle ai dati della proprietà.</param>
        /// <param name="Parameter">Parametro fornito dall'applicazione.</param>
        /// <returns>Questo metodo ritorna sempre true.</returns>
        private static bool GetPropertyData(IntPtr WindowHandle, IntPtr PropertyName, IntPtr DataHandle, IntPtr Parameter)
        {
            string PropertyNameString = Marshal.PtrToStringUni(PropertyName);
            if (!string.IsNullOrWhiteSpace(PropertyNameString))
            {
                WindowProperties.Add(new WindowProperty(PropertyNameString, "0x" + DataHandle.ToString("X")));
            }
            else
            {
                WindowProperties.Add(new WindowProperty(Properties.Resources.UnavailableText, "0x" + DataHandle.ToString("X")));
            }
            return true;
        }

        /// <summary>
        /// Determina se una finestra è visibile.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se la finestra è visibile, false altrimenti.</returns>
        public static bool IsWindowVisible(IntPtr WindowHandle)
        {
            return Win32ProcessFunctions.IsWindowVisible(WindowHandle);
        }

        /// <summary>
        /// Determina se una finestra è attiva.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se la finestra è attiva, false altrimenti.</returns>
        public static bool IsWindowEnabled(IntPtr WindowHandle)
        {
            return Win32ProcessFunctions.IsWindowEnabled(WindowHandle);
        }

        /// <summary>
        /// Recupera la percentuale di trasparenza di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>Un intero che rappresenta la percentuale, arrotondata al multiplo di 10 più vicino, di trasparenza della finestra, -1 in caso di errore.</returns>
        public static int GetWindowTransparencyPercentage(IntPtr WindowHandle)
        {
            int Percentage;
            //Recupera il valore di opacità della finestra.
            if (Win32ProcessFunctions.GetLayeredWindowAttributes(WindowHandle, out _, out byte OpacityLevel, out _))
            {
                //Il valore restituito dalla funzione viene trasformato in percentuale e poi arrotondato alla decina più vicina.
                Percentage = 100 * OpacityLevel / 255;
                return ((int)Math.Round(Percentage / 10.0, MidpointRounding.AwayFromZero)) * 10;
            }
            else
            {
                //Se non è stato possibile recuperare l'opacità della finestra l'operazione è fallita, l'evento viene messo a log, il metodo restituisce -1.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare gli attributi di una finestra a strati", EventAction.WindowInfoSpecific, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return -1;
            }
        }
        #endregion
        /// <summary>
        /// Recupera l'ID del processo a cui appartiene la finestra sotto il cursore.
        /// </summary>
        /// <returns>L'ID del processo, nullo in caso di errore.</returns>
        public static uint? GetOwnerProcessIDOfWindowUnderCursor()
        {
            if (Win32OtherFunctions.GetCursorPos(out Win32Structures.POINT Point))
            {
                IntPtr WindowHandle = Win32OtherFunctions.WindowFromPoint(Point);
                if (WindowHandle != IntPtr.Zero)
                {
                    _ = Win32ProcessFunctions.GetWindowThreadProcessID(WindowHandle, out uint PID);
                    return PID;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recupera l'handle alla finestra sotto la posizione attuale del cursore", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recupera la posizione del cursore", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera l'ID del processo a cui appartiene una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>L'ID del processo.</returns>
        public static uint GetWindowOwnerProcessID(IntPtr WindowHandle)
        {
            _ = Win32ProcessFunctions.GetWindowThreadProcessID(WindowHandle, out uint PID);
            return PID;
        }
        #endregion
        #region Process Windows Info Setters
        #region Window Size Manipulation
        /// <summary>
        /// Ripristina lo stato di visibilità della finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.</remarks>
        public static bool RestoreWindow(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (Win32ProcessFunctions.IsIconic(WindowHandle))
                {
                    //Se la finestra è ridotta a icona, viene ripristinata e attivata.
                    if (Win32ProcessFunctions.OpenIcon(WindowHandle))
                    {
                        return true;
                    }
                    else
                    {
                        //Se si verifica un errore durante l'operazione, l'evento viene messo a log e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ripristinare una finestra", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    //Se la finestra non è ridotta a icona ma non è visibile, essa viene mostrata. 
                    Win32ProcessFunctions.ShowWindow(WindowHandle, Win32Enumerations.WindowShowState.SW_SHOW);
                    return true;
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }

        /// <summary>
        /// Riduce a icona una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde o è già ridotta a icona questo metodo non effettua alcuna operazione.</remarks>
        public static bool MinimizeWindow(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (Win32ProcessFunctions.IsIconic(WindowHandle))
                {
                    //Se la finestra è già ridotta a icona non viene eseguita alcuna operazione e l'operazione è considerata riuscita.
                    return true;
                }
                else
                {
                    if (Win32ProcessFunctions.CloseWindow(WindowHandle))
                    {
                        //Se la finestra non è ridotta a icona, essa viene ridotta a icona.
                        return true;
                    }
                    else
                    {
                        //Se si verifica un errore durante l'operazione, l'evento viene messo a log e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ridurre a icona una finestra", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }


        /// <summary>
        /// Ingrandisce una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde o già ingrandita questo metodo non effettua alcuna operazione.</remarks>
        public static bool MaximizeWindow(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (Win32ProcessFunctions.IsZoomed(WindowHandle))
                {
                    //Se la finestra è già ingrandita non viene eseguita alcuna operazione e l'operazione è considerata riuscita.
                    return true;
                }
                else
                {
                    if (Win32ProcessFunctions.IsIconic(WindowHandle))
                    {
                        //Se la finestra è ridotta a icona viene ripristinata alla dimensione precedente.
                        if (Win32ProcessFunctions.OpenIcon(WindowHandle))
                        {
                            if (Win32ProcessFunctions.IsZoomed(WindowHandle))
                            {
                                //Se dopo il ripristino la finestra è già ingrandita, l'operazione è completata.
                                return true;
                            }
                            else
                            {
                                //Se la finestra non è ingrandita essa viene ingrandita.
                                Win32ProcessFunctions.ShowWindow(WindowHandle, Win32Enumerations.WindowShowState.SW_MAXIMIZE);
                                if (Win32ProcessFunctions.IsZoomed(WindowHandle))
                                {
                                    //Se la finestra risulta ingrandita, l'operazione è completata.
                                    return true;
                                }
                                else
                                {
                                    //Se la finestra non risulta ingrandita, l'operazione è fallita.
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ingrandire una finestra", EventAction.WindowOperation, null, null, null);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //Se si verifica un errore durante l'operazione, l'evento viene messo a log e l'operazione è fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ingrandire una finestra", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        //Se la finestra non è ridotta a icona essa viene mostrata, attivata e ingrandita.
                        Win32ProcessFunctions.ShowWindow(WindowHandle, Win32Enumerations.WindowShowState.SW_SHOWMAXIMIZED);
                        if (Win32ProcessFunctions.IsZoomed(WindowHandle))
                        {
                            //Se la finestra risulta ingrandita, l'operazione è completata.
                            return true;
                        }
                        else
                        {
                            //Se la finestra non risulta ingrandita, l'operazione è fallita.
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ingrandire una finestra", EventAction.WindowOperation, null, null, null);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }
        #endregion
        #region Window Visibility Status Manipulation
        /// <summary>
        /// Rende visibile o nasconde una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true, se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.<br/>
        /// La finestra non viene ne attivata ne portata in primo piano.</remarks>
        public static bool ChangeWindowVisibility(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (!Win32ProcessFunctions.IsWindowVisible(WindowHandle))
                {
                    //Se la finestra non è visibile essa viene mostrata ma non viene attivata.
                    Win32ProcessFunctions.ShowWindow(WindowHandle, Win32Enumerations.WindowShowState.SW_SHOWNA);
                    //L'operazione è riuscita quando la funzione IsWindowVisible restituisce true.
                    return Win32ProcessFunctions.IsWindowVisible(WindowHandle);
                }
                else
                {
                    //Se la finestra è visibile essa viene ridotta a icona.
                    //L'operazione è riuscita se il metodo CloseWindow restituisce true.
                    return CloseWindow(WindowHandle);
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }

        /// <summary>
        /// Porta una finestra in primo piano.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.</remarks>
        public static bool BringWindowToFront(IntPtr WindowHandle)
        {
            uint Flags;
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                //Le opzioni per il parametro Flags della funzione SetWindowPos vengono impostate alle seguenti:
                //SWP_NOMOVE = mantenere la posizione della finestra
                //SWP_NOSIZE = mantenere la dimensione della finestra
                //SWP_SHOWWINDOW = mostrare la finestra
                Flags = (uint)(Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOMOVE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOSIZE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_SHOWWINDOW);
                //La funzione SetWindowPos viene chiamata indicando per il parametro PrecedingWindowHandle il valore HWND_TOP (0) che indica il primo posto nell'ordine Z,
                //non vengono indicate informazioni relative alle coordinate degli angoli superiori e inferiori o della larghezza e altezza della finestra.
                if (Win32ProcessFunctions.SetWindowPos(WindowHandle, (IntPtr)Win32Enumerations.PrecedingWindowHandleSpecialValue.HWND_TOP, 0, 0, 0, 0, Flags))
                {
                    return true;
                }
                else
                {
                    //Se il riposizionamento della finestra non è riuscito, l'evento viene messo a log e l'operazione è fallita.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile portare una finestra in primo piano", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }

        /// <summary>
        /// Nasconde una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.</remarks>
        public static bool CloseWindow(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (!Win32ProcessFunctions.IsWindowVisible(WindowHandle))
                {
                    //Se la finestra non è visibile non viene eseguita alcuna operazione e l'operazione è considerata riuscita.
                    return true;
                }
                else
                {
                    //Se la finestra è visibile viene nascosta.
                    Win32ProcessFunctions.ShowWindow(WindowHandle, Win32Enumerations.WindowShowState.SW_HIDE);
                    if (!Win32ProcessFunctions.IsWindowVisible(WindowHandle))
                    {
                        //Se la finestra risulta nascosta, l'operazione è riuscita.
                        return true;
                    }
                    else
                    {
                        //Se la finestra non risulta nascosta, l'operazione è fallita, l'evento viene messo a log.
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile nascondere una finestra", EventAction.WindowOperation, null, null, null);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }

        /// <summary>
        /// Abilita o disabilita una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.</remarks>
        public static bool ChangeWindowEnabledStatus(IntPtr WindowHandle)
        {
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (!Win32ProcessFunctions.IsWindowEnabled(WindowHandle))
                {
                    //Se la finestra è disattivata viene attivata.
                    Win32ProcessFunctions.EnableWindow(WindowHandle, true);
                    if (Win32ProcessFunctions.IsWindowEnabled(WindowHandle))
                    {
                        //Se la finestra risulta abilitata, l'operazione è riuscita.
                        return true;
                    }
                    else
                    {
                        //Se la finestra risulta ancora disattivata, l'operazione è fallita, l'evento viene messo a log.
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile abilitare una finestra", EventAction.WindowOperation, null, null, null);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    //Se la finestra è attiva viene disattivata.
                    Win32ProcessFunctions.EnableWindow(WindowHandle, false);
                    if (!Win32ProcessFunctions.IsWindowEnabled(WindowHandle))
                    {
                        //Se la finestra risulta disattivata, l'operazione è riuscita.
                        return true;
                    }
                    else
                    {
                        //Se la finestra risulta ancora attivata, l'operazione è fallita, l'evento viene messo a log.
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile disabilitare una finestra", EventAction.WindowOperation, null, null, null);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }

        /// <summary>
        /// Cambia lo stato topmost di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="IsTopMost">Indica se la finestra è una finestra topmost.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se la finestra non risponde questo metodo non effettua alcuna operazione.</remarks>
        public static bool ChangeWindowTopMostStatus(IntPtr WindowHandle, bool IsTopMost)
        {
            uint Flags;
            if (!Win32ProcessFunctions.IsHungAppWindow(WindowHandle))
            {
                if (IsTopMost)
                {
                    //Le opzioni per il parametro Flags della funzione SetWindowPos vengono impostate alle seguenti:
                    //SWP_NOMOVE = mantenere la posizione della finestra
                    //SWP_NOSIZE = mantenere la dimensione della finestra
                    //SWP_SHOWWINDOW = mostrare la finestra
                    Flags = (uint)(Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOMOVE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOSIZE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_SHOWWINDOW);
                    //La funzione SetWindowPos viene chiamata indicando per il parametro PrecedingWindowHandle il valore HWND_NOTOPMOST (-2) che indica la posizione sotto tutte le finestre in primo piano non permanente,
                    //non vengono indicate informazioni relative alle coordinate degli angoli superiori e inferiori o della larghezza e altezza della finestra.
                    if (Win32ProcessFunctions.SetWindowPos(WindowHandle, (IntPtr)Win32Enumerations.PrecedingWindowHandleSpecialValue.HWND_NOTOPMOST, 0, 0, 0, 0, Flags))
                    {
                        return true;
                    }
                    else
                    {
                        //Se il riposizionamento della finestra non è riuscito, l'evento viene messo a log e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato topmost", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    //Le opzioni per il parametro Flags della funzione SetWindowPos vengono impostate alle seguenti:
                    //SWP_NOMOVE = mantenere la posizione della finestra
                    //SWP_NOSIZE = mantenere la dimensione della finestra
                    //SWP_SHOWWINDOW = mostrare la finestra
                    Flags = (uint)(Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOMOVE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_NOSIZE | Win32Enumerations.WindowSizingAndPositioningOptions.SWP_SHOWWINDOW);
                    //La funzione SetWindowPos viene chiamata indicando per il parametro PrecedingWindowHandle il valore HWND_TOPMOST (-1) che indica la posizione sopra tutte le finestre in primo piano non permanente,
                    //non vengono indicate informazioni relative alle coordinate degli angoli superiori e inferiori o della larghezza e altezza della finestra.
                    if (Win32ProcessFunctions.SetWindowPos(WindowHandle, (IntPtr)Win32Enumerations.PrecedingWindowHandleSpecialValue.HWND_TOPMOST, 0, 0, 0, 0, Flags))
                    {
                        return true;
                    }
                    else
                    {
                        //Se il riposizionamento della finestra non è riuscito, l'evento viene messo a log e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato topmost", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                //Se la finestra non risponde non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                return false;
            }
        }
        #endregion
        /// <summary>
        /// Imposta la percentuale di trasparenza di una finestra.
        /// </summary>
        /// <param name="WindowHandle">Handle nativo alla finestra.</param>
        /// <param name="Percentage">Nuova percentuale di trasparenza.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetWindowTransparencyPercentage(IntPtr WindowHandle, byte Percentage)
        {
            byte AlphaValue = (byte)(Percentage * 255 / 100);
            //Al parametro Flags della funzione SetLayeredWindowAttributes viene assegnato il valore 2 che indica di utilizza il valore del parametro AlphaValue
            //per determinare l'opacità della finestra.
            if (Win32ProcessFunctions.SetLayeredWindowAttributes(WindowHandle, 0, AlphaValue, Win32Enumerations.LayeredWindowAttributesFlags.LWA_ALPHA))
            {
                return true;
            }
            else
            {
                //Se non è stato possibile cambiare la percentuale di trasparenza della finestra, l'operazione è fallita, l'evento viene messo a log.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare gli attributi di una finestra a strati", EventAction.WindowOperation, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Process Properties Setters
        /// <summary>
        /// Imposta l'affinità del processo.
        /// </summary>
        /// <param name="ProcessSafeHandle">Handle al processo.</param>
        /// <param name="NewAffinity">Nuovo valore di affinità per il processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetProcessAffinity(SafeProcessHandle ProcessSafeHandle, ulong NewAffinity)
        {
            if (ProcessSafeHandle != null && !ProcessSafeHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessSafeHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare l'affinità di un processo, azione su processi di sistema non sono permesse", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessSafeHandle.DangerousGetHandle()))
                {
                    //Se l'handle si riferisce al processo corrente, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare l'affinità di un processo, azione sul processo corrente non sono permesse", EventAction.ProcessPropertiesManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    //Il valore della nuova maschera di affinità del processo viene trasformata in un'istanza della struttura IntPtr
                    //cosi che possa essere data come parametro alla funzione SetProcessAffinityMask.
                    IntPtr ProcessAffinity = (IntPtr)NewAffinity;
                    if (Win32ProcessFunctions.SetProcessAffinityMask(ProcessSafeHandle.DangerousGetHandle(), ProcessAffinity))
                    {
                        //Se l'impostazione della maschera di affinità è riuscita, l'operazione è completata.
                        LogEntry Entry = BuildLogEntryForInformation("Affinità del processo impostata, valore affinità: " + NewAffinity.ToString("N0", CultureInfo.CurrentCulture), EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                        Logger.WriteEntry(Entry);
                        return true;
                    }
                    else
                    {
                        //Se l'impostazione della maschera di affinità è fallita, l'evento viene messo a log e l'operazione è fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare l'affinità di un processo", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare l'affinità di un processo, handle non valido", EventAction.ProcessPropertiesManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Imposta la priorità di un processo.
        /// </summary>
        /// <param name="ProcessSafeHandle">Handle al processo.</param>
        /// <param name="ProcessPriority">Priorità del processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetProcessPriority(SafeProcessHandle ProcessSafeHandle, ProcessInfo.ProcessPriority ProcessPriority)
        {
            if (ProcessSafeHandle != null && !ProcessSafeHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessSafeHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un processo, azione su processi di sistema non sono permesse", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessSafeHandle.DangerousGetHandle()))
                {
                    //Se l'handle si riferisce al processo corrente, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un processo, azione sul processo corrente non sono permesse", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    if (ProcessPriority == ProcessInfo.ProcessPriority.Unknown)
                    {
                        //Se il nuovo valore della priorità del processo non è valido, l'operazione è fallita.
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un processo, nuovo valore di priorità non valido", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (Win32ProcessFunctions.SetPriorityClass(ProcessSafeHandle.DangerousGetHandle(), (uint)ProcessPriority))
                        {
                            //Se l'impostazione della priorità è riuscita, l'operazione è completata.
                            LogEntry Entry = BuildLogEntryForInformation("Priorità del processo impostata, nuova priorità: " + ProcessPriority.ToString("g"), EventAction.ProcessPropertiesManipulation, ProcessSafeHandle);
                            Logger.WriteEntry(Entry);
                            return true;
                        }
                        else
                        {
                            //Se l'impostazione della priorità è fallita, l'operazione è fallita e l'evento viene messo a log.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un processo", EventAction.ProcessPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un processo, handle non valido", EventAction.ProcessPropertiesManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Process Properties Getters
        #region Main Properties
        /// <summary>
        /// Recupera la priorità di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Uno dei valori di <see cref="ProcessInfo.ProcessPriority"/>, 0 in caso di errore.</returns>
        public static uint GetProcessPriority(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint Priority = Win32ProcessFunctions.GetPriorityClass(Handle.DangerousGetHandle());
                if (Priority != 0)
                {
                    //Se la funzione ha restituito un valore valido, l'operazione è riuscita.
                    return Priority;
                }
                else
                {
                    //Se non è stato possibile recuperare la priorità del processo, l'operazione è fallita, l'evento viene messo a log.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la priorità di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la priorità di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }

        /// <summary>
        /// Recupera il PID del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="DebugLoggingCall">Indica se metodo è stato chiamato durante il log di debug.</param>
        /// <returns>Il PID del processo, 0 in caso di errore.</returns>
        public static uint GetProcessPID(SafeProcessHandle Handle, bool DebugLoggingCall = false)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint PID = Win32ProcessFunctions.GetProcessID(Handle.DangerousGetHandle());
                if (PID != 0)
                {
                    //Se il valore del PID recuperato è valido, l'operazione è riuscita.
                    return PID;
                }
                else
                {
                    //Se non è stato possibile recuperare il PID, l'operazione è fallita, l'evento viene messo a log.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'ID di un processo", EventAction.ProcessPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare l'ID di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }

        /// <summary>
        /// Recupera la quantità di memoria privata utilizzata dal processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>La quantità di memoria privata espressa in byte, 0 in caso di errore.</returns>
        public static ulong GetProcessPrivateBytes(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                bool Result = Win32ProcessFunctions.GetProcessMemoryInfo(Handle.DangerousGetHandle(), out Win32Structures.PROCESS_MEMORY_COUNTERS CountersInfo, (uint)Marshal.SizeOf(typeof(Win32Structures.PROCESS_MEMORY_COUNTERS)));
                if (Result)
                {
                    //Se le informazioni sulla memoria sono recuperate correttamente il metodo restituisce il valore del campo PageFileUsage
                    //che rappresenta la memoria privata usata dal processo.
                    return (ulong)CountersInfo.PageFileUsage.ToInt64();
                }
                else
                {
                    //Se non è stato possibile recuperare le informazioni sulla memoria, l'operazione è fallita, l'evento viene messo a log e il metodo restituisce 0.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sull'utilizzo di memoria di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sull'utilizzo di memoria di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
            
        }

        /// <summary>
        /// Recupera la data di creazione di un processo.
        /// </summary>
        /// <param name="Handle">Handle del processo.</param>
        /// <returns>Oggetto <see cref="DateTime"/> contenente la data di creazione di un processo.</returns>
        public static DateTime? GetProcessStartTime(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Win32ProcessFunctions.GetProcessTimes(Handle.DangerousGetHandle(), out Win32Structures.FILETIME CreationTime, out _, out _, out _))
                {
                    //Se il recupero della data di creazione del processo è riuscito, la struttura FILETIME che la rappresenta viene convertita
                    //in una struttura DateTime che rappresenta il risultato del metodo.
                    return FileTimeToDateTime(CreationTime);
                }
                else
                {
                    //Se non è stato possibile recuperare la data di creazione del processo, l'operazione è fallita, l'evento viene messo a log.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sulle tempistiche di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sulle tempistiche di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la data di creazione di un processo.
        /// </summary>
        /// <param name="Handle">Handle nativo al processo.</param>
        /// <returns>Oggetto <see cref="DateTime"/> contenente la data di creazione di un processo.</returns>
        /// <remarks>Questo metodo è una variante del metodo <see cref="NativeHelpers.GetProcessStartTime(SafeProcessHandle)"/> che accetta un handle nativo al posto di un handle gestito, esegue le stesse operazioni.</remarks>
        private static DateTime? GetProcessStartTime(IntPtr Handle)
        {
            if (Handle != IntPtr.Zero)
            {
                if (Win32ProcessFunctions.GetProcessTimes(Handle, out Win32Structures.FILETIME CreationTime, out _, out _, out _))
                {
                    //Se il recupero della data di creazione del processo è riuscito, la struttura FILETIME che la rappresenta viene convertita
                    //in una struttura DateTime che rappresenta il risultato del metodo.
                    return FileTimeToDateTime(CreationTime);
                }
                else
                {
                    //Se non è stato possibile recuperare la data di creazione del processo, l'operazione è fallita, l'evento viene messo a log.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sulle tempistiche di un processo", EventAction.ProcessPropertiesRead, new SafeProcessHandle(Handle, false), ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sulle tempistiche di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il nome del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="PID">ID del processo, se disponibile.</param>
        /// <param name="DebugLoggingCall">Indica se metodo è stato chiamato durante il log di debug.</param>
        /// <returns>Il nome del processo, nullo in caso di errore.</returns>
        public static string GetProcessName(SafeProcessHandle Handle, uint? PID = null)
        {
            if (Handle != null)
            {
                if (!PID.HasValue)
                {
                    PID = GetProcessPID(Handle);
                }
                switch (PID)
                {
                    case 0:
                        return "System Idle Process";
                    case 4:
                        return "System";
                    case 56:
                        return "Secure System";
                    case 116:
                        return "Registry";
                    default:
                        string FullPath = GetProcessFullPathNT(Handle);
                        //Se non è stato possibile recuperare il percorso completo del processo (FullPath è "Non disponibile"), il metodo restituisce "Non disponibile" altrimenti
                        //estrae il nome del processo dal percorso e lo restituisce come risultato.
                        return FullPath == Properties.Resources.UnavailableText ? FullPath : Path.GetFileName(FullPath);
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un process, handle non disponibile", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la descrizione dell'eseguibile del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>La descrizione del processo.</returns>
        public static string GetProcessDescription(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                FileVersionInfo Info = GetFileVersionInfo2(Handle);
                if (Info != null)
                {
                    //Se è stato possibile recuperare le informazioni sulla verione di un processo, viene restituita la descrizione.
                    return Info.FileDescription;
                }
                else
                {
                    //Se non è stato possibile recuperare le informazioni sulla verione di un processo, viene restituito "Non diponibile".
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni di versione su un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il numero di thread di un processo.
        /// </summary>
        /// <param name="ProcessID">ID del processo.</param>
        /// <returns>Il numero di thread.</returns>
        public static uint GetProcessThreadCount(uint ProcessID)
        {
            IntPtr SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPTHREAD, 0);
            if (SnapshotHandle != IntPtr.Zero)
            {
                Win32Structures.THREADENTRY32 ThreadInfo = new();
                ThreadInfo.StructureSize = Convert.ToUInt32(Marshal.SizeOf(ThreadInfo));
                uint ThreadCount = 0;
                if (!Win32ProcessFunctions.Thread32First(SnapshotHandle, ref ThreadInfo))
                {
                    //Se non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema,
                    //l'operazione è fallita, l'handle allo snapshot viene chiuso e l'evento viene messo a log,
                    //il metodo restituisce 0.
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema", EventAction.ProcessPropertiesRead, null, ex.NativeErrorCode, ex.Message, ProcessID);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
                else
                {
                    if (ProcessID == ThreadInfo.OwnerPID)
                    {
                        //Se il thread appartiene al processo il conteggio aumenta.
                        ThreadCount += 1;
                    }
                    bool End = false;
                    while (!End)
                    {
                        if (!Win32ProcessFunctions.Thread32Next(SnapshotHandle, ref ThreadInfo))
                        {
                            int ErrorCode = Marshal.GetLastWin32Error();
                            if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                            {
                                //Se non ci sono più thread l'operazione è completata.
                                End = true;
                            }
                            else
                            {
                                //Se non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema,
                                //l'operazione è fallita, l'handle allo snapshot viene chiuso e l'evento viene messo a log,
                                //il metodo restituisce 0.
                                _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                Win32Exception ex = new(ErrorCode);
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema", EventAction.ProcessPropertiesRead, null, ex.NativeErrorCode, ex.Message, ProcessID);
                                Logger.WriteEntry(Entry);
                                return 0;
                            }
                        }
                        else
                        {
                            if (ProcessID == ThreadInfo.OwnerPID)
                            {
                                //Se il thread appartiene al processo il conteggio aumenta.
                                ThreadCount += 1;
                            }
                        }

                    }
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                }
                return ThreadCount;
            }
            else
            {
                //Se non è stato possibile eseguire uno snapshot del sistema, l'operazione è fallita e l'evento viene messo a log.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile eseguire uno snapshot del sistema", EventAction.ProcessPropertiesRead, null, ex.NativeErrorCode, ex.Message, ProcessID);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }
        #endregion
        #region Detailed Properties (Generic)
        /// <summary>
        /// Recupera il nome del processo padre di un processo.
        /// </summary>
        /// <param name="ParentPID">ID del processo padre.</param>
        /// <param name="ChildStartTime">Data e ora di avvio del processo figlio.</param>
        /// <returns>Il nome del processo padre oppure una stringa vuota se il processo padre è terminato, il valore di ritorno è nullo in caso di errore.</returns>
        public static string GetParentProcessName(uint ParentPID, DateTime ChildStartTime)
        {
            IntPtr ProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ParentPID);
            if (ProcessHandle != IntPtr.Zero)
            {
                DateTime? ParentStartTime = GetProcessStartTime(ProcessHandle);
                if (ParentStartTime.HasValue)
                {
                    if (ParentStartTime < ChildStartTime)
                    {
                        //Se la data di creazione del processo padre è precedente a quella dell'eventuale figlio,
                        //viene recuperato il nome del processo.
                        SafeProcessHandle SafeHandle = new(ProcessHandle, true);
                        string ProcessName = GetProcessName(SafeHandle);
                        SafeHandle.Dispose();
                        return ProcessName;
                    }
                    else
                    {
                        //Se la data di creazione del processo padre è successiva a quella dell'eventuale figlio,
                        //viene restituita una stringa vuota.
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome del processo padre, il PID non è quello del padre", EventAction.ProcessPropertiesRead, null);
                        Logger.WriteEntry(Entry);
                        _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                        return string.Empty;
                    }
                }
                else
                {
                    //Se non è stato possibile recuperare la data di creazione del processo, il valore di ritorno è nullo.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome del processo padre, data di avvio del processo padre non disponibile", EventAction.ProcessPropertiesRead, null);
                    Logger.WriteEntry(Entry);
                    _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                    return null;
                }
            }
            else
            {
                //Se non è stato possibile aprire un handle al processo, l'evento viene messo a log e il valore di ritorno del metodo è nullo.
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un processo", EventAction.ProcessPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il percorso completo dell'eseguibile di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il percorso completo dell'eseguibile del processo se disponibile.</returns>
        /// <remarks>Questo metodo usa la funzione <see cref="Win32ProcessFunctions.NtQueryInformationProcess"/> per recuperare l'informazione.</remarks>
        public static string GetProcessFullPathNT(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessImageFileNameWin32, IntPtr.Zero, 0, out uint ReturnSize);
                if (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                {
                    IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnSize);
                    Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessImageFileNameWin32, Buffer, ReturnSize, out _);
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.UNICODE_STRING ImageName = (Win32Structures.UNICODE_STRING)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.UNICODE_STRING));
                        return ImageName.Length > 0 ? ImageName.Buffer : Properties.Resources.UnavailableText;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare il percorso completo di un processo", EventAction.ProcessPropertiesRead, null, Result);
                        Logger.WriteEntry(Entry);
                        return Properties.Resources.UnavailableText;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare il percorso completo di un processo", EventAction.ProcessPropertiesRead, null, Result);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il percorso completo di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera la versione dell'eseguibile del processo.
        /// </summary>
        /// <param name="Handle"></param>
        /// <returns>La versione dell'eseguibile.</returns>
        public static string GetProcessVersion(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint[] Info = GetFileVersionInfo(Handle);
                if (Info != null)
                {
                    StringBuilder VersionString = new();
                    //Se le informazioni sulla versione sono disponibili, vengono estratti i componenti della versione del processo (Major, Minor, Build, Revision).
                    ushort MajorVersion = (ushort)(Info[0] >> 16 & 0xffff);
                    ushort MinorVersion = (ushort)(Info[0] & 0xffff);
                    ushort Build = (ushort)(Info[1] >> 16 & 0xffff);
                    ushort Revision = (ushort)(Info[1] & 0xffff);
                    //Il valore di ritorno del metodo è una stringa nel seguente formato:
                    //<Major version>.<Minor version>.<Build>.<Revision>
                    VersionString.Append(MajorVersion).Append('.').Append(MinorVersion).Append('.').Append(Build).Append('.').Append(Revision);
                    return VersionString.ToString();
                }
                else
                {
                    //Se le informazioni sulla versione non sono disponibili il valore di ritorno del metodo è la stringa "Non disponibile".
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la versione di un processo", EventAction.ProcessPropertiesRead, Handle);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recupera la versione di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera le informazioni sulla versione del file eseguibile del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array che contiene le informazioni.</returns>
        private static uint[] GetFileVersionInfo(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint[] VersionInfoData;
                string ProcessFullPath = GetProcessFullPathNT(Handle);
                if (ProcessFullPath != Properties.Resources.UnavailableText)
                {
                    uint VersionInfoSize = Win32FileFunctions.GetFileVersionInfoSize(ProcessFullPath, out _);
                    if (VersionInfoSize > 0)
                    {
                        //Se esiste informazioni di versione per il file e non c'è stato nessun errore, viene allocata la memoria necessaria per scrivere le informazioni.
                        IntPtr VersionInfo = Marshal.AllocHGlobal((int)VersionInfoSize);
                        if (Win32FileFunctions.GetFileVersionInfo(ProcessFullPath, 0, VersionInfoSize, VersionInfo))
                        {
                            //Se le informazioni di versione sono state recuperate, la funzione VerQueryValue viene utilizzata per recuperare la parte fissa delle informazioni.
                            if (Win32FileFunctions.VerQueryValue(VersionInfo, "\\", out IntPtr VersionInfoPointer, out _))
                            {
                                Win32Structures.VS_FIXEDFILEINFO VersionInfoStructure = (Win32Structures.VS_FIXEDFILEINFO)Marshal.PtrToStructure(VersionInfoPointer, typeof(Win32Structures.VS_FIXEDFILEINFO));
                                Marshal.FreeHGlobal(VersionInfo);
                                //Questo array conterrà i due componenti del numero di versione del file e rappresenta il risultato del metodo.
                                VersionInfoData = new uint[2];
                                //I componenti del numero di versione del file vengono recuperati dalla struttura e inseriti nell'array.
                                VersionInfoData[0] = VersionInfoStructure.FileVersionMS;
                                VersionInfoData[1] = VersionInfoStructure.FileVersionLS;
                                return VersionInfoData;
                            }
                            else
                            {
                                //Se non è stato possibile recuperare l'informazione, l'evento viene messo a log e il valore di ritorno è nullo.
                                Marshal.FreeHGlobal(VersionInfo);
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sulla versione del file eseguibile associato a un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message, ProcessName: Path.GetFileName(ProcessFullPath));
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                        else
                        {
                            //Se non è stato possibile recuperare le informazioni di versione del file, l'evento viene messo a log e il valore di ritorno è nullo.
                            Marshal.FreeHGlobal(VersionInfo);
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sulla versione del file eseguibile associato a un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message, ProcessName: Path.GetFileName(ProcessFullPath));
                            Logger.WriteEntry(Entry);
                            return null;
                        }
                    }
                    else
                    {
                        //Se non è stato possibile recuperare le dimensioni delle informazioni di versione del file, l'evento viene messo a log e il valore di ritorno è nullo.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la dimensione delle informazioni sulla versione del file eseguibile associato a un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    //Se non stato possibile recuperare il percorso completo del file, l'operazione è fallita e il valore di ritorno del metodo è nullo.
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la versione del processo, il percorso completo del processo non è disponibile", EventAction.ProcessPropertiesRead, Handle, ProcessName: Path.GetFileName(ProcessFullPath));
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la versione di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera le informazioni sulla versione del file eseguibile del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="ProcessFullPath">Percorso completo del processo.</param>
        /// <returns>Un'istanza di <see cref="FileVersionInfo"/> che contiene le informazioni.</returns>
        private static FileVersionInfo GetFileVersionInfo2(SafeProcessHandle Handle, string ProcessFullPath = null)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ProcessFullPath))
                    {
                        ProcessFullPath = GetProcessFullPathNT(Handle);
                    }
                    if (ProcessFullPath != Properties.Resources.UnavailableText)
                    {
                        return FileVersionInfo.GetVersionInfo(ProcessFullPath);
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sulla versione di un file, percorso file non disponibile", EventAction.ProcessPropertiesRead, Handle);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                catch (FileNotFoundException)
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni di versione di un processo, file non trovato", EventAction.ProcessPropertiesRead, Handle);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la versione di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera l'affinità del processo.
        /// </summary>
        /// <returns>Un intero a 64 bit senza segno rappresentante l'affinità del processo.</returns>
        public static ulong? GetProcessAffinity(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Win32ProcessFunctions.GetProcessAffinityMask(Handle.DangerousGetHandle(), out IntPtr ProcessAffinityMask, out _))
                {
                    return (ulong)ProcessAffinityMask.ToInt64();
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'affinità di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare l'affinità di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera l'indirizzo del PEB (Process Environment Block) di un processo come stringa.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>La rappresentazione di stringa dell'indirizzo di memoria del PEB.</returns>
        public static string GetProcessPEBAddress(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED)));
                uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessBasicInformation, StructurePointer, (uint)Marshal.SizeOf(typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED)), out _);
                if (Result == 0)
                {
                    Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED BasicInfo = (Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED)Marshal.PtrToStructure(StructurePointer, typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED));
                    Marshal.FreeHGlobal(StructurePointer);
                    //Il puntatore al PEB è trasformato in un numero a 64 bit e viene restituito dal metodo come stringa in formato esadecimale.
                    long PointerValue = BasicInfo.PEBAddress.ToInt64();
                    return "0x" + PointerValue.ToString("X", CultureInfo.InvariantCulture);
                }
                else
                {
                    Marshal.FreeHGlobal(StructurePointer);
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare l'indirizzo di memoria del PEB di un processo", EventAction.ProcessPropertiesRead, Handle, Result);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare l'indirizzo di memoria del PEB di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera l'indirizzo del PEB (Process Environment Block) di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Puntatore all'indirizzo di memoria del PEB.</returns>
        private static IntPtr GetProcessPEBAddress2(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                int StructureSize = Marshal.SizeOf(typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED));
                IntPtr StructurePointer = Marshal.AllocHGlobal(StructureSize);
                uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessBasicInformation, StructurePointer, (uint)Marshal.SizeOf(typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED)), out _);
                if (Result == 0)
                {
                    Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED BasicInfo = (Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED)Marshal.PtrToStructure(StructurePointer, typeof(Win32Structures.PROCESS_BASIC_INFORMATION_DOCUMENTED));
                    Marshal.FreeHGlobal(StructurePointer);
                    return BasicInfo.PEBAddress;
                }
                else
                {
                    Marshal.FreeHGlobal(StructurePointer);
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare l'indirizzo di memoria del PEB di un processo", EventAction.ProcessPropertiesRead, Handle, Result);
                    Logger.WriteEntry(Entry);
                    return IntPtr.Zero;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare l'indirizzo di memoria del PEB di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Recupera le informazioni di protezione del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Una stringa che rappresenta il livello di protezione del processo.</returns>
        public static string GetProcessProtectionType(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PS_PROTECTION)));
                uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessProtectionInformation, StructurePointer, (uint)Marshal.SizeOf(typeof(Win32Structures.PS_PROTECTION)), out _);
                if (Result == 0)
                {
                    Win32Structures.PS_PROTECTION ProtectionInfo = (Win32Structures.PS_PROTECTION)Marshal.PtrToStructure(StructurePointer, typeof(Win32Structures.PS_PROTECTION));
                    Marshal.FreeHGlobal(StructurePointer);
                    //Il campo Level della struttura PS_PROTECTION rappresenta il tipo e il signer del processo.
                    //Gli array qui inizializzati conterranno le informazioni estratte dal campo Level:
                    //Info conterrà il valore di Level senza modifiche
                    //ProtectionLevel conterrà il valore a cui corrispondono i primi tre bit di Level
                    //ProtectionType conterrà il valore a cui corrispondono gli ultimi 4 bit di Level
                    byte[] Info = new byte[1];
                    byte[] ProtectionLevel = new byte[1];
                    byte[] ProtectionType = new byte[1];
                    Info[0] = ProtectionInfo.Level;
                    BitArray Bits = new(Info);
                    BitArray LevelBits = new(3);
                    BitArray TypeBits = new(4);
                    //I primi tre bit di Level rappresentano il tipo.
                    LevelBits.Set(0, Bits.Get(0));
                    LevelBits.Set(1, Bits.Get(1));
                    LevelBits.Set(2, Bits.Get(2));
                    LevelBits.CopyTo(ProtectionLevel, 0);
                    //Gli ultimi 4 bit rappresentano il signer.
                    TypeBits.Set(0, Bits.Get(4));
                    TypeBits.Set(1, Bits.Get(5));
                    TypeBits.Set(2, Bits.Get(6));
                    TypeBits.Set(3, Bits.Get(7));
                    TypeBits.CopyTo(ProtectionType, 0);
                    //I valori ricavati dalle operazioni precedenti vengono trasformati nel membro di enumerazione appropriato:
                    //ProcessProtectionLevel per il tipo
                    //ProcessProtectionType per il signer
                    Win32Enumerations.ProcessProtectionLevel Level = (Win32Enumerations.ProcessProtectionLevel)ProtectionLevel[0];
                    Win32Enumerations.ProcessProtectionType Type = (Win32Enumerations.ProcessProtectionType)ProtectionType[0];
                    //Il valore di ritorno del metodo è una stringa nel seguente formato:
                    //<tipo> (<signer>)
                    return Enum.GetName(typeof(Win32Enumerations.ProcessProtectionLevel), Level) + " (" + Enum.GetName(typeof(Win32Enumerations.ProcessProtectionType), Type) + ")";
                }
                else
                {
                    Marshal.FreeHGlobal(StructurePointer);
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare il livello di protezione di un processo", EventAction.ProcessPropertiesRead, Handle, Result);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recupera il livello di protezione di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera i criteri di mitigazione di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un istanza di <see cref="ProcessMitigationPoliciesData"/> che contiene i dati relativi sui criteri di mitigazione.</returns>
        public static ProcessMitigationPoliciesData GetProcessMitigationPolicies(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                //Questo metodo recupera informazioni sulle politiche di mitigazione attivate per un processo
                //Ognuna delle strutture seguenti contiene informazioni sulle componenti della politica a cui si riferiscono.
                //Il parametro out NoErrors ha valore true solo se si verifica un errore durante il recupero delle informazioni.
                //Se si verifica un errore tutte le informazioni recuperate fino a quel punto vengono scartate.
                DEPPolicyData DEP = GetDEPPolicy(Handle, out bool NoErrors);
                ASLRPolicyData ASLR = GetASLRPolicy(Handle, out NoErrors);
                DynamicCodePolicyData DynamicCode = GetDynamicCodePolicy(Handle, out NoErrors);
                StrictHandleCheckPolicyData StrictHandleCheck = GetStrictHandleCheckPolicy(Handle, out NoErrors);
                SystemCallDisablePolicyData SystemCallDisable = GetSystemCallDisablePolicy(Handle, out NoErrors);
                ExtensionPointDisablePolicyData ExtensionPointsDisable = GetExtensionPointsDisablePolicy(Handle, out NoErrors);
                CFGPolicyData CFG = GetCFGPolicy(Handle, out NoErrors);
                BinarySignaturePolicyData BinarySignature = GetBinarySignaturePolicy(Handle, out NoErrors);
                FontDisablePolicyData FontDisable = GetFontDisablePolicy(Handle, out NoErrors);
                ImageLoadPolicyData ImageLoad = GetImageLoadPolicy(Handle, out NoErrors);
                SideChannelIsolationPolicyData SideChannelIsolation = GetSideChannelIsolationPolicy(Handle, out NoErrors);
                UserShadowStackPolicyData UserShadowStack = GetUserShadowStackPolicy(Handle, out NoErrors);
                if (NoErrors)
                {
                    return new ProcessMitigationPoliciesData(DEP, ASLR, DynamicCode, StrictHandleCheck, SystemCallDisable, ExtensionPointsDisable, CFG, BinarySignature, FontDisable, ImageLoad, SideChannelIsolation, UserShadowStack);
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sui criteri di mitigazione di un processo, il recupero di informazioni su uno o più criteri non è riuscito", EventAction.ProcessPropertiesRead, Handle);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sui criteri di mitigazione di un processo, handle non valido", EventAction.ProcessPropertiesRead, Handle);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #region Policies Info Retrieval Methods
        /// <summary>
        /// Recupera la politica DEP di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="NoErrors">Indica se si sono verificati errori nel recupero delle informazioni.</param>
        /// <returns>Una struttura <see cref="DEPPolicyData"/> con le informazioni sulla politica.</returns>
        private static DEPPolicyData GetDEPPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_DEP_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessDEPPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_DEP_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_DEP_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_DEP_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_DEP_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new DEPPolicyData(Bits.Get(0), Bits.Get(1), Structure.Permanent);
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica DEP di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new DEPPolicyData();
            }
        }

        private static ASLRPolicyData GetASLRPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_ASLR_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessASLRPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_ASLR_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_ASLR_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_ASLR_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_ASLR_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new ASLRPolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2), Bits.Get(3));
            }
            else
            {
                NoErrors = false;
                Win32Exception ex = new (Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica ASLR di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new ASLRPolicyData();
            }
        }

        private static DynamicCodePolicyData GetDynamicCodePolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_DYNAMIC_CODE_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessDynamicCodePolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_DYNAMIC_CODE_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_DYNAMIC_CODE_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_DYNAMIC_CODE_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_DYNAMIC_CODE_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new DynamicCodePolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2), Bits.Get(3));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica del codice dinamico di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new DynamicCodePolicyData();
            }
        }

        private static StrictHandleCheckPolicyData GetStrictHandleCheckPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessStrictHandleCheckPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_STRICT_HANDLE_CHECK_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new StrictHandleCheckPolicyData(Bits.Get(0), Bits.Get(1));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa agli handle non validi di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new StrictHandleCheckPolicyData();
            }
        }

        private static SystemCallDisablePolicyData GetSystemCallDisablePolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessSystemCallDisablePolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_SYSTEM_CALL_DISABLE_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new SystemCallDisablePolicyData(Bits.Get(0), Bits.Get(1));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa alle system call di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new SystemCallDisablePolicyData();
            }
        }

        private static ExtensionPointDisablePolicyData GetExtensionPointsDisablePolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessExtensionPointDisablePolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_EXTENSION_POINT_DISABLE_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new ExtensionPointDisablePolicyData(Bits.Get(0));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa agli extension points di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new ExtensionPointDisablePolicyData();
            }
        }

        private static CFGPolicyData GetCFGPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessControlFlowGuardPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_CONTROL_FLOW_GUARD_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new CFGPolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica CFG di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new CFGPolicyData();
            }
        }

        private static BinarySignaturePolicyData GetBinarySignaturePolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessSignaturePolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new BinarySignaturePolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2), Bits.Get(3), Bits.Get(4));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa alle immagini firmate di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new BinarySignaturePolicyData();
            }
        }

        private static FontDisablePolicyData GetFontDisablePolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_FONT_DISABLE_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessFontDisablePolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_FONT_DISABLE_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_FONT_DISABLE_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_FONT_DISABLE_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_FONT_DISABLE_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new FontDisablePolicyData(Bits.Get(0), Bits.Get(1));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa al caricamento di font non di sistema di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new FontDisablePolicyData();
            }
        }

        private static ImageLoadPolicyData GetImageLoadPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_IMAGE_LOAD_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessImageLoadPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_IMAGE_LOAD_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_IMAGE_LOAD_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_IMAGE_LOAD_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_IMAGE_LOAD_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new ImageLoadPolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2), Bits.Get(3), Bits.Get(4));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa al caricamento di immagini remote di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new ImageLoadPolicyData();
            }
        }

        private static SideChannelIsolationPolicyData GetSideChannelIsolationPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessSideChannelIsolationPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_SIDE_CHANNEL_ISOLATION_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new SideChannelIsolationPolicyData(Bits.Get(0), Bits.Get(1), Bits.Get(2), Bits.Get(3));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa ai side channels di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new SideChannelIsolationPolicyData();
            }
        }

        private static UserShadowStackPolicyData GetUserShadowStackPolicy(SafeProcessHandle Handle, out bool NoErrors)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY)));
            if (Win32ProcessFunctions.GetProcessMitigationPolicy(Handle.DangerousGetHandle(), Win32Enumerations.ProcessMitigationPolicy.ProcessUserShadowStackPolicy, Buffer, Marshal.SizeOf(typeof(Win32Structures.PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY))))
            {
                NoErrors = true;
                Win32Structures.PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY Structure = (Win32Structures.PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PROCESS_MITIGATION_USER_SHADOW_STACK_POLICY));
                Marshal.FreeHGlobal(Buffer);
                BitArray Bits = new(BitConverter.GetBytes(Structure.Flags));
                return new UserShadowStackPolicyData(Bits.Get(0));
            }
            else
            {
                NoErrors = false;
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla politica relativa alla protezione dello stack basata su hardware di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return new UserShadowStackPolicyData();
            }
        }
        #endregion
        #endregion
        #region Detailed Properties (Statistics)
        #region CPU Statistics
        /// <summary>
        /// Recupera il numero di cicli CPU di esecuzione del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il numero di cicli CPU.</returns>
        public static ulong GetProcessCPUCycles(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Win32ProcessFunctions.QueryProcessCycleTime(Handle.DangerousGetHandle(), out ulong CycleTime))
                {
                    return CycleTime;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di cicli CPU di esecuzione di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il numero di cicli CPU di esecuzione di un processo", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }

        /// <summary>
        /// Recupera i tempi di esecuzione di un processo dal suo avvio (kernel, user e totale).
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array di oggetti <see cref="TimeSpan"/>.</returns>
        /// <remarks>Il primo elemento dell'array rappresenta il tempo di esecuzione kernel, il secondo elemento rappresenta il tempo di esecuzione user, il terzo elemento rappresenta il tempo di esecuzione totale.</remarks>
        public static TimeSpan?[] GetProcessTimes(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                TimeSpan?[] ProcessTimes = new TimeSpan?[3];
                if (Win32ProcessFunctions.GetProcessTimes(Handle.DangerousGetHandle(), out _, out _, out Win32Structures.FILETIME KernelTime, out Win32Structures.FILETIME UserTime))
                {
                    //Le tempistiche del processo sono trasformate da una struttura FILETIME a una struttura TimeSpan e inserite nell'array.
                    //Indice 0: tempo kernel
                    //Indice 1: tempo user
                    //Indice 2: tempo totale (kernel + user)
                    ProcessTimes[0] = TimeSpan.FromTicks(FileTimeToInt64(KernelTime));
                    ProcessTimes[1] = TimeSpan.FromTicks(FileTimeToInt64(UserTime));
                    ProcessTimes[2] = TimeSpan.FromTicks(ProcessTimes[0].Value.Ticks + ProcessTimes[1].Value.Ticks);
                    return ProcessTimes;
                }
                else
                {
                    //Se non è stato possibile recuperare le tempistiche del processo. l'evento viene messo a log,
                    //I contenuti dell'array sono nulli.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le tempistiche di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    ProcessTimes[0] = null;
                    ProcessTimes[1] = null;
                    ProcessTimes[2] = null;
                    return ProcessTimes;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le tempistiche di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #region Memory Statistics
        /// <summary>
        /// Recupera le informazioni sulla memoria di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array di interi che rappresentato le diverse informazioni.</returns>
        /// <remarks>Il primo elemento è la memoria privata corrente, il secondo la memoria privata massima, il terzo la memoria virtuale corrente, il quarto la memoria virtuale massima, il quinto il numero di page fault, il sesto la dimensione corrente del working set, l'ultimo la dimensione massima del working set.</remarks>
        public static ulong[] GetProcessMemorySizes(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                ulong[] MemoryData = new ulong[7];
                IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.VM_COUNTERS)));
                uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessVmCounters, StructurePointer, (uint)Marshal.SizeOf(typeof(Win32Structures.VM_COUNTERS)), out _);
                if (Result == 0)
                {
                    Win32Structures.VM_COUNTERS Structure = (Win32Structures.VM_COUNTERS)Marshal.PtrToStructure(StructurePointer, typeof(Win32Structures.VM_COUNTERS));
                    Marshal.FreeHGlobal(StructurePointer);
                    //I contatori di utilizzo della memoria presenti nella struttura VM_COUNTERS sono inseriti nell'array ai seguenti indici:
                    //0: memoria privata
                    //1: memoria privata massima
                    //2: memoria virtuale
                    //3: memoria virtuale massima
                    //4: conteggio page fault
                    //5: dimensione working set
                    //6: dimensione massima working set
                    MemoryData[0] = (ulong)Structure.PageFileUsage.ToInt64();
                    MemoryData[1] = (ulong)Structure.PeakPageFileUsage.ToInt64();
                    MemoryData[2] = (ulong)Structure.VirtualSize.ToInt64();
                    MemoryData[3] = (ulong)Structure.PeakVirtualSize.ToInt64();
                    MemoryData[4] = Structure.PageFaultCount;
                    MemoryData[5] = (ulong)Structure.WorkingSetSize.ToInt64();
                    MemoryData[6] = (ulong)Structure.PeakWorkingSetSize.ToInt64();
                    return MemoryData;
                }
                else
                {
                    //Se non è stato possibile recuperare le informazioni sull'utilizzo della memoria, l'evento viene messo a log e il valore di ritorno del metodo è nullo.
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare le informazioni sulla memoria di un processo", EventAction.ProcessPropertiesRead, Handle, Result);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sulla memoria di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la composizione del working set di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="MemoryPageSize">Dimensione di una pagina in memoria.</param>
        /// <returns>Un array con le informazioni.</returns>
        /// <remarks>Il primo elemento dell'array rappresenta la dimensione delle pagine private del WS, il secondo la dimensione delle pagine condivisibili, il terzo la dimensione delle pagine condivise.</remarks>
        public static ulong[] GetWorkingSetDetailedInfo(SafeProcessHandle Handle, uint MemoryPageSize)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                int PrivatePagesCount;
                int ShareablePagesCount = 0;
                int SharedPagesCount = 0;
                int PagesCount;
                ulong[] WsData = new ulong[3];
                uint BufferSize = (uint)IntPtr.Size * 2;
                IntPtr StructurePointer = Marshal.AllocHGlobal((int)BufferSize);
                bool Result = Win32ProcessFunctions.QueryWorkingSet(Handle.DangerousGetHandle(), StructurePointer, BufferSize);
                int ErrorCode;
                while (!Result)
                {
                    ErrorCode = Marshal.GetLastWin32Error();
                    if (ErrorCode == Win32Constants.ERROR_BAD_LENGTH)
                    {
                        BufferSize *= 2;
                        StructurePointer = Marshal.ReAllocHGlobal(StructurePointer, (IntPtr)BufferSize);
                        Result = Win32ProcessFunctions.QueryWorkingSet(Handle.DangerousGetHandle(), StructurePointer, BufferSize);
                    }
                    else
                    {
                        Marshal.FreeHGlobal(StructurePointer);
                        Win32Exception ex = new(ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sul working set di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                if (Result)
                {
                    IntPtr SecondBuffer = StructurePointer;
                    ulong NumberOfEntries = (ulong)Marshal.ReadIntPtr(StructurePointer).ToInt64();
                    //Il valore di PagesCount corrisponde al campo NumberOfEntries della struttura PSAPI_WORKING_SET_INFORMATION.
                    PagesCount = (int)NumberOfEntries;
                    SecondBuffer += IntPtr.Size;
                    long[] WorkingSetInfo = new long[NumberOfEntries];
                    Marshal.Copy(SecondBuffer, WorkingSetInfo, 0, (int)NumberOfEntries);
                    Marshal.FreeHGlobal(StructurePointer);
                    //Per ogni pagina viene determinato se è condivisibile e se è condivisa, i relativi contatori vengono aggiornati (ShareablePagesCount e SharedPagesCount).
                    foreach (long info in WorkingSetInfo)
                    {
                        if (IsPageShareable((ulong)info, out uint ShareCount))
                        {
                            ShareablePagesCount += 1;
                            //Se ShareCount è maggiore di 1 significa che la pagina è condivisa.
                            if (ShareCount > 1)
                            {
                                SharedPagesCount += 1;
                            }
                        }
                    }
                    //Il conteggio delle pagine private viene determinato sottraendo dal totale il numero di pagine condivisibili.
                    //L'array viene riempito nel seguente modo:
                    //Indice 0: dimensione totale delle pagine private
                    //Indice 1: dimensione totale delle pagine condivisibili
                    //Indice 2: dimensione totale delle pagine condivise
                    PrivatePagesCount = PagesCount - ShareablePagesCount;
                    WsData[0] = (ulong)PrivatePagesCount * MemoryPageSize;
                    WsData[1] = (ulong)ShareablePagesCount * MemoryPageSize;
                    WsData[2] = (ulong)SharedPagesCount * MemoryPageSize;
                    return WsData;
                }
                else
                {
                    Marshal.FreeHGlobal(StructurePointer);
                    //Se non è stato possibile recuperare le informazioni sul working set del processo, l'evento viene messo a log, il valore di ritorno del metodo è nullo.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sul working set di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sul working set di un processo", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Determina se una pagina è condivisibile, in tal caso indica anche con quanti processi essa è condivisa.
        /// </summary>
        /// <param name="PageInfo">Informazioni sulla pagina.</param>
        /// <param name="ShareCount">Numero di processi che condividono la pagina.</param>
        /// <returns>true se la pagina è condivisibile, false altrimenti.</returns>
        private static bool IsPageShareable(ulong PageInfo, out uint ShareCount)
        {
            byte[] PageInfoBytes = BitConverter.GetBytes(PageInfo);
            byte[] ShareCountBytes = new byte[1];
            bool Shared;
            ShareCount = 0;
            BitArray Bits = new(PageInfoBytes);
            BitArray ShareCountBits = new(3);
            //I bit 5, 6 e 7 del valore di Pageinfo rappresentano il numero di processi con cui la pagina è condivisa.
            ShareCountBits.Set(0, Bits.Get(5));
            ShareCountBits.Set(1, Bits.Get(6));
            ShareCountBits.Set(2, Bits.Get(7));
            //L'ottavo bit del valore di Pageinfo indica se la pagina è condivisa.
            Shared = Bits.Get(8);
            if (Shared)
            {
                ShareCountBits.CopyTo(ShareCountBytes, 0);
                ShareCount = ShareCountBytes[0];
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region I/O Statistics
        /// <summary>
        /// Recupera informazioni sulle operazioni I/O effettuate da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array di valori che rappresentano l'attività I/O del processo.</returns>
        /// <remarks>Il primo elemento dell'array rappresenta il numero di operazioni di lettura effettuate, il secondo rappresenta il numero di operazioni di scrittura effettuate, il terzo rappresenta il numero di operazione effettuate diverse da lettura e scrittura, il quarto rappresenta i byte letti, il quinto rappresenta i byte scritti, il sesto rappresenta i byte trasferiti nel contesto di operazioni diverse da lettura e scrittura.</remarks>
        public static ulong[] GetProcessIOInfo(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                ulong[] IOData = new ulong[6];
                if (Win32ProcessFunctions.GetProcessIOCounters(Handle.DangerousGetHandle(), out Win32Structures.IO_COUNTERS Counters))
                {
                    //Le informazioni contenute nella struttura IO_COUNTERS vengono inserite nell'array nel seguente modo:
                    //0: conteggio operazioni in lettura
                    //1: conteggio operazioni in scrittura
                    //2: conteggio altre operazioni (diverse da lettura e scrittura)
                    //3: numero di byte letti
                    //4: numero di byte scritti
                    //5: numero di byte trasferiti durante operazioni diverse da lettura e scrittura.
                    IOData[0] = Counters.ReadOperationsCount;
                    IOData[1] = Counters.WriteOperationCount;
                    IOData[2] = Counters.OtherOperationCount;
                    IOData[3] = Counters.ReadTransferCount;
                    IOData[4] = Counters.WriteTransferCount;
                    IOData[5] = Counters.OtherTransferCount;
                    return IOData;
                }
                else
                {
                    //Se non è stato possibile recuperare i contatori I/O del processo, l'evento viene messo a log, il valore di ritorno del metodo è nullo.
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni sull'attività I/O di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sull'attività I/O di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #region Handle Statistics
        /// <summary>
        /// Recupera il numero di handle aperti da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il numero di handle aperti, 0 se l'operazione è fallita.</returns>
        public static uint GetProcessHandleCount(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Win32ProcessFunctions.GetProcessHandleCount(Handle.DangerousGetHandle(), out uint HandleCount))
                {
                    return HandleCount;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di handle aperti da un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il numero di handle aperti da un processo", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return 0;
            }
        }

        /// <summary>
        /// Recupera il numero di handle a oggetti GDI aperti da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il numero di handle aperti, -1 se l'operazione è fallita.</returns>
        public static int GetProcessGDIHandlesCount(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint HandleCount = Win32ProcessFunctions.GetGuiResources(Handle.DangerousGetHandle(), Win32Enumerations.ProcessGUIObjectType.GR_GDIOBJECTS);
                if (HandleCount == 0)
                {
                    //Il valore 0 restituito dalla funzione può indicare una condizione di errore oppure che il processo non ha handle GDI aperti.
                    //Se l'ultimo codice di errore Win32 è diverso da 0 significa che si è verificato un errore, in tal caso l'evento viene messo a log e il metodo restituisce -1,
                    //in caso contrario viene restituito il valore ricevuto.
                    if (Marshal.GetLastWin32Error() == 0)
                    {
                        return (int)HandleCount;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di handle GDI aperti da un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return -1;
                    }
                }
                else
                {
                    return (int)HandleCount;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il numero di handle GDI aperti da un processo", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return -1;
            }
        }

        /// <summary>
        /// Recupera il numero di handle a oggetti USER aperti da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il numero di handle aperti, -1 se l'operazione è fallita.</returns>
        public static int GetProcessUSERHandlesCount(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                uint HandleCount = Win32ProcessFunctions.GetGuiResources(Handle.DangerousGetHandle(), Win32Enumerations.ProcessGUIObjectType.GR_USEROBJECTS);
                if (HandleCount == 0)
                {
                    //Il valore 0 restituito dalla funzione può indicare una condizione di errore oppure che il processo non ha handle USER aperti.
                    //Se l'ultimo codice di errore Win32 è diverso da 0 significa che si è verificato un errore, in tal caso l'evento viene messo a log e il metodo restituisce -1,
                    //in caso contrario viene restituito il valore ricevuto.
                    if (Marshal.GetLastWin32Error() == 0)
                    {
                        return (int)HandleCount;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di handle USER aperti da un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return -1;
                    }
                }
                else
                {
                    return (int)HandleCount;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il numero di handle USER aperti da un processo", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return -1;
            }
        }

        /// <summary>
        /// Recupera informazioni sugli handle aperti da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="ProcessID">ID del processo.</param>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <returns>Un array di istanze di <see cref="HandleInfo"/> con le informazioni.</returns>
        public static HandleInfo[] GetProcessHandlesInfo(SafeProcessHandle Handle, string ProcessName = null, uint? ProcessID = null)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                List<HandleInfo> HandlesInfoList = new();
                IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.SYSTEM_HANDLE_INFORMATION_EX)));
                uint Size = (uint)Marshal.SizeOf(typeof(Win32Structures.SYSTEM_HANDLE_INFORMATION_EX));
                //Recupera informazioni su tutti gli handle aperti nel sistema.
                uint Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemExtendedHandleInformation, StructurePointer, Size, out _);
                while (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                {
                    Size = Size *= 2;
                    StructurePointer = Marshal.ReAllocHGlobal(StructurePointer, (IntPtr)Size);
                    Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemExtendedHandleInformation, StructurePointer, Size, out _);
                }
                if (Result == Win32Constants.STATUS_SUCCESS)
                {
                    Win32Structures.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX HandleInfo;
                    Win32Structures.OBJECT_TYPE_INFORMATION TypeInfo;
                    IntPtr SecondBuffer = StructurePointer;
                    uint NumberOfHandles = (uint)Marshal.ReadIntPtr(SecondBuffer).ToInt32();
                    SecondBuffer += IntPtr.Size * 2;
                    if (string.IsNullOrWhiteSpace(ProcessName))
                    {
                        ProcessName = GetProcessName(Handle);
                    }
                    //Per ogni handle determina se appartiene al processo, recupera informazioni sul suo tipo e tenta di recuperarne il nome.
                    for (uint i = 0; i < NumberOfHandles; i++)
                    {
                        HandleInfo = (Win32Structures.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)); //HandlesInfo.Handles[i];
                        SecondBuffer += Marshal.SizeOf(typeof(Win32Structures.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
                        if (!ProcessID.HasValue)
                        {
                            ProcessID = GetProcessPID(Handle);
                        }
                        if (ProcessID != 0)
                        {
                            if ((uint)HandleInfo.ProcessID.ToInt32() == ProcessID)
                            {
                                //L'handle viene duplicato così che possa essere usato nel processo corrente.
                                Result = Win32OtherFunctions.NtDuplicateObject(Handle.DangerousGetHandle(), HandleInfo.HandleValue, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, 0, (uint)Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS);
                                if (Result == Win32Constants.STATUS_SUCCESS)
                                {
                                    Size = (uint)Marshal.SizeOf(typeof(Win32Structures.OBJECT_TYPE_INFORMATION));
                                    IntPtr TypeInfoStructurePointer = Marshal.AllocHGlobal((int)Size);
                                    //Il buffer viene ridimensionato alla dimensione indicata dal parametro ReturnLength della funzione NtQueryObject.
                                    if (Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectTypeInformation, TypeInfoStructurePointer, Size, out uint ReturnLength) == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                                    {
                                        Size = ReturnLength;
                                        Marshal.FreeHGlobal(TypeInfoStructurePointer);
                                        TypeInfoStructurePointer = Marshal.AllocHGlobal((int)Size);
                                    }
                                    Result = Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectTypeInformation, TypeInfoStructurePointer, Size, out _);
                                    if (Result == Win32Constants.STATUS_SUCCESS)
                                    {
                                        TypeInfo = (Win32Structures.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(TypeInfoStructurePointer, typeof(Win32Structures.OBJECT_TYPE_INFORMATION));
                                        Marshal.FreeHGlobal(TypeInfoStructurePointer);
                                        //Le informazioni disponibili nella struttura OBJECT_TYPE_INFORMATION vengono trasformate in stringhe nel formato più adatto.
                                        string HandleType = TypeInfo.TypeName.Buffer;
                                        string HandleValue = "0x" + HandleInfo.HandleValue.ToString("X");
                                        string ObjectAddress = "0x" + HandleInfo.Object.ToString("X");
                                        string GrantedAccess = "0x" + HandleInfo.GrantedAccess.ToString("X", CultureInfo.InvariantCulture);
                                        //Determina quali diritti di accesso sono applicati all'handle.
                                        //List<string> AppliedRights = DetectAppliedAccessRights(HandleInfo.GrantedAccess, HandleType);
                                        string ReferencesCount = TypeInfo.TotalNumberOfObjects.ToString("N0", CultureInfo.InvariantCulture);
                                        string HandlesCount = TypeInfo.TotalNumberOfHandles.ToString("N0", CultureInfo.InvariantCulture);
                                        string PagedPoolUsage = ConvertUsageToString(TypeInfo.TotalPagedPoolUsage);
                                        string NonPagedPoolUsage = ConvertUsageToString(TypeInfo.TotalNonPagedPoolUsage);
                                        //Si tenta di recuperare il nome dell'handle, sono supportati i tipi seguenti:
                                        //File, process, thread, token, desktop, window station, key
                                        string HandleName = GetHandleName(NewHandle, HandleType, ProcessID.Value, ProcessName);
                                        HandleInfo Info = new(HandleType, HandleName, HandleValue, ObjectAddress, GrantedAccess, ReferencesCount, HandlesCount, PagedPoolUsage, NonPagedPoolUsage);
                                        HandlesInfoList.Add(Info);
                                    }
                                    else
                                    {
                                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un handle", EventAction.ProcessPropertiesRead, Handle, Result, ProcessID, ProcessName);
                                        Logger.WriteEntry(Entry);
                                        Marshal.FreeHGlobal(TypeInfoStructurePointer);
                                    }
                                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                                }
                                else
                                {
                                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile duplicare un handle", EventAction.ProcessPropertiesRead, Handle, Result, ProcessID, ProcessName);
                                    Logger.WriteEntry(Entry);
                                }
                            }
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'ID di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message, ProcessName: ProcessName);
                            Logger.WriteEntry(Entry);
                            Marshal.FreeHGlobal(StructurePointer);
                            return null;
                        }
                    }
                    Marshal.FreeHGlobal(StructurePointer);
                    HandlesInfoList.Sort((x, y) => x.Type.CompareTo(y.Type));
                    return HandlesInfoList.ToArray();
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni sugli handle aperti nel sistema", EventAction.ProcessPropertiesRead, null, Result);
                    Logger.WriteEntry(Entry);
                    Marshal.FreeHGlobal(StructurePointer);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sugli handle aperti da un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un handle.
        /// </summary>
        /// <param name="HandleValue">Valore numerico dell'handle.</param>
        /// <param name="ObjectAddress">Puntatore all'oggetto.</param>
        /// <param name="OwnerProcessHandle">Handle al processo proprietario.</param>
        /// <param name="ObjectName">Nome dell'oggetto, se disponibile.</param>
        /// <returns>Istanza di <see cref="HandleInfo"/> con le informazioni.</returns>
        public static HandleInfo GetHandleInfo(uint HandleValue, IntPtr ObjectAddress, SafeProcessHandle OwnerProcessHandle, string ObjectName = null)
        {
            HandleInfo Info;
            uint Result = Win32OtherFunctions.NtDuplicateObject(OwnerProcessHandle.DangerousGetHandle(), (IntPtr)HandleValue, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, 0, (uint)Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS);
            if (Result == Win32Constants.STATUS_SUCCESS)
            {
                uint Size = (uint)Marshal.SizeOf(typeof(Win32Structures.PUBLIC_OBJECT_BASIC_INFORMATION));
                IntPtr BasicInfoStructurePointer = Marshal.AllocHGlobal((int)Size);
                Result = Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectBasicInformation, BasicInfoStructurePointer, Size, out uint ReturnLength);
                if (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                {
                    Size = ReturnLength;
                    BasicInfoStructurePointer = Marshal.ReAllocHGlobal(BasicInfoStructurePointer, (IntPtr)Size);
                    Result = Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectBasicInformation, BasicInfoStructurePointer, Size, out _);
                }
                Win32Structures.PUBLIC_OBJECT_BASIC_INFORMATION BasicInfo = (Win32Structures.PUBLIC_OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(BasicInfoStructurePointer, typeof(Win32Structures.PUBLIC_OBJECT_BASIC_INFORMATION));
                Marshal.FreeHGlobal(BasicInfoStructurePointer);
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.OBJECT_TYPE_INFORMATION));
                IntPtr TypeInfoStructurePointer = Marshal.AllocHGlobal((int)Size);
                if (Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectTypeInformation, TypeInfoStructurePointer, Size, out ReturnLength) == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                {
                    Size = ReturnLength;
                    Marshal.FreeHGlobal(TypeInfoStructurePointer);
                    TypeInfoStructurePointer = Marshal.AllocHGlobal((int)Size);
                    Result = Win32OtherFunctions.NtQueryObject(NewHandle, Win32Enumerations.ObjectInformationClass.ObjectTypeInformation, TypeInfoStructurePointer, Size, out _);
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.OBJECT_TYPE_INFORMATION TypeInfo = (Win32Structures.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(TypeInfoStructurePointer, typeof(Win32Structures.OBJECT_TYPE_INFORMATION));
                        Marshal.FreeHGlobal(TypeInfoStructurePointer);
                        string HandleType = TypeInfo.TypeName.Buffer;
                        string HandleValueString = "0x" + HandleValue.ToString("X", CultureInfo.InvariantCulture);
                        string ObjectAddressString = "0x" + ObjectAddress.ToString("X");
                        string GrantedAccess = "0x" + BasicInfo.GrantedAccess.ToString("X", CultureInfo.InvariantCulture);
                        string ReferencesCount = TypeInfo.TotalNumberOfObjects.ToString("N0", CultureInfo.InvariantCulture);
                        string HandlesCount = TypeInfo.TotalNumberOfHandles.ToString("N0", CultureInfo.InvariantCulture);
                        string PagedPoolUsage = ConvertUsageToString(TypeInfo.TotalPagedPoolUsage);
                        string NonPagedPoolUsage = ConvertUsageToString(TypeInfo.TotalNonPagedPoolUsage);
                        //Si tenta di recuperare il nome dell'handle, sono supportati i tipi seguenti:
                        //File, process, thread, token, desktop, window station, key
                        if (ObjectName is not null)
                        {
                            Info = new(HandleType, ObjectName, HandleValueString, ObjectAddressString, GrantedAccess, ReferencesCount, HandlesCount, PagedPoolUsage, NonPagedPoolUsage);
                        }
                        else
                        {
                            uint? ProcessID = GetProcessPID(OwnerProcessHandle);
                            string ProcessName = GetProcessName(OwnerProcessHandle, ProcessID);
                            string HandleName = GetHandleName(NewHandle, HandleType, ProcessID.Value, ProcessName);
                            Info = new(HandleType, HandleName, HandleValueString, ObjectAddressString, GrantedAccess, ReferencesCount, HandlesCount, PagedPoolUsage, NonPagedPoolUsage);
                        }
                        _ = Win32OtherFunctions.CloseHandle(NewHandle);
                        return Info;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un handle", EventAction.ProcessPropertiesRead, OwnerProcessHandle, Result);
                        Logger.WriteEntry(Entry);
                        Marshal.FreeHGlobal(BasicInfoStructurePointer);
                        Marshal.FreeHGlobal(TypeInfoStructurePointer);
                        return null;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni sul tipo di un handle", EventAction.ProcessPropertiesRead, OwnerProcessHandle, Result);
                    Logger.WriteEntry(Entry);
                    Marshal.FreeHGlobal(BasicInfoStructurePointer);
                    Marshal.FreeHGlobal(TypeInfoStructurePointer);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile duplicare un handle", EventAction.ProcessPropertiesRead, OwnerProcessHandle, Result);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Converte il valore numerico dell'utilizzo di memoria di un handle in una stringa.
        /// </summary>
        /// <param name="Usage">Utilizzo di memoria.</param>
        /// <returns>La stringa risultato della conversione.</returns>
        private static string ConvertUsageToString(uint Usage)
        {
            string UsageString;
            double CalculatedValue;
            if (Usage is >= 1048576 and < 1073741824)
            {
                CalculatedValue = (double)Usage / 1024 / 1024;
                UsageString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            else if (Usage >= 1073741824)
            {
                CalculatedValue = (double)Usage / 1024 / 1024 / 1024;
                UsageString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            else if (Usage is >= 1024 and < 1048576)
            {
                CalculatedValue = (double)Usage / 1024;
                UsageString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
            }
            else
            {
                UsageString = Usage.ToString("N0", CultureInfo.CurrentCulture) + " B";
            }
            return UsageString;
        }

        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso applicati.</param>
        /// <param name="Type">Tipo di oggetto.</param>
        /// <returns>Una stringa che elenca i diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedAccessRights(uint GrantedAccess, string Type)
        {
            return Type switch
            {
                "Token" => DetectAppliedTokenAccessRights(GrantedAccess),
                "Desktop" => DetectAppliedDesktopAccessRights(GrantedAccess),
                "Event" or "Semaphore" or "Timer" => DetectAppliedSyncObjectAccessRights(GrantedAccess),
                "Mutant" => DetectAppliedMutexAccessRights(GrantedAccess),
                "Job" => DetectAppliedJobObjectAccessRights(GrantedAccess),
                "File" => DetectAppliedFileAccessRights(GrantedAccess),
                "Directory" => DetectAppliedDirectoryAccessRights(GrantedAccess),
                "Key" => DetectAppliedRegistryKeyAccessRights(GrantedAccess),
                "Process" => DetectAppliedProcessAccessRights(GrantedAccess),
                "Thread" => DetectAppliedThreadAccessRights(GrantedAccess),
                "WindowStation" => DetectAppliedWindowStationAccessRights(GrantedAccess),
                "Section" => DetectAppliedSectionAccessRights(GrantedAccess),
                _ => DetectStandardAccessRights(GrantedAccess),
            };
        }
        #region Access Rights Check Methods
        #region Standard Access Rights
        /// <summary>
        /// Determina quali diritti di accesso standard sono applicati a un handle.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che identifica i diritti di accesso.</param>
        /// <returns>Una lista con i diritti di accesso applicati.</returns>
        private static List<string> DetectStandardAccessRights(uint GrantedAccess)
        {
            List<string> Rights = new();
            Win32Enumerations.StandardAccessRights StandardAccessRights = (Win32Enumerations.StandardAccessRights)GrantedAccess;
            if (StandardAccessRights.HasFlag(Win32Enumerations.StandardAccessRights.STANDARD_RIGHTS_ALL))
            {
                Rights.Add("Delete");
                Rights.Add("Security descriptor read");
                Rights.Add("Change DACL");
                Rights.Add("Change owner");
                Rights.Add("Synchronize");
            }
            else
            {
                Win32Enumerations.GenericObjectAccessRights GenericAccessRights = (Win32Enumerations.GenericObjectAccessRights)GrantedAccess;
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.ACCESS_SYSTEM_SECURITY))
                {
                    Rights.Add("SACL read and write");
                }
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.DELETE))
                {
                    Rights.Add("Delete");
                }
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.READ_CONTROL))
                {
                    Rights.Add("Security descriptor read");
                }
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.WRITE_DAC))
                {
                    Rights.Add("Change DACL");
                }
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.WRITE_OWNER))
                {
                    Rights.Add("Change owner");
                }
                if (GenericAccessRights.HasFlag(Win32Enumerations.GenericObjectAccessRights.SYNCHRONIZE))
                {
                    Rights.Add("Synchronize");
                }
            }
            return Rights;
        }
        #endregion
        #region Token Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un token.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso applicati.</param>
        /// <returns>Una lista con i diritti di accesso applicati al token.</returns>
        private static List<string> DetectAppliedTokenAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.TokenAccessRights TokenAccessRights = (Win32Enumerations.TokenAccessRights)GrantedAccess;
            if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ALL_ACCESS))
            {
                Rights.Add("Adjust default");
                Rights.Add("Adjust groups");
                Rights.Add("Adjust privileges");
                Rights.Add("Adjust session ID");
                Rights.Add("Assign primary");
                Rights.Add("Duplicate");
                Rights.Add("Impersonation");
                Rights.Add("Query");
                Rights.Add("Query source");
            }
            else
            {
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ADJUST_DEFAULT))
                {
                    Rights.Add("Adjust default");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ADJUST_GROUPS))
                {
                    Rights.Add("Adjust groups");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ADJUST_PRIVILEGES))
                {
                    Rights.Add("Adjust privileges");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ADJUST_SESSIONID))
                {
                    Rights.Add("Adjust session ID");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_ASSIGN_PRIMARY))
                {
                    Rights.Add("Assign primary");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_DUPLICATE))
                {
                    Rights.Add("Duplicate");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_IMPERSONATE))
                {
                    Rights.Add("Impersonation");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_QUERY))
                {
                    Rights.Add("Query");
                }
                if (TokenAccessRights.HasFlag(Win32Enumerations.TokenAccessRights.TOKEN_QUERY_SOURCE))
                {
                    Rights.Add("Query source");
                }
            }
            return Rights;
        }
        #endregion
        #region Desktop Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un desktop.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso applicati.</param>
        /// <returns>Una lista con i diritti di accesso applicati al desktop.</returns>
        private static List<string> DetectAppliedDesktopAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.DesktopAccessRights DesktopAccessRights = (Win32Enumerations.DesktopAccessRights)GrantedAccess;
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_CREATEMENU))
            {
                Rights.Add("Create menu");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_CREATEWINDOW))
            {
                Rights.Add("Create window");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_ENUMERATE))
            {
                Rights.Add("Enumerate");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_HOOKCONTROL))
            {
                Rights.Add("Establish window hooks");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_JOURNALPLAYBACK))
            {
                Rights.Add("Journal playback");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_JOURNALRECORD))
            {
                Rights.Add("Journal record");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_READOBJECTS))
            {
                Rights.Add("Read objects");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_SWITCHDESKTOP))
            {
                Rights.Add("Switch desktop");
            }
            if (DesktopAccessRights.HasFlag(Win32Enumerations.DesktopAccessRights.DESKTOP_WRITEOBJECTS))
            {
                Rights.Add("Write objects");
            }
            return Rights;
        }
        #endregion
        #region Synchronization Objects Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un oggetto di sincronizzazione.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedSyncObjectAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.SyncObjectAccessRights SyncObjectAccessRights = (Win32Enumerations.SyncObjectAccessRights)GrantedAccess;
            if (SyncObjectAccessRights.HasFlag(Win32Enumerations.SyncObjectAccessRights.SYNC_OBJECT_MODIFY_STATE))
            {
                Rights.Add("Modify state");
            }
            return Rights;
        }

        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un mutex.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedMutexAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.MutexAccessRights SyncObjectAccessRights = (Win32Enumerations.MutexAccessRights)GrantedAccess;
            if (SyncObjectAccessRights.HasFlag(Win32Enumerations.MutexAccessRights.MUTANT_QUERY_STATE))
            {
                Rights.Add("Query state");
            }
            return Rights;
        }
        #endregion
        #region Job Objects Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un job.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedJobObjectAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.JobObjectAccessRights JobObjectAccessRights = (Win32Enumerations.JobObjectAccessRights)GrantedAccess;
            if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_ALL_ACCESS))
            {
                Rights.Add("Assign process");
                Rights.Add("Query");
                Rights.Add("Set attributes");
                Rights.Add("Set security attributes");
                Rights.Add("Terminate");
            }
            else
            {
                if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_ASSIGN_PROCESS))
                {
                    Rights.Add("Assign process");
                }
                if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_QUERY))
                {
                    Rights.Add("Query");
                }
                if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_SET_ATTRIBUTES))
                {
                    Rights.Add("Set attributes");
                }
                if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_SET_SECURITY_ATTRIBUTES))
                {
                    Rights.Add("Set security attributes");
                }
                if (JobObjectAccessRights.HasFlag(Win32Enumerations.JobObjectAccessRights.JOB_OBJECT_TERMINATE))
                {
                    Rights.Add("Terminate");
                }
            }
            return Rights;
        }
        #endregion
        #region File And Directory Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un file.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedFileAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.FileDirectoryAccessRights FileAccessRights = (Win32Enumerations.FileDirectoryAccessRights)GrantedAccess;
            if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_ALL_ACCESS))
            {
                Rights.Add("Append data");
                Rights.Add("Execute file");
                Rights.Add("Read attributes");
                Rights.Add("Read file");
                Rights.Add("Read extended attributes");
                Rights.Add("Write attributes");
                Rights.Add("Write file");
                Rights.Add("Write extended attributes");
            }
            else
            {
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_APPEND_DATA))
                {
                    Rights.Add("Append data");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_EXECUTE))
                {
                    Rights.Add("Execute file");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_READ_ATTRIBUTES))
                {
                    Rights.Add("Read attributes");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_READ_DATA))
                {
                    Rights.Add("Read file");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_READ_EA))
                {
                    Rights.Add("Read extended attributes");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_WRITE_ATTRIBUTES))
                {
                    Rights.Add("Write attributes");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_WRITE_DATA))
                {
                    Rights.Add("Write file");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_WRITE_EA))
                {
                    Rights.Add("Write extended attributes");
                }
            }
            return Rights;
        }

        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a una directory.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedDirectoryAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.FileDirectoryAccessRights FileAccessRights = (Win32Enumerations.FileDirectoryAccessRights)GrantedAccess;
            if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_ALL_ACCESS))
            {
                Rights.Add("Create file");
                Rights.Add("Create subdirectory");
                Rights.Add("Delete subdirectory");
                Rights.Add("List directory");
                Rights.Add("Traverse directory");
            }
            else
            {
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_ADD_FILE))
                {
                    Rights.Add("Create file");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_ADD_SUBDIRECTORY))
                {
                    Rights.Add("Create subdirectory");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_DELETE_CHILD))
                {
                    Rights.Add("Delete subdirectory");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_LIST_DIRECTORY))
                {
                    Rights.Add("List directory");
                }
                if (FileAccessRights.HasFlag(Win32Enumerations.FileDirectoryAccessRights.FILE_TRAVERSE))
                {
                    Rights.Add("Traverse directory");
                }
            }
            return Rights;
        }
        #endregion
        #region File Mapping Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un oggetto di file mapping.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedFileMappingAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.FileMappingAccessRightsAndFlags FileMappingAccessRights = (Win32Enumerations.FileMappingAccessRightsAndFlags)GrantedAccess;
            if (FileMappingAccessRights.HasFlag(Win32Enumerations.FileMappingAccessRightsAndFlags.FILE_MAP_ALL_ACCESS))
            {
                Rights.Add("Read mapping");
                Rights.Add("Write mapping");
            }
            else
            {
                if (FileMappingAccessRights.HasFlag(Win32Enumerations.FileMappingAccessRightsAndFlags.FILE_MAP_READ))
                {
                    Rights.Add("Read mapping");
                }
                if (FileMappingAccessRights.HasFlag(Win32Enumerations.FileMappingAccessRightsAndFlags.FILE_MAP_WRITE))
                {
                    Rights.Add("Write mapping");
                }
                if (FileMappingAccessRights.HasFlag(Win32Enumerations.FileMappingAccessRightsAndFlags.FILE_MAP_EXECUTE))
                {
                    Rights.Add("Execute mapping");
                }
            }
            return Rights;
        }
        #endregion
        #region Registry Key Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a una chiave di registro.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedRegistryKeyAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.RegistryKeyAccessRights KeyAccessRights = (Win32Enumerations.RegistryKeyAccessRights)GrantedAccess;
            if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_ALL_ACCESS))
            {
                Rights.Add("Create link");
                Rights.Add("Create subkey");
                Rights.Add("Enumerate subkeys");
                Rights.Add("Request change notifications");
                Rights.Add("Query value");
                Rights.Add("Set value");
            }
            else
            {
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_CREATE_LINK))
                {
                    Rights.Add("Create link");
                }
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_CREATE_SUB_KEY))
                {
                    Rights.Add("Create subkey");
                }
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_ENUMERATE_SUB_KEYS))
                {
                    Rights.Add("Enumerate subkeys");
                }
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_NOTIFY))
                {
                    Rights.Add("Request change norifications");
                }
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_QUERY_VALUE))
                {
                    Rights.Add("Query value");
                }
                if (KeyAccessRights.HasFlag(Win32Enumerations.RegistryKeyAccessRights.KEY_SET_VALUE))
                {
                    Rights.Add("Set value");
                }
            }
            return Rights;
        }
        #endregion
        #region Process And Threads Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un processo.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedProcessAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.ProcessAccessRights ProcessAccessRights = (Win32Enumerations.ProcessAccessRights)GrantedAccess;
            if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS))
            {
                Rights.Add("Create process");
                Rights.Add("Create thread");
                Rights.Add("Duplicate handle");
                Rights.Add("Query information");
                Rights.Add("Set information");
                Rights.Add("Set quota");
                Rights.Add("Suspend and resume");
                Rights.Add("Terminate");
                Rights.Add("Virtual memory operation");
                Rights.Add("Virtual memory read");
                Rights.Add("Virtual memory write");
            }
            else
            {
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_CREATE_PROCESS))
                {
                    Rights.Add("Create process");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_CREATE_THREAD))
                {
                    Rights.Add("Create thread");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_DUP_HANDLE))
                {
                    Rights.Add("Duplicate handle");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_INFORMATION))
                {
                    Rights.Add("Query information");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION))
                {
                    Rights.Add("Query limited information");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_SET_LIMITED_INFORMATION))
                {
                    Rights.Add("Set limited information");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_SET_INFORMATION))
                {
                    Rights.Add("Set information");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_SET_QUOTA))
                {
                    Rights.Add("Set quota");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_SUSPEND_RESUME))
                {
                    Rights.Add("Suspend and resume");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_TERMINATE))
                {
                    Rights.Add("Terminate");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_VM_OPERATION))
                {
                    Rights.Add("Virtual memory operation");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_VM_READ))
                {
                    Rights.Add("Virtual memory read");
                }
                if (ProcessAccessRights.HasFlag(Win32Enumerations.ProcessAccessRights.PROCESS_VM_WRITE))
                {
                    Rights.Add("Virtual memory write");
                }
            }
            return Rights;
        }

        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a un thread.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedThreadAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.ThreadAccessRights ThreadAccessRights = (Win32Enumerations.ThreadAccessRights)GrantedAccess;
            if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_ALL_ACCESS))
            {
                Rights.Add("Direct impersonation");
                Rights.Add("Get context");
                Rights.Add("Impersonate");
                Rights.Add("Query information");
                Rights.Add("Set context");
                Rights.Add("Set information");
                Rights.Add("Set thread token");
                Rights.Add("Suspend and resume");
                Rights.Add("Terminate");
            }
            else
            {
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_DIRECT_IMPERSONATION))
                {
                    Rights.Add("Direct impersonation");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_GET_CONTEXT))
                {
                    Rights.Add("Get context");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_IMPERSONATE))
                {
                    Rights.Add("Impersonate");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_DIRECT_IMPERSONATION))
                {
                    Rights.Add("Query information");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_LIMITED_INFORMATION))
                {
                    Rights.Add("Query limited information");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_SET_CONTEXT))
                {
                    Rights.Add("Set context");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_SET_INFORMATION))
                {
                    Rights.Add("Set information");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_SET_LIMITED_INFORMATION))
                {
                    Rights.Add("Set limited information");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_SET_THREAD_TOKEN))
                {
                    Rights.Add("Set impersonation token");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_SUSPEND_RESUME))
                {
                    Rights.Add("Suspend and resume");
                }
                if (ThreadAccessRights.HasFlag(Win32Enumerations.ThreadAccessRights.THREAD_TERMINATE))
                {
                    Rights.Add("Terminate");
                }

            }
            return Rights;
        }
        #endregion
        #region Window Station Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a una window station.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedWindowStationAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.WindowStationAccessRights WindowStationAccessRights = (Win32Enumerations.WindowStationAccessRights)GrantedAccess;
            if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_ALL_ACCESS))
            {
                Rights.Add("Clipboard usage");
                Rights.Add("Global atoms access");
                Rights.Add("Create desktop");
                Rights.Add("Desktops enumeration");
                Rights.Add("Enumerate window station");
                Rights.Add("Exit windows");
                Rights.Add("Read screen");
                Rights.Add("Read attributes");
                Rights.Add("Write attributes");
            }
            else
            {
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_ACCESSCLIPBOARD))
                {
                    Rights.Add("Clipboard usage");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_ACCESSGLOBALATOMS))
                {
                    Rights.Add("Global atoms access");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_CREATEDESKTOP))
                {
                    Rights.Add("Create desktop");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_ENUMDESKTOPS))
                {
                    Rights.Add("Desktops enumeration");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_ENUMERATE))
                {
                    Rights.Add("Enumerate window station");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_EXITWINDOWS))
                {
                    Rights.Add("Exit windows");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_READATTRIBUTES))
                {
                    Rights.Add("Read attributes");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_READSCREEN))
                {
                    Rights.Add("Read screen");
                }
                if (WindowStationAccessRights.HasFlag(Win32Enumerations.WindowStationAccessRights.WINSTA_WRITEATTRIBUTES))
                {
                    Rights.Add("Write attributes");
                }
            }
            return Rights;
        }
        #endregion
        #region Section Access Rights
        /// <summary>
        /// Determina quali diritti di accesso sono applicati a un handle a una sezione.
        /// </summary>
        /// <param name="GrantedAccess">Valore numerico che indica i diritti di accesso.</param>
        /// <returns>Una lista che elenca di diritti di accesso applicati.</returns>
        private static List<string> DetectAppliedSectionAccessRights(uint GrantedAccess)
        {
            List<string> Rights = DetectStandardAccessRights(GrantedAccess);
            Win32Enumerations.SectionAccessRights SectionAccessRights = (Win32Enumerations.SectionAccessRights)GrantedAccess;
            if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_ALL_ACCESS))
            {
                Rights.Add("Query");
                Rights.Add("Map write");
                Rights.Add("Map read");
                Rights.Add("Map execute");
                Rights.Add("Extend size");
            }
            else
            {
                if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_QUERY))
                {
                    Rights.Add("Query");
                }
                if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_MAP_WRITE))
                {
                    Rights.Add("Map write");
                }
                if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_MAP_READ))
                {
                    Rights.Add("Map read");
                }
                if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_MAP_EXECUTE) || SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_MAP_EXECUTE_EXPLICIT))
                {
                    Rights.Add("Map execute");
                }
                if (SectionAccessRights.HasFlag(Win32Enumerations.SectionAccessRights.SECTION_EXTEND_SIZE))
                {
                    Rights.Add("Extend size");
                }
            }
            return Rights;
        }
        #endregion
        #endregion
        /// <summary>
        /// Recupera il nome di un handle.
        /// </summary>
        /// <param name="Handle">Handle nativo all'oggetto.</param>
        /// <param name="HandleType">Tipo di handle.</param>
        /// <returns>Il nome dell'handle.</returns>
        private static string GetHandleName(IntPtr Handle, string HandleType, uint PID, string ProcessName)
        {
            StringBuilder Path;
            uint BufferSize = 0;
            uint Result;
            string NameString;
            SafeProcessHandle SafeHandle;
            IntPtr Buffer;
            IntPtr SecondBuffer;
            byte[] StringBytes;
            switch (HandleType)
            {
                case "File":
                    BufferSize = Win32FileFunctions.GetFinalPathNameByHandle(Handle, null, BufferSize, Win32Enumerations.FileResultType.VOLUME_NAME_DOS);
                    Path = new StringBuilder((int)BufferSize);
                    Result = Win32FileFunctions.GetFinalPathNameByHandle(Handle, Path, BufferSize, Win32Enumerations.FileResultType.VOLUME_NAME_DOS);
                    if (Result == 0)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il percorso completo di un file", EventAction.HandlePropertiesRead, null, ex.NativeErrorCode, ex.Message, PID, ProcessName);
                        Logger.WriteEntry(Entry);
                        return Properties.Resources.UnavailableText;
                    }
                    else
                    {
                        return Path.ToString().Replace("\\\\?\\", string.Empty);
                    }
                case "Process":
                    using (SafeHandle = new SafeProcessHandle(Handle, false))
                    {
                        if (!SafeHandle.IsInvalid)
                        {
                            if (ProcessName != Properties.Resources.UnavailableText && PID != 0)
                            {
                                return ProcessName + " (" + PID.ToString("D0", CultureInfo.CurrentCulture) + ")";
                            }
                            else
                            {
                                if (ProcessName == Properties.Resources.UnavailableText && PID != 0)
                                {
                                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un processo", EventAction.HandlePropertiesRead, SafeHandle, PID, ProcessName);
                                    Logger.WriteEntry(Entry);
                                }
                                else if (PID == 0)
                                {
                                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un processo, PID non disponibile", EventAction.HandlePropertiesRead, SafeHandle, ProcessName: ProcessName);
                                    Logger.WriteEntry(Entry);
                                }
                                return Properties.Resources.UnavailableText;
                            }
                        }
                        else
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un processo, handle non valido", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                            return Properties.Resources.UnavailableText;
                        }
                    }
                case "Thread":
                    uint TID = Win32ProcessFunctions.GetThreadID(Handle);
                    if (TID != 0 && PID != 0)
                    {
                        return ProcessName + " (" + PID.ToString("D0", CultureInfo.CurrentCulture) + ") : " + TID.ToString("D0", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        if (TID == 0 && PID != 0)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un thread, TID non disponibile", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        else if (TID != 0 && PID == 0)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un thread, PID non disponibile", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        else if (TID == 0 && PID == 0)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un thread, TID e PID non disponibili", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        return Properties.Resources.UnavailableText;
                    }
                case "Token":
                    GetTokenAssociatedUserInfo(Handle, out string Username, out _);
                    TokenStatistics Statistics = GetTokenStatistics(Handle);
                    if (Username != Properties.Resources.UnavailableText && Statistics != null)
                    {
                        return Username + ": " + Statistics.AuthenticationLUID + " (" + Statistics.Type + ")";
                    }
                    else
                    {
                        if (Username == Properties.Resources.UnavailableText && Statistics != null)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni da un token, nome utente associato non disponibile", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        else if (Username != Properties.Resources.UnavailableText && Statistics == null)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni da un token, statistiche del token non disponibili", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        else if (Username == Properties.Resources.UnavailableText && Statistics == null)
                        {
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni da un token, nome utente associato e statistiche del token non disponibili", EventAction.HandlePropertiesRead, null);
                            Logger.WriteEntry(Entry);
                        }
                        return Properties.Resources.UnavailableText;
                    }
                case "Desktop":
                case "WindowStation":
                    if (!Win32OtherFunctions.GetUserObjectInformation(Handle, Win32Enumerations.UserObjectInfo.UOI_NAME, IntPtr.Zero, 0, out uint LengthNeeded))
                    {
                        Buffer = Marshal.AllocHGlobal((int)LengthNeeded);
                        if (Win32OtherFunctions.GetUserObjectInformation(Handle, Win32Enumerations.UserObjectInfo.UOI_NAME, Buffer, LengthNeeded, out _))
                        {
                            NameString = Marshal.PtrToStringUni(Buffer);
                            Marshal.FreeHGlobal(Buffer);
                            return NameString;
                        }
                        else
                        {
                            if (HandleType == "Desktop")
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di un desktop", EventAction.HandlePropertiesRead, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                            }
                            else if (HandleType == "WindowStation")
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di una window station", EventAction.HandlePropertiesRead, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                            }
                            return Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                case "Key":
                    Result = Win32OtherFunctions.NtQueryKey(Handle, Win32Enumerations.KeyinformationClass.KeyNameInformation, IntPtr.Zero, 0, out uint ResultLength);
                    if (Result is Win32Constants.STATUS_BUFFER_OVERFLOW or Win32Constants.STATUS_BUFFER_TOO_SMALL)
                    {
                        Buffer = Marshal.AllocHGlobal((int)ResultLength);
                        Result = Win32OtherFunctions.NtQueryKey(Handle, Win32Enumerations.KeyinformationClass.KeyNameInformation, Buffer, ResultLength, out _);
                        if (Result == Win32Constants.STATUS_SUCCESS)
                        {
                            IntPtr CurrentProcessTokenHandle = GetCurrentProcessTokenForQuery();
                            if (CurrentProcessTokenHandle != IntPtr.Zero)
                            {
                                GetTokenAssociatedUserInfo(CurrentProcessTokenHandle, out _, out string UserSID);
                                if (UserSID != Properties.Resources.UnavailableText)
                                {
                                    SecondBuffer = Buffer;
                                    int NameSize = Marshal.ReadInt32(Buffer);
                                    SecondBuffer += 4;
                                    StringBytes = new byte[NameSize];
                                    for (int i = 0; i < NameSize; i++)
                                    {
                                        StringBytes[i] = Marshal.ReadByte(SecondBuffer);
                                        SecondBuffer += 1;
                                    }
                                    NameString = Encoding.Unicode.GetString(StringBytes);
                                    if (NameString.Contains("REGISTRY\\MACHINE"))
                                    {
                                        NameString = NameString.Replace("REGISTRY\\MACHINE", "HKLM");
                                    }
                                    else if (NameString.Contains("REGISTRY\\USER\\" + UserSID))
                                    {
                                        NameString = NameString.Replace("REGISTRY\\USER\\" + UserSID, "HKCU");
                                    }
                                    else if (NameString.StartsWith("\\REGISTRY\\USER", StringComparison.CurrentCulture) && !NameString.Contains(UserSID))
                                    {
                                        NameString = NameString.Replace("REGISTRY\\USER", "HKU");
                                    }
                                    else
                                    {
                                        NameString = Properties.Resources.UnavailableText;
                                    }
                                    if (NameString != Properties.Resources.UnavailableText)
                                    {
                                        NameString = NameString.Remove(0, 1);
                                    }
                                    _ = Win32OtherFunctions.CloseHandle(CurrentProcessTokenHandle);
                                    Marshal.FreeHGlobal(Buffer);
                                    return NameString;
                                }
                                else
                                {
                                    _ = Win32OtherFunctions.CloseHandle(CurrentProcessTokenHandle);
                                    Marshal.FreeHGlobal(Buffer);
                                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di una chiave di registro, impossibile recuperare il nome dell'account corrente", EventAction.HandlePropertiesRead, null);
                                    Logger.WriteEntry(Entry);
                                    return Properties.Resources.UnavailableText;
                                }
                            }
                            else
                            {
                                Marshal.FreeHGlobal(Buffer);
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di una chiave di registro, impossibile aprire il token del processo corrente", EventAction.HandlePropertiesRead, null);
                                Logger.WriteEntry(Entry);
                                return Properties.Resources.UnavailableText;
                            }
                        }
                        else
                        {
                            Marshal.FreeHGlobal(Buffer);
                            LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare il nome di una chiave di registro", EventAction.HandlePropertiesRead, null, Result);
                            Logger.WriteEntry(Entry);
                            return Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare il nome di una chiave di registro", EventAction.HandlePropertiesRead, null, Result);
                        Logger.WriteEntry(Entry);
                        return Properties.Resources.UnavailableText;
                    }
                default:
                    return Properties.Resources.UnavailableText;
            }
        }
        #endregion
        #endregion
        #region Detailed Properties (Threads)
        /// <summary>
        /// Recupera informazioni sui thread di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un dizionario con le informazioni sui thread del processo.</returns>
        public static ObservableCollection<ThreadInfo> GetProcessThreadsInfo(SafeProcessHandle Handle, uint ProcessID)
        {
            Contract.Requires(Handle != null);
            IntPtr ThreadHandle;
            ThreadInfo ThreadInfos;
            ObservableCollection<ThreadInfo> ThreadsInfo = new();
            if (ProcessID == 0)
            {
                ProcessID = GetProcessPID(Handle);
            }
            if (ProcessID == 0)
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sui thread di un processo, ID del processo non disponibile", EventAction.ThreadEnumeration, Handle);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                IntPtr SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPTHREAD, ProcessID);
                if (SnapshotHandle != IntPtr.Zero)
                {
                    Win32Structures.THREADENTRY32 ThreadInfo = new();
                    ThreadInfo.StructureSize = Convert.ToUInt32(Marshal.SizeOf(ThreadInfo));
                    if (!Win32ProcessFunctions.Thread32First(SnapshotHandle, ref ThreadInfo))
                    {
                        _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema", EventAction.ThreadEnumeration, Handle, ex.NativeErrorCode, ex.Message, ProcessID);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                    else
                    {
                        if (ProcessID == ThreadInfo.OwnerPID)
                        {
                            ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_ALL_ACCESS, false, ThreadInfo.ThreadID);
                            if (ThreadHandle != IntPtr.Zero)
                            {
                                ThreadInfos = GetThreadInfo(ThreadHandle, ThreadInfo);
                                ThreadsInfo.Add(ThreadInfos);
                            }
                            else
                            {
                                ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_LIMITED_INFORMATION, false, ThreadInfo.ThreadID);
                                if (ThreadHandle != IntPtr.Zero)
                                {
                                    ThreadInfos = GetThreadInfo(ThreadHandle, ThreadInfo);
                                    ThreadsInfo.Add(ThreadInfos);
                                }
                                else
                                {
                                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un thread", EventAction.ThreadEnumeration, Handle, ex.NativeErrorCode, ex.Message, ProcessID);
                                    Logger.WriteEntry(Entry);
                                    return null;
                                }
                            }
                        }
                        bool End = false;
                        while (!End)
                        {
                            if (!Win32ProcessFunctions.Thread32Next(SnapshotHandle, ref ThreadInfo))
                            {
                                int ErrorCode = Marshal.GetLastWin32Error();
                                if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                                {
                                    End = true;
                                }
                                else
                                {
                                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                    Win32Exception ex = new(ErrorCode);
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread in uno snapshot del sistema", EventAction.ThreadEnumeration, Handle, ex.NativeErrorCode, ex.Message, ProcessID);
                                    Logger.WriteEntry(Entry);
                                    foreach (ThreadInfo info in ThreadsInfo)
                                    {
                                        info.Dispose();
                                    }
                                    return null;
                                }
                            }
                            else
                            {
                                if (ProcessID == ThreadInfo.OwnerPID)
                                {
                                    ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_ALL_ACCESS, false, ThreadInfo.ThreadID);
                                    if (ThreadHandle != IntPtr.Zero)
                                    {
                                        ThreadInfos = GetThreadInfo(ThreadHandle, ThreadInfo);
                                        ThreadsInfo.Add(ThreadInfos);
                                    }
                                    else
                                    {
                                        ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_LIMITED_INFORMATION, false, ThreadInfo.ThreadID);
                                        if (ThreadHandle != IntPtr.Zero)
                                        {
                                            ThreadInfos = GetThreadInfo(ThreadHandle, ThreadInfo);
                                            ThreadsInfo.Add(ThreadInfos);
                                        }
                                        else
                                        {
                                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un thread", EventAction.ThreadEnumeration, Handle, ex.NativeErrorCode, ex.Message, ProcessID);
                                            Logger.WriteEntry(Entry);
                                        }
                                    }
                                }
                            }

                        }
                        _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                        return ThreadsInfo;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile eseguire uno snapshot del sistema", EventAction.ThreadEnumeration, Handle, ex.NativeErrorCode, ex.Message, ProcessID);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
        }

        /// <summary>
        /// Recupera informazioni su un thread.
        /// </summary>
        /// <param name="ThreadHandle">Handle al thread.</param>
        /// <param name="ThreadInfo">Struttura <see cref="Win32Structures.THREADENTRY32"/> che contiene alcune informazioni sul thread.</param>
        /// <returns>Un'istanza di <see cref="ThreadInfo"/> con le informazioni.</returns>
        private static ThreadInfo GetThreadInfo(IntPtr ThreadHandle, Win32Structures.THREADENTRY32 ThreadInfo)
        {
            (DateTime? ThreadCreationTime, TimeSpan? ThreadKernelTime, TimeSpan? ThreadUserTime) = GetThreadTimes(ThreadHandle);
            uint? DynamicPriority = GetThreadDynamicPriority(ThreadInfo.ThreadID);
            string StartAddress = GetThreadStartAddress(ThreadHandle) ?? Properties.Resources.UnavailableText;
            string BasePriority = ThreadInfo.BasePriority.ToString("N0", CultureInfo.CurrentCulture);
            string CycleTimeString;
            if (Win32ProcessFunctions.QueryThreadCycleTime(ThreadHandle, out ulong CycleTime))
            {
                CycleTimeString = CycleTime.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile reucperare il numero di cicli di esecuzione di un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                CycleTimeString = Properties.Resources.UnavailableText;
            }
            string PriorityString = GetThreadPriorityString(ThreadHandle);
            string DynamicPriorityString = DynamicPriority.HasValue ? DynamicPriority.Value.ToString("N0", CultureInfo.InvariantCulture) : Properties.Resources.UnavailableText;
            string IdealProcessor = GetThreadIdealProcessor(ThreadHandle) ?? Properties.Resources.UnavailableText;
            string CreationTime = ThreadCreationTime.HasValue ? ThreadCreationTime.Value.ToString(CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string KernelTime = ThreadKernelTime.HasValue ? ThreadKernelTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string UserTime = ThreadUserTime.HasValue ? ThreadUserTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            return new ThreadInfo(ThreadHandle, ThreadInfo.ThreadID, BasePriority, CycleTimeString, CreationTime, KernelTime, UserTime, StartAddress, PriorityString, GetThreadAffinity(ThreadHandle) ?? 0, DynamicPriorityString, IdealProcessor);
        }

        /// <summary>
        /// Recupera informazioni su un thread.
        /// </summary>
        ///<param name="PID">ID del processo associato al thread.</param>
        ///<param name="TID">ID del thread.</param>
        /// <returns>Un'istanza di <see cref="ThreadInfo"/> con le informazioni.</returns>
        public static ThreadInfo GetThreadInfo(uint TID, uint PID)
        {
            IntPtr ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_INFORMATION, false, TID);
            string BasePriority = WMIProcessInfoMethods.GetThreadBasePriority(TID, PID).ToString("N0", CultureInfo.InvariantCulture);
            string StartAddress = GetThreadStartAddress(ThreadHandle);
            (DateTime? ThreadCreationTime, TimeSpan? ThreadKernelTime, TimeSpan? ThreadUserTime) = GetThreadTimes(ThreadHandle);
            uint? DynamicPriority = GetThreadDynamicPriority(TID);
            string CycleTimeString;
            if (Win32ProcessFunctions.QueryThreadCycleTime(ThreadHandle, out ulong CycleTime))
            {
                CycleTimeString = CycleTime.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile reucperare il numero di cicli di esecuzione di un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                CycleTimeString = Properties.Resources.UnavailableText;
            }
            string PriorityString = GetThreadPriorityString(ThreadHandle);
            string DynamicPriorityString = DynamicPriority.HasValue ? DynamicPriority.Value.ToString("N0", CultureInfo.InvariantCulture) : Properties.Resources.UnavailableText;
            string IdealProcessor = GetThreadIdealProcessor(ThreadHandle) ?? Properties.Resources.UnavailableText;
            string CreationTime = ThreadCreationTime.HasValue ? ThreadCreationTime.Value.ToString(CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string KernelTime = ThreadKernelTime.HasValue ? ThreadKernelTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string UserTime = ThreadUserTime.HasValue ? ThreadUserTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            return new ThreadInfo(ThreadHandle, TID, BasePriority, CycleTimeString, CreationTime, KernelTime, UserTime, StartAddress, PriorityString, GetThreadAffinity(ThreadHandle) ?? 0, DynamicPriorityString, IdealProcessor);
        }

        /// <summary>
        /// Recupera informazioni dinamiche su un thread.
        /// </summary>
        ///<param name="TID">ID del thread.</param>
        /// <returns>Un'istanza di <see cref="ThreadInfo"/> con le informazioni.</returns>
        public static ThreadInfo GetThreadDynamicInfo(uint TID)
        {
            IntPtr ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_INFORMATION, false, TID);
            (DateTime? ThreadCreationTime, TimeSpan? ThreadKernelTime, TimeSpan? ThreadUserTime) = GetThreadTimes(ThreadHandle);
            uint? DynamicPriority = GetThreadDynamicPriority(TID);
            string CycleTimeString;
            if (Win32ProcessFunctions.QueryThreadCycleTime(ThreadHandle, out ulong CycleTime))
            {
                CycleTimeString = CycleTime.ToString("N0", CultureInfo.CurrentCulture);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile reucperare il numero di cicli di esecuzione di un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                CycleTimeString = Properties.Resources.UnavailableText;
            }
            string PriorityString = GetThreadPriorityString(ThreadHandle);
            string DynamicPriorityString = DynamicPriority.HasValue ? DynamicPriority.Value.ToString("N0", CultureInfo.InvariantCulture) : Properties.Resources.UnavailableText;
            string IdealProcessor = GetThreadIdealProcessor(ThreadHandle) ?? Properties.Resources.UnavailableText;
            string CreationTime = ThreadCreationTime.HasValue ? ThreadCreationTime.Value.ToString(CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string KernelTime = ThreadKernelTime.HasValue ? ThreadKernelTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            string UserTime = ThreadUserTime.HasValue ? ThreadUserTime.Value.ToString(@"d\:hh\:mm\:ss", CultureInfo.CurrentCulture) : Properties.Resources.UnavailableText;
            ulong? ThreadAffinity = GetThreadAffinity(ThreadHandle);
            _ = Win32OtherFunctions.CloseHandle(ThreadHandle);
            return new ThreadInfo(IntPtr.Zero, TID, null, CycleTimeString, CreationTime, KernelTime, UserTime, null, PriorityString, ThreadAffinity ?? 0, DynamicPriorityString, IdealProcessor);
        }

        /// <summary>
        /// Recupera i tempi di esecuzione del thread e la sua data di creazione.
        /// </summary>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>Una tupla con la data di creazione, il tempo di esecuzione kernel e il termpo di esecuzione utente del thread, i componenti della tupla possono essere nulli se l'informazione non è disponibile.</returns>
        private static (DateTime? StartDate, TimeSpan? KernelTime, TimeSpan? UserTime) GetThreadTimes(IntPtr ThreadHandle)
        {
            if (Win32ProcessFunctions.GetThreadTimes(ThreadHandle, out Win32Structures.FILETIME CreationTime, out _, out Win32Structures.FILETIME KernelTime, out Win32Structures.FILETIME UserTime))
            {
                DateTime StartDate = FileTimeToDateTime(CreationTime);
                TimeSpan KernelExecutionTime = TimeSpan.FromTicks(FileTimeToInt64(KernelTime));
                TimeSpan UserExecutionTime = TimeSpan.FromTicks(FileTimeToInt64(UserTime));
                return (StartDate, KernelExecutionTime, UserExecutionTime);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le termpistiche di un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return (null, null, null);
            }
        }

        /// <summary>
        /// Recupera la priorità del thread come stringa
        /// </summary>
        /// <param name="Handle">Handle nativo al thread.</param>
        /// <returns>Una stringa che rappresenta la priorità del thread.</returns>
        private static string GetThreadPriorityString(IntPtr Handle)
        {
            int Priority = Win32ProcessFunctions.GetThreadPriority(Handle);
            if (Priority == Win32Constants.THREAD_PRIORITY_ERROR_RETURN)
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la priorità di un thread", EventAction.ThreadPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
            else
            {
                return Priority switch
                {
                    1 => "AboveNormal",
                    -1 => "BelowNormal",
                    2 => "Highest",
                    -15 => "Idle",
                    -2 => "Lowest",
                    0 => "Normal",
                    -7 or -6 or -5 or -4 or -3 or 3 or 4 or 5 or 6 or 15 => "TimeCritical",
                    _ => "Unknown"
                };
            }
        }

        /// <summary>
        /// Recupera il processore ideale per il thread.
        /// </summary>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>Una stringa che rappresenta processore ideale del thread (gruppo:numero), null se l'operazione è fallita.</returns>
        private static string GetThreadIdealProcessor(IntPtr ThreadHandle)
        {
            if (Win32ProcessFunctions.GetThreadIdealProcessorEx(ThreadHandle, out Win32Structures.PROCESSOR_NUMBER IdealProcessor))
            {
                return IdealProcessor.Group.ToString("N0", CultureInfo.InvariantCulture) + ":" + IdealProcessor.Number.ToString("N0", CultureInfo.InvariantCulture);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il processore ideale per un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera l'indirizzo di base di un thread.
        /// </summary>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>Una string che rappresenta l'indirizzo di memoria di base del thread.</returns>
        private static string GetThreadStartAddress(IntPtr ThreadHandle)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(IntPtr.Size);
            string StartAddress;
            uint Result = Win32ProcessFunctions.NtQueryInformationThread(ThreadHandle, Win32Enumerations.ThreadInformationClass.ThreadQuerySetWin32StartAddress, Buffer, (uint)IntPtr.Size, out _);
            if (Result == 0)
            {
                StartAddress = "0x" + Marshal.ReadIntPtr(Buffer).ToString("X");
                Marshal.FreeHGlobal(Buffer);
                return StartAddress;
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare l'indirizzo di memoria di avvio di un thread", EventAction.ThreadPropertiesRead, null, Result);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera la priorità dinamica di un thread.
        /// </summary>
        /// <param name="TID">ID del thread.</param>
        /// <returns>La priorità dinamica del thread, nullo in caso di errore.</returns>
        private static uint? GetThreadDynamicPriority(uint TID)
        {
            IntPtr ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_INFORMATION, false, TID);
            if (ThreadHandle != IntPtr.Zero)
            {
                IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.THREAD_BASIC_INFORMATION)));
                uint Result = Win32ProcessFunctions.NtQueryInformationThread(ThreadHandle, Win32Enumerations.ThreadInformationClass.ThreadBasicInformation, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.THREAD_BASIC_INFORMATION)), out _);
                if (Result == 0)
                {
                    Win32Structures.THREAD_BASIC_INFORMATION BasicInfo = (Win32Structures.THREAD_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.THREAD_BASIC_INFORMATION));
                    Marshal.FreeHGlobal(Buffer);
                    Win32OtherFunctions.CloseHandle(ThreadHandle);
                    return (uint)BasicInfo.Priority;
                }
                else
                {
                    Marshal.FreeHGlobal(Buffer);
                    Win32OtherFunctions.CloseHandle(ThreadHandle);
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare le informazioni di base su un thread", EventAction.ThreadPropertiesRead, null, Result);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un thread", EventAction.ThreadPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #region Detailed Properties (Modules)
        /// <summary>
        /// Recupera informazioni sui moduli caricati da un processo.
        /// </summary>
        /// <param name="PID">ID del processo.</param>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>Una lista di istanze di <see cref="ModuleInfo"/> con le informazioni.</returns>
        public static List<ModuleInfo> GetProcessModulesInfo(uint PID, SafeProcessHandle ProcessHandle)
        {
            Contract.Requires(ProcessHandle != null);
            List<ModuleInfo> ModulesInfo = new();
            IntPtr SnapshotHandle = IntPtr.Zero;
            int SnapshotAttempts = 0;
            while (SnapshotAttempts < 3)
            {
                SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPMODULE | Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPMODULE32, PID);
                if (SnapshotHandle == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == Win32Constants.ERROR_BAD_LENGTH)
                    {
                        SnapshotAttempts += 1;
                    }
                }
                else
                {
                    break;
                }
            }
            if (SnapshotHandle == IntPtr.Zero)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile eseguire uno snapshot del sistema", EventAction.ModulesEnumeration, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Win32Structures.MODULEENTRY32 ModuleInfo = new()
                {
                    Size = (uint)Marshal.SizeOf(typeof(Win32Structures.MODULEENTRY32))
                };
                if (!Win32ProcessFunctions.Module32First(SnapshotHandle, ref ModuleInfo))
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un modulo caricato da un processo", EventAction.ModulesEnumeration, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                else
                {
                    StringBuilder sb = new(260);
                    if (Win32ProcessFunctions.GetModuleFileName(ProcessHandle.DangerousGetHandle(), ModuleInfo.ModuleHandle, sb, 260) != 0)
                    {
                        FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(sb.ToString());
                        ModulesInfo.Add(new(sb.ToString(), "0x" + ModuleInfo.ModuleBaseAddress.ToString("X"), ModuleInfo.ModuleBaseSize, VersionInfo.FileDescription));
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il percorso di un modulo", EventAction.ModulesPropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                bool End = false;
                while (!End)
                {
                    if (!Win32ProcessFunctions.Module32Next(SnapshotHandle, ref ModuleInfo))
                    {
                        int ErrorCode = Marshal.GetLastWin32Error();
                        if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                        {
                            End = true;
                        }
                        else
                        {
                            Win32Exception ex = new(ErrorCode);
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un modulo caricato da un processo", EventAction.ModulesEnumeration, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                            Logger.WriteEntry(Entry);
                            return null;
                        }
                    }
                    else
                    {
                        StringBuilder sb = new(260);
                        if (Win32ProcessFunctions.GetModuleFileName(ProcessHandle.DangerousGetHandle(), ModuleInfo.ModuleHandle, sb, 260) != 0)
                        {
                            if (File.Exists(sb.ToString()))
                            {
                                FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(sb.ToString());
                                ModulesInfo.Add(new(sb.ToString(), "0x" + ModuleInfo.ModuleBaseAddress.ToString("X"), ModuleInfo.ModuleBaseSize, VersionInfo.FileDescription));
                            }
                            else
                            {
                                ModulesInfo.Add(new(sb.ToString(), "0x" + ModuleInfo.ModuleBaseAddress.ToString("X"), ModuleInfo.ModuleBaseSize, Properties.Resources.UnavailableText));
                            }
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il percorso di un modulo", EventAction.ModulesPropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message, PID);
                            Logger.WriteEntry(Entry);
                            return null;
                        }
                    }
                }
                string ProcessName = GetProcessName(ProcessHandle, PID);
                ModuleInfo MainModule = ModulesInfo.Find(module => module.Name.Equals(ProcessName, StringComparison.OrdinalIgnoreCase));
                _ = ModulesInfo.Remove(MainModule);
                ModulesInfo.Sort((x, y) => x.Name.CompareTo(y.Name));
                ModulesInfo.Insert(0, MainModule);
                return ModulesInfo;
            }
        }

        /// <summary>
        /// Apre la finestra delle proprietà di un modulo.
        /// </summary>
        /// <param name="FullPath">Percorso del modulo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool OpenModulePropertiesWindow(string FullPath)
        {
            bool Result = Win32COMFunctions.SHObjectProperties(IntPtr.Zero, Win32Enumerations.PropertiesObjectType.SHOP_FILEPATH, FullPath, null);
            if (!Result)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile visualizza le proprietà di un modulo", EventAction.ModulesPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            return Result;
        }

        /// <summary>
        /// Trova il percorso completo di un modulo.
        /// </summary>
        /// <param name="Handle">Handle al processo che ha caricato il modulo.</param>
        /// <param name="ModuleName">Nome del modulo.</param>
        /// <param name="ModuleSize">Dimensione del modulo.</param>
        /// <param name="Checksum">Checksum del modulo.</param>
        /// <param name="ProcessFullPath">Percorso completo del processo.</param>
        /// <returns>Il percorso completo del modulo, nullo se il modulo non è stato trovato.</returns>
        /// <remarks>Questo metodo ricerca il modulo nelle stesse directory dove la funzione LoadLibrary esegue la ricerca.</remarks>
        public static string FindModulePath(SafeProcessHandle Handle, string ModuleName, uint ModuleSize, uint Checksum, string ProcessFullPath)
        {
            DirectoryInfo SearchDirectory = new(ProcessFullPath);
            System.IO.FileInfo Module = SearchDirectory.EnumerateFiles(ModuleName, SearchOption.AllDirectories).FirstOrDefault();
            if (Module is not null)
            {
                if (IsCorrectModule(Module, ModuleSize, Checksum))
                {
                    return Module.FullName;
                }
            }
            SearchDirectory = new(GetProcessCurrentDirectory(Handle));
            if (SearchDirectory.FullName != ProcessFullPath)
            {
                Module = SearchDirectory.EnumerateFiles(ModuleName, SearchOption.AllDirectories).FirstOrDefault();
            }
            if (Module is not null)
            {
                if (IsCorrectModule(Module, ModuleSize, Checksum))
                {
                    return Module.FullName;
                }
            }
            StringBuilder WindowsDirectoryPathBuffer = new((int)Win32Constants.MAX_PATH);
            uint Result = Win32ComputerInfoFunctions.GetWindowsDirectory(WindowsDirectoryPathBuffer, Win32Constants.MAX_PATH);
            if (Result is not 0)
            {
                SearchDirectory = new(WindowsDirectoryPathBuffer.ToString());
                Module = SearchDirectory.EnumerateFiles(ModuleName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (Module is not null)
                {
                    if (IsCorrectModule(Module, ModuleSize, Checksum))
                    {
                        return Module.FullName;
                    }
                }
            }
            _ = WindowsDirectoryPathBuffer.Clear();
            Result = Win32ComputerInfoFunctions.GetSystemDirectory(WindowsDirectoryPathBuffer, Win32Constants.MAX_PATH);
            if (Result is not 0)
            {
                SearchDirectory = new(WindowsDirectoryPathBuffer.ToString());
                Module = SearchDirectory.EnumerateFiles(ModuleName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (Module is not null)
                {
                    if (IsCorrectModule(Module, ModuleSize, Checksum))
                    {
                        return Module.FullName;
                    }
                }
            }
            string PATHVariable = Environment.GetEnvironmentVariable("PATH");
            string[] Paths = PATHVariable.Split(';');
            foreach (string path in Paths)
            {
                SearchDirectory = new(path);
                Module = SearchDirectory.EnumerateFiles(ModuleName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (Module is not null)
                {
                    if (IsCorrectModule(Module, ModuleSize, Checksum))
                    {
                        return Module.FullName;
                    }
                }
            }
            return Module is not null ? Module.FullName : null;
        }

        /// <summary>
        /// Controlla se il modulo trovato è effetivamente quello caricato.
        /// </summary>
        /// <param name="Module">Istanza di <see cref="System.IO.FileInfo"/> che rappresenta il modulo.</param>
        /// <param name="ModuleSize">Dimensione, in byte, del modulo.</param>
        /// <param name="Checksum">Checksum del modulo.</param>
        /// <returns>true se il modulo trovato è quello corretto, false altrimenti.</returns>
        private static bool IsCorrectModule(System.IO.FileInfo Module, uint ModuleSize, uint Checksum)
        {
            if (Module.Length == ModuleSize)
            {
                uint ImageChecksum = GetImageChecksum(Module.FullName, IsImage32Bit(Module.FullName));
                return ImageChecksum == Checksum;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region Detailed Properties (Memory)
        /// <summary>
        /// Recupera informazioni sulla memoria utilizzata da un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Una lista di istanze di <see cref="MemoryRegionInfo"/> con le informazioni.</returns>
        public static List<MemoryRegionInfo> GetProcessMemoryInfo(SafeProcessHandle Handle)
        {
            if (Handle is not null && !Handle.IsInvalid)
            {
                bool? Is32BitProcess = IsProcess32Bit(Handle);
                if (Is32BitProcess.HasValue)
                {
                    List<MemoryRegionInfo> MemoryRegionsInfo = new();
                    Win32ComputerInfoFunctions.GetNativeSystemInfo(out Win32Structures.SYSTEM_INFO SystemInfo);
                    IntPtr Address = IntPtr.Zero;
                    if (Is32BitProcess.Value)
                    {
                        while (Address.ToInt64() < SystemInfo.MaximumApplicationAddress.ToInt64())
                        {
                            if (Win32MemoryFunctions.VirtualQuery32(Handle.DangerousGetHandle(), Address, out Win32Structures.MEMORY_BASIC_INFORMATION Buffer, (ulong)Marshal.SizeOf(typeof(Win32Structures.MEMORY_BASIC_INFORMATION))) == IntPtr.Zero)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry;
                                if (ex.NativeErrorCode == Win32Constants.ERROR_INVALID_PARAMETER)
                                {
                                    Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo, indirizzo di memoria non valido", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                }
                                else
                                {
                                    Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                }
                                return null;
                            }
                            else
                            {
                                string PagesType = Enum.GetName(typeof(Win32Enumerations.MemoryPageType), Buffer.Type);
                                if (string.IsNullOrWhiteSpace(PagesType))
                                {
                                    PagesType = Properties.Resources.UnavailableText;
                                }
                                string PagesState = Enum.GetName(typeof(Win32Enumerations.MemoryPageState), Buffer.State);
                                if (string.IsNullOrWhiteSpace(PagesState))
                                {
                                    PagesType = Properties.Resources.UnavailableText;
                                }
                                ulong Size = Buffer.RegionSize.ToUInt64();
                                double CalculatedValue;
                                string SizeString = null;
                                if (Size >= 1048576 && Size < 1073741824)
                                {
                                    CalculatedValue = (double)Size / 1024 / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                                }
                                else if (Size >= 1073741824)
                                {
                                    CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                                }
                                else if (Size < 1048576)
                                {
                                    CalculatedValue = (double)Size / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                                }
                                string InitialProtection = GetCombinedProtectionValues(Buffer.AllocationProtect);
                                string CurrentProtection = GetCombinedProtectionValues(Buffer.Protect);
                                MemoryRegionsInfo.Add(new MemoryRegionInfo(Buffer.BaseAddress, PagesType, PagesState, SizeString, InitialProtection, CurrentProtection));
                                long NewMemoryAddress = (long)Buffer.BaseAddress + (long)Buffer.RegionSize;
                                Address = new IntPtr(NewMemoryAddress);
                            }
                        }
                    }
                    else
                    {
                        while (Address.ToInt64() < SystemInfo.MaximumApplicationAddress.ToInt64())
                        {
                            if (Win32MemoryFunctions.VirtualQuery64(Handle.DangerousGetHandle(), Address, out Win32Structures.MEMORY_BASIC_INFORMATION64 Buffer, (ulong)Marshal.SizeOf(typeof(Win32Structures.MEMORY_BASIC_INFORMATION64))) == IntPtr.Zero)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry;
                                if (ex.NativeErrorCode == Win32Constants.ERROR_INVALID_PARAMETER)
                                {
                                    Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo, indirizzo di memoria non valido", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                }
                                else
                                {
                                    Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                }
                                return null;
                            }
                            else
                            {
                                string PagesType = Enum.GetName(typeof(Win32Enumerations.MemoryPageType), Buffer.Type);
                                if (string.IsNullOrWhiteSpace(PagesType))
                                {
                                    PagesType = Properties.Resources.UnavailableText;
                                }
                                string PagesState = Enum.GetName(typeof(Win32Enumerations.MemoryPageState), Buffer.State);
                                if (string.IsNullOrWhiteSpace(PagesState))
                                {
                                    PagesType = Properties.Resources.UnavailableText;
                                }
                                ulong Size = Buffer.RegionSize;
                                double CalculatedValue;
                                string SizeString = null;
                                if (Size >= 1048576 && Size < 1073741824)
                                {
                                    CalculatedValue = (double)Size / 1024 / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                                }
                                else if (Size >= 1073741824)
                                {
                                    CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                                }
                                else if (Size < 1048576)
                                {
                                    CalculatedValue = (double)Size / 1024;
                                    SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                                }
                                string InitialProtection = GetCombinedProtectionValues(Buffer.AllocationProtect);
                                string CurrentProtection = GetCombinedProtectionValues(Buffer.Protect);
                                MemoryRegionsInfo.Add(new MemoryRegionInfo(new IntPtr((long)Buffer.BaseAddress), PagesType, PagesState, SizeString, InitialProtection, CurrentProtection));
                                long NewMemoryAddress = (long)Buffer.BaseAddress + (long)Buffer.RegionSize;
                                Address = new IntPtr(NewMemoryAddress);
                            }
                        }
                    }
                    MemoryRegionsInfo.Sort((x, y) => x.BaseAddress.CompareTo(y.BaseAddress));
                    return MemoryRegionsInfo;
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile se un processo è a 32 bit", EventAction.MemoryInfoRead, Handle);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo, handle non valido", EventAction.MemoryInfoRead);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su una regione di memoria.
        /// </summary>
        /// <param name="Handle">Handle al processo che ha eseguito l'allocazione o la liberazione.</param>
        /// <param name="BaseAddress">Indirizzo di base della regione di memoria.</param>
        /// <returns>Un'istanza di <see cref="MemoryRegionInfo"/> con le informazioni.</returns>
        public static MemoryRegionInfo GetMemoryRegionInfo(SafeProcessHandle Handle, IntPtr BaseAddress)
        {
            MemoryRegionInfo Info;
            bool? Is32BitProcess = IsProcess32Bit(Handle);
            if (Is32BitProcess.HasValue)
            {
                if (Is32BitProcess.Value)
                {
                    if (Win32MemoryFunctions.VirtualQuery32(Handle.DangerousGetHandle(), BaseAddress, out Win32Structures.MEMORY_BASIC_INFORMATION Buffer, (ulong)Marshal.SizeOf(typeof(Win32Structures.MEMORY_BASIC_INFORMATION))) == IntPtr.Zero)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry;
                        if (ex.NativeErrorCode == Win32Constants.ERROR_INVALID_PARAMETER)
                        {
                            Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo, indirizzo di memoria non valido", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                        else
                        {
                            Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                        return null;
                    }
                    else
                    {
                        string PagesType = Enum.GetName(typeof(Win32Enumerations.MemoryPageType), Buffer.Type);
                        if (string.IsNullOrWhiteSpace(PagesType))
                        {
                            PagesType = Properties.Resources.UnavailableText;
                        }
                        string PagesState = Enum.GetName(typeof(Win32Enumerations.MemoryPageState), Buffer.State);
                        if (string.IsNullOrWhiteSpace(PagesState))
                        {
                            PagesType = Properties.Resources.UnavailableText;
                        }
                        ulong Size = Buffer.RegionSize.ToUInt64();
                        double CalculatedValue;
                        string SizeString = null;
                        if (Size >= 1048576 && Size < 1073741824)
                        {
                            CalculatedValue = (double)Size / 1024 / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                        }
                        else if (Size >= 1073741824)
                        {
                            CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                        }
                        else if (Size < 1048576)
                        {
                            CalculatedValue = (double)Size / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                        }
                        string InitialProtection = GetCombinedProtectionValues(Buffer.AllocationProtect);
                        string CurrentProtection = GetCombinedProtectionValues(Buffer.Protect);
                        Info = new MemoryRegionInfo(Buffer.BaseAddress, PagesType, PagesState, SizeString, InitialProtection, CurrentProtection);
                    }
                }
                else
                {
                    if (Win32MemoryFunctions.VirtualQuery64(Handle.DangerousGetHandle(), BaseAddress, out Win32Structures.MEMORY_BASIC_INFORMATION64 Buffer, (ulong)Marshal.SizeOf(typeof(Win32Structures.MEMORY_BASIC_INFORMATION64))) == IntPtr.Zero)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry;
                        if (ex.NativeErrorCode == Win32Constants.ERROR_INVALID_PARAMETER)
                        {
                            Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo, indirizzo di memoria non valido", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                        else
                        {
                            Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla memoria virtuale di un processo", EventAction.MemoryInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                        return null;
                    }
                    else
                    {
                        string PagesType = Enum.GetName(typeof(Win32Enumerations.MemoryPageType), Buffer.Type);
                        if (string.IsNullOrWhiteSpace(PagesType))
                        {
                            PagesType = Properties.Resources.UnavailableText;
                        }
                        string PagesState = Enum.GetName(typeof(Win32Enumerations.MemoryPageState), Buffer.State);
                        if (string.IsNullOrWhiteSpace(PagesState))
                        {
                            PagesType = Properties.Resources.UnavailableText;
                        }
                        ulong Size = Buffer.RegionSize;
                        double CalculatedValue;
                        string SizeString = null;
                        if (Size >= 1048576 && Size < 1073741824)
                        {
                            CalculatedValue = (double)Size / 1024 / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " MB";
                        }
                        else if (Size >= 1073741824)
                        {
                            CalculatedValue = (double)Size / 1024 / 1024 / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " GB";
                        }
                        else if (Size < 1048576)
                        {
                            CalculatedValue = (double)Size / 1024;
                            SizeString = CalculatedValue.ToString("N2", CultureInfo.CurrentCulture) + " KB";
                        }
                        string InitialProtection = GetCombinedProtectionValues(Buffer.AllocationProtect);
                        string CurrentProtection = GetCombinedProtectionValues(Buffer.Protect);
                        Info = new MemoryRegionInfo(new IntPtr((long)Buffer.BaseAddress), PagesType, PagesState, SizeString, InitialProtection, CurrentProtection);
                    }
                }
                return Info;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile se un processo è a 32 bit", EventAction.MemoryInfoRead, Handle);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Restituisce l'eventuale valore combinato della protezione della memoria.
        /// </summary>
        /// <param name="Protection">Tipo di protezione.</param>
        /// <returns>La stringa che indica tutte le protezioni applicate alla memoria.</returns>
        private static string GetCombinedProtectionValues(Win32Enumerations.MemoryProtections Protection)
        {
            StringBuilder CombinedProtection = new();
            string CombinedValueString;
            string SingleValueString;
            Win32Enumerations.MemoryProtections SingleValue;
            foreach (Win32Enumerations.MemoryProtections value in Enum.GetValues(typeof(Win32Enumerations.MemoryProtections)))
            {
                if (value is Win32Enumerations.MemoryProtections.PAGE_GUARD or Win32Enumerations.MemoryProtections.PAGE_NOCACHE or Win32Enumerations.MemoryProtections.PAGE_WRITECOMBINE or Win32Enumerations.MemoryProtections.PAGE_ENCLAVE_THREAD_CONTROL or Win32Enumerations.MemoryProtections.PAGE_ENCLAVE_UNVALIDATED)
                {
                    if (Protection.HasFlag(value))
                    {
                        CombinedValueString = Enum.GetName(typeof(Win32Enumerations.MemoryProtections), value);
                        SingleValue = Protection & ~value;
                        SingleValueString = Enum.GetName(typeof(Win32Enumerations.MemoryProtections), SingleValue);
                        CombinedProtection.Append(SingleValueString).Append(" + ").Append(CombinedValueString);
                    }
                }
            }
            if (CombinedProtection.Length == 0)
            {
                return Enum.GetName(typeof(Win32Enumerations.MemoryProtections), Protection);
            }
            else
            {
                return CombinedProtection.ToString();
            }
        }
        #endregion
        #region Detailed Properties (CLR Performance)
        /// <summary>
        /// Recupera la lista di istanze di <see cref="PerformanceCounter"/> in cui appare il processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Una lista di istanze di <see cref="PerformanceCounter"/>.</returns>
        public static Dictionary<string, List<PerformanceCounter>> GetCounters(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                Dictionary<string, List<PerformanceCounter>> Counters = new();
                string ProcessName = GetProcessName(Handle).Replace(".exe", string.Empty);
                PerformanceCounterCategory Category = new(".NET CLR Exceptions");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Exceptions", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR Interop");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Interop", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR JIT");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("JIT", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR Loading");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Loading", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR LocksAndThreads");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("LockAndThreads", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR Memory");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Memory", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR Networking 4.0.0.0");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Networking", Category.GetCounters(ProcessName).ToList());
                }
                Category = new PerformanceCounterCategory(".NET CLR Security");
                if (Category.InstanceExists(ProcessName))
                {
                    Counters.Add("Security", Category.GetCounters(ProcessName).ToList());
                }
                return Counters;
            }
            else
            {
                return null;
            }
        }
        #endregion
        #region Other Properties
        /// <summary>
        /// Recupera il nome della società che ha prodotto un eseguibile.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il nome della società.</returns>
        public static string GetExecutableCompanyName(SafeProcessHandle Handle, string FullPath)
        {
            Contract.Requires(Handle != null);
            FileVersionInfo VersionInfo = GetFileVersionInfo2(Handle, FullPath);
            if (VersionInfo != null)
            {
                return VersionInfo.CompanyName;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome dell società produttrice di un eseguibile, informazioni sulla versione del file non disponibili", EventAction.OtherPropertiesRead, Handle, ProcessName: Path.GetFileName(FullPath));
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera il nome dell'interfaccia COM identificata da un GUID.
        /// </summary>
        /// <param name="InterfaceID">GUID dell'interfaccia.</param>
        /// <returns>Il nome dell'interfaccia.</returns>
        public static string GetComInterfaceName(string InterfaceID)
        {
            using RegistryKey AppIDKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\AppID\\" + InterfaceID);
            if (AppIDKey != null)
            {
                string Name = (string)AppIDKey.GetValue(string.Empty);
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    return Name;
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un'interfaccia COM, nome non fornito", EventAction.OtherPropertiesRead, null);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome di un'interfaccia COM, impossibile aprire la relativa chiave di registro", EventAction.OtherPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera la linea di comando di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>La linea di comando.</returns>
        public static string GetProcessCommandLine(SafeProcessHandle Handle)
        {
            IntPtr PEBAddress = GetProcessPEBAddress2(Handle);
            if (PEBAddress != IntPtr.Zero)
            {
                int BufferSize = Marshal.SizeOf(typeof(Win32Structures.PEB_64));
                IntPtr Buffer = Marshal.AllocHGlobal(BufferSize);
                if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), PEBAddress, Buffer, (IntPtr)BufferSize, out _))
                {
                    Win32Structures.PEB_64 PEB = (Win32Structures.PEB_64)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PEB_64));
                    Marshal.FreeHGlobal(Buffer);
                    BufferSize = Marshal.SizeOf(typeof(Win32Structures.RTL_USER_PROCESS_PARAMETERS_DOCUMENTED));
                    Buffer = Marshal.AllocHGlobal(BufferSize);
                    if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), PEB.ProcessParameters, Buffer, (IntPtr)BufferSize, out _))
                    {
                        Win32Structures.RTL_USER_PROCESS_PARAMETERS_DOCUMENTED ProcessParameters = (Win32Structures.RTL_USER_PROCESS_PARAMETERS_DOCUMENTED)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.RTL_USER_PROCESS_PARAMETERS_DOCUMENTED));
                        Marshal.FreeHGlobal(Buffer);
                        BufferSize = ProcessParameters.CommandLine.Length;
                        Buffer = Marshal.AllocHGlobal(BufferSize);
                        if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), ProcessParameters.CommandLine.Buffer, Buffer, (IntPtr)BufferSize, out _))
                        {
                            byte[] StringBytes = new byte[BufferSize];
                            for (int i = 0; i < StringBytes.Length; i++)
                            {
                                StringBytes[i] = Marshal.ReadByte(Buffer, i);
                            }
                            Marshal.FreeHGlobal(Buffer);
                            string CommandLine = Encoding.Unicode.GetString(StringBytes);
                            return CommandLine;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return Properties.Resources.UnavailableText;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la linea di comando di un processo, indirizzo di memoria del PEB non disponibile", EventAction.OtherPropertiesRead, Handle);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera la linea di comando di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>La linea di comando del processo se disponibile.</returns>
        public static string GetProcessCommandLine2(SafeProcessHandle Handle)
        {
            uint Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessCommandLineInformation, IntPtr.Zero, 0, out uint ReturnSize);
            if (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
            {
                IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnSize);
                Result = Win32ProcessFunctions.NtQueryInformationProcess(Handle.DangerousGetHandle(), Win32Enumerations.ProcessInformationClass.ProcessCommandLineInformation, Buffer, ReturnSize, out _);
                if (Result == Win32Constants.STATUS_SUCCESS)
                {
                    Win32Structures.UNICODE_STRING2 CommandLine = (Win32Structures.UNICODE_STRING2)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.UNICODE_STRING2));
                    byte[] StringBytes = new byte[CommandLine.Length];
                    for (int i = 0; i < CommandLine.Length; i++)
                    {
                        StringBytes[i] = Marshal.ReadByte(CommandLine.Buffer);
                        CommandLine.Buffer += 1;
                    }
                    return Encoding.Unicode.GetString(StringBytes);
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare la linea di comando di un processo", EventAction.OtherPropertiesRead, Handle, Result);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la linea di comando di un processo, handle non valido", EventAction.OtherPropertiesRead, Handle);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera il percorso della directory corrente di un processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il percorso della directory corrente.</returns>
        public static string GetProcessCurrentDirectory(SafeProcessHandle Handle)
        {
            IntPtr PEBAddress = GetProcessPEBAddress2(Handle);
            if (PEBAddress != IntPtr.Zero)
            {
                int BufferSize = Marshal.SizeOf(typeof(Win32Structures.PEB_64));
                IntPtr Buffer = Marshal.AllocHGlobal(BufferSize);
                if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), PEBAddress, Buffer, (IntPtr)BufferSize, out _))
                {
                    Win32Structures.PEB_64 PEB = (Win32Structures.PEB_64)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.PEB_64));
                    Marshal.DestroyStructure(Buffer, typeof(Win32Structures.UNICODE_STRING2));
                    Marshal.DestroyStructure(Buffer, typeof(Win32Structures.RTL_USER_PROCESS_PARAMETERS));
                    Marshal.FreeHGlobal(Buffer);
                    BufferSize = Marshal.SizeOf(typeof(Win32Structures.RTL_USER_PROCESS_PARAMETERS));
                    Buffer = Marshal.AllocHGlobal(BufferSize);
                    if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), PEB.ProcessParameters, Buffer, (IntPtr)BufferSize, out _))
                    {
                        Win32Structures.RTL_USER_PROCESS_PARAMETERS ProcessParameters = (Win32Structures.RTL_USER_PROCESS_PARAMETERS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.RTL_USER_PROCESS_PARAMETERS));
                        Marshal.FreeHGlobal(Buffer);
                        BufferSize = ProcessParameters.CurrentDirectoryPath.Length;
                        Buffer = Marshal.AllocHGlobal(BufferSize);
                        if (Win32MemoryFunctions.ReadProcessMemory(Handle.DangerousGetHandle(), ProcessParameters.CurrentDirectoryPath.Buffer, Buffer, (IntPtr)BufferSize, out _))
                        {
                            byte[] StringBytes = new byte[BufferSize];
                            for (int i = 0; i < StringBytes.Length; i++)
                            {
                                StringBytes[i] = Marshal.ReadByte(Buffer, i);
                            }
                            Marshal.FreeHGlobal(Buffer);
                            return Encoding.Unicode.GetString(StringBytes);
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        Marshal.DestroyStructure(Buffer, typeof(Win32Structures.UNICODE_STRING2));
                        Marshal.FreeHGlobal(Buffer);
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return Properties.Resources.UnavailableText;
                    }
                }
                else
                {
                    Marshal.FreeHGlobal(Buffer);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile leggere la memoria di un processo", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la directory corrente di un processo, indirizzo di memoria del PEB non disponibile", EventAction.OtherPropertiesRead, Handle);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera il nome del pacchetto a cui appartiene l'applicazione.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Il nome del pacchetto.</returns>
        public static string GetApplicationPackageName(SafeProcessHandle Handle)
        {
            uint BufferSize = 0;
            int Result = Win32OtherFunctions.GetPackageFullName(Handle.DangerousGetHandle(), ref BufferSize, null);
            if (Result == Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                StringBuilder PackageName = new((int)BufferSize);
                Result = Win32OtherFunctions.GetPackageFullName(Handle.DangerousGetHandle(), ref BufferSize, PackageName);
                if (Result == Win32Constants.ERROR_SUCCESS)
                {
                    return PackageName.ToString();
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome del pacchetto", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return Properties.Resources.UnavailableText;
                }
            }
            else if (Result == Win32Constants.APPMODEL_ERROR_NO_PACKAGE)
            {
                return Properties.Resources.NoneText;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome del pacchetto", EventAction.OtherPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Determina se un process appartiene a un job.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se il processo appartiene a un job, false altrimenti, nullo in caso di errore.</returns>
        public static bool? IsProcessInJob(SafeProcessHandle Handle)
        {
            if (Win32OtherFunctions.IsProcessInJob(Handle.DangerousGetHandle(), IntPtr.Zero, out bool Result))
            {
                return Result;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo appartiene a un job", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Determina se un processo è a 32 bit.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>true se il processo è a 32 bit, false altrimenti, nullo in caso di errore.</returns>
        public static bool? IsProcess32Bit(SafeProcessHandle Handle)
        {
            if (Win32OtherFunctions.IsWow64Process(Handle.DangerousGetHandle(), out ushort ProcessMachine, out _))
            {
                return ProcessMachine != 0; //IMAGE_FILE_MACHINE_UNKNOWN
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è a 32 bit", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #endregion
        #region Processor Usage Calculation Methods
        /// <summary>
        /// Recupera l'uso del processore da parte di un processo.
        /// </summary>
        /// <param name="Handle">Handle del processo.</param>
        /// <param name="LastProcessTime">Ultimo uso della CPU da parte del processo.</param>
        /// <returns>Un valore decimale rappresentante l'uso della cpu da parte del processo, nullo in caso di errore.</returns>
        public static decimal? GetProcessProcessorUsage(SafeProcessHandle Handle, ref decimal LastProcessTime)
        {
            if (Handle is not null && !Handle.IsInvalid)
            {
                if (Win32ProcessFunctions.GetProcessTimes(Handle.DangerousGetHandle(), out _, out _, out ulong KernelTime, out ulong UserTime))
                {
                    decimal KernelNanosecondsCount = KernelTime * 100;
                    decimal UserNanosecondsCount = UserTime * 100;
                    decimal TotalProcessNanosecondsTime = (KernelNanosecondsCount + UserNanosecondsCount) / Environment.ProcessorCount;
                    decimal TotalProcessTime = TotalProcessNanosecondsTime / 1000000;
                    if (LastProcessTime == 0)
                    {
                        LastProcessTime = TotalProcessTime;
                    }
                    decimal ProcessTimeDifference = TotalProcessTime - LastProcessTime;
                    decimal CpuUsage = ProcessTimeDifference * 100 / Settings.ProcessDataUpdateRate;
                    LastProcessTime = TotalProcessTime;
                    return CpuUsage;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le tempistiche di un processo", EventAction.ProcessPropertiesRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le tempistiche di un processo, handle non valido", EventAction.ProcessPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la percentuale di inattività del processore.
        /// </summary>
        /// <param name="PreviousSystemIdleTime">Precedente tempo di inattività del sistema.</param>
        /// <param name="PreviousSystemTotalTime">Precedente tempo totale di sistema.</param>
        /// <param name="PreviousSystemUserTime">Precedente tempo utente di sistema.</param>
        /// <returns>Un valore decimale rappresentante la percentuale di tempo rispetto al tempo totale di attività in cui il processore è stato inattivo, nullo in caso di errore.</returns>
        public static decimal? GetProcessorIdlePercentage(ref decimal PreviousSystemIdleTime, ref decimal PreviousSystemUserTime, ref decimal PreviousSystemTotalTime)
        {
            if (Win32ProcessFunctions.GetSystemTimes(out ulong IdleTime, out ulong KernelTime, out ulong UserTime))
            {
                decimal IdleTimeNanosecondCount = IdleTime * 100;
                decimal KernelTimeNanosecondCount = KernelTime * 100;
                decimal UserTimeNanosecondCount = UserTime * 100;
                decimal SystemIdleTime = IdleTimeNanosecondCount / 1000000;
                decimal SystemKernelTime = KernelTimeNanosecondCount / 1000000;
                decimal SystemUserTime = UserTimeNanosecondCount / 1000000;
                decimal SystemTotalTime = SystemKernelTime + SystemUserTime;
                decimal TotalTimeDifference = SystemTotalTime - PreviousSystemTotalTime;
                decimal IdleTimeDifference = SystemIdleTime - PreviousSystemIdleTime;
                decimal Headroom = IdleTimeDifference / TotalTimeDifference * 100;
                PreviousSystemIdleTime = SystemIdleTime;
                PreviousSystemTotalTime = SystemTotalTime;
                PreviousSystemUserTime = SystemUserTime;
                return Headroom;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le tempistiche del sistema", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #region Process Enumeration Methods
        /// <summary>
        /// Recupera la lista di PID dei processi figlio di un processo specificato.
        /// </summary>
        /// <param name="ParentProcessStartTime">Data e ora di avvio del processo padre.</param>
        /// <param name="ParentProcessID">ID del processo padre.</param>
        /// <param name="SnapshotHandle">Handle nativo a uno snapshot del sistema, può avere valore <see cref="IntPtr.Zero"/></param>
        /// <returns>Una lista con i PID dei processi figli, null in caso di errore.</returns>
        /// <remarks>Se <paramref name="SnapshotHandle"/> ha un valore diverso da <see cref="IntPtr.Zero"/>, questo metodo utilizza lo snapshot fornito per eseguire l'enumerazione al posto di eseguirne uno proprio.</remarks>
        public static List<uint> FindProcessChildren(DateTime ParentProcessStartTime, uint ParentProcessID, IntPtr SnapshotHandle)
        {
            List<uint> ChildrenPIDs = new();
            if (SnapshotHandle == IntPtr.Zero)
            {
                SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPPROCESS, 0);
            }
            if (SnapshotHandle != IntPtr.Zero)
            {
                Win32Structures.PROCESSENTRY32 ProcessInfo = new()
                {
                    Size = Convert.ToUInt32(Marshal.SizeOf(typeof(Win32Structures.PROCESSENTRY32)))
                };
                IntPtr ProcessHandle;
                DateTime? ProcessStartTime;
                if (!Win32ProcessFunctions.Process32First(SnapshotHandle, ref ProcessInfo))
                {
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un processo da uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                else
                {
                    if (ProcessInfo.ParentPID == ParentProcessID)
                    {
                        ProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessInfo.PID);
                        if (ProcessHandle != IntPtr.Zero)
                        {
                            ProcessStartTime = GetProcessStartTime(ProcessHandle);
                            if (ProcessStartTime.HasValue)
                            {
                                if (ProcessStartTime > ParentProcessStartTime)
                                {
                                    ChildrenPIDs.Add(ProcessInfo.PID);
                                }
                            }
                            else
                            {
                                _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                return null;
                            }
                            _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                        }
                        else
                        {
                            ProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessInfo.PID);
                            if (ProcessHandle != IntPtr.Zero)
                            {
                                ProcessStartTime = GetProcessStartTime(ProcessHandle);
                                if (ProcessStartTime.HasValue)
                                {
                                    if (ProcessStartTime > ParentProcessStartTime)
                                    {
                                        ChildrenPIDs.Add(ProcessInfo.PID);
                                    }
                                }
                                else
                                {
                                    _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                    return null;
                                }
                                _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                            }
                            else
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un processo", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                    }
                    bool End = false;
                    while (!End)
                    {
                        if (!Win32ProcessFunctions.Process32Next(SnapshotHandle, ref ProcessInfo))
                        {
                            int ErrorCode = Marshal.GetLastWin32Error();
                            if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                            {
                                End = true;
                            }
                            else
                            {
                                _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                Win32Exception ex = new(ErrorCode);
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un processo da uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                        else
                        {
                            if (ProcessInfo.ParentPID == ParentProcessID)
                            {
                                ProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessInfo.PID);
                                if (ProcessHandle != IntPtr.Zero)
                                {
                                    ProcessStartTime = GetProcessStartTime(ProcessHandle);
                                    if (ProcessStartTime.HasValue)
                                    {
                                        if (ProcessStartTime > ParentProcessStartTime)
                                        {
                                            ChildrenPIDs.Add(ProcessInfo.PID);
                                        }
                                    }
                                    else
                                    {
                                        _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                        return null;
                                    }
                                    _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                }
                                else
                                {
                                    ProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessInfo.PID);
                                    if (ProcessHandle != IntPtr.Zero)
                                    {
                                        ProcessStartTime = GetProcessStartTime(ProcessHandle);
                                        if (ProcessStartTime.HasValue)
                                        {
                                            if (ProcessStartTime > ParentProcessStartTime)
                                            {
                                                ChildrenPIDs.Add(ProcessInfo.PID);
                                            }
                                        }
                                        else
                                        {
                                            _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                            return null;
                                        }
                                        _ = Win32OtherFunctions.CloseHandle(ProcessHandle);
                                    }
                                    else
                                    {
                                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un processo", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                                        Logger.WriteEntry(Entry);
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile prendere uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            return ChildrenPIDs;
        }

        /// <summary>
        /// Recupera i processi in esecuzione.
        /// </summary>
        /// <param name="OtherInformations">Contiene altre informazioni utili per il processo.</param>
        /// <returns>Un array di <see cref="SafeProcessHandle"/> contenente gli handle con cui accedere ai processi.</returns>
        public static SafeProcessHandle[] GetRunningProcesses(out List<(uint PID, uint ThreadCount, string Name)> OtherInformations)
        {
            List<SafeProcessHandle> ProcessHandles = new();
            OtherInformations = new List<(uint PID, uint ThreadCount, string Name)>();
            SafeProcessHandle ProcessHandle;
            IntPtr ProcessUnsafeHandle;
            IntPtr SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPPROCESS, 0);
            if (SnapshotHandle != IntPtr.Zero)
            {
                Win32Structures.PROCESSENTRY32 ProcessInfo = new()
                {
                    Size = Convert.ToUInt32(Marshal.SizeOf(typeof(Win32Structures.PROCESSENTRY32)))
                };
                if (!Win32ProcessFunctions.Process32First(SnapshotHandle, ref ProcessInfo))
                {
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un processo da uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                else
                {
                    ProcessUnsafeHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessInfo.PID);
                    ProcessHandle = new SafeProcessHandle(ProcessUnsafeHandle, true);
                    ProcessHandles.Add(ProcessHandle);
                    OtherInformations.Add((ProcessInfo.PID, ProcessInfo.Threads, ProcessInfo.ExecutableName));
                    bool End = false;
                    while (!End)
                    {
                        if (!Win32ProcessFunctions.Process32Next(SnapshotHandle, ref ProcessInfo))
                        {
                            int ErrorCode = Marshal.GetLastWin32Error();
                            if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                            {
                                End = true;
                            }
                            else
                            {
                                _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un processo da uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                        else
                        {
                            ProcessUnsafeHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessInfo.PID);
                            if (ProcessUnsafeHandle == IntPtr.Zero)
                            {
                                ProcessUnsafeHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessInfo.PID);
                                if (ProcessUnsafeHandle != IntPtr.Zero)
                                {
                                    ProcessHandle = new SafeProcessHandle(ProcessUnsafeHandle, true);
                                    ProcessHandles.Add(ProcessHandle);
                                    OtherInformations.Add((ProcessInfo.PID, ProcessInfo.Threads, ProcessInfo.ExecutableName));
                                }
                            }
                            else
                            {
                                ProcessHandle = new SafeProcessHandle(ProcessUnsafeHandle, true);
                                ProcessHandles.Add(ProcessHandle);
                                OtherInformations.Add((ProcessInfo.PID, ProcessInfo.Threads, ProcessInfo.ExecutableName));
                            }
                        }

                    }
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile prendere uno snapshot del sistema", EventAction.ProcessEnumeration, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            return ProcessHandles.ToArray();
        }

        /// <summary>
        /// Recupera una lista di handle ai thread di un processo.
        /// </summary>
        /// <param name="ProcessID">ID del processo.</param>
        /// <returns>Una lista di handle ai thread del processo.</returns>
        public static List<IntPtr> EnumerateProcessThreads(uint ProcessID)
        {
            List<IntPtr> ThreadHandles = new();
            IntPtr SnapshotHandle = Win32ProcessFunctions.CreateToolHelp32Snapshot(Win32Enumerations.SnapshotSystemPortions.TH32CS_SNAPTHREAD, 0);
            if (SnapshotHandle != IntPtr.Zero)
            {
                Win32Structures.THREADENTRY32 ThreadInfo = new();
                ThreadInfo.StructureSize = Convert.ToUInt32(Marshal.SizeOf(ThreadInfo));
                if (!Win32ProcessFunctions.Thread32First(SnapshotHandle, ref ThreadInfo))
                {
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread da uno snapshot del sistema", EventAction.ThreadEnumeration, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                else
                {
                    if (ProcessID == ThreadInfo.OwnerPID)
                    {
                        IntPtr ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_ALL_ACCESS, false, ThreadInfo.ThreadID);
                        if (ThreadHandle != IntPtr.Zero)
                        {
                            ThreadHandles.Add(ThreadHandle);
                        }
                        else
                        {
                            ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_LIMITED_INFORMATION, false, ThreadInfo.ThreadID);
                            if (ThreadHandle != IntPtr.Zero)
                            {
                                ThreadHandles.Add(ThreadHandle);
                            }
                            else
                            {
                                _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un thread", EventAction.ThreadEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                    }
                    bool End = false;
                    while (!End)
                    {
                        if (!Win32ProcessFunctions.Thread32Next(SnapshotHandle, ref ThreadInfo))
                        {
                            int ErrorCode = Marshal.GetLastWin32Error();
                            if (ErrorCode == Win32Constants.ERROR_NO_MORE_FILES)
                            {
                                End = true;
                            }
                            else
                            {
                                _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un thread da uno snapshot del sistema", EventAction.ThreadEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                foreach (IntPtr handle in ThreadHandles)
                                {
                                    _ = Win32OtherFunctions.CloseHandle(handle);
                                }
                                return null;
                            }
                        }
                        else
                        {
                            if (ProcessID == ThreadInfo.OwnerPID)
                            {
                                IntPtr ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_ALL_ACCESS, false, ThreadInfo.ThreadID);
                                if (ThreadHandle != IntPtr.Zero)
                                {
                                    ThreadHandles.Add(ThreadHandle);
                                }
                                else
                                {
                                    ThreadHandle = Win32ProcessFunctions.OpenThread(Win32Enumerations.ThreadAccessRights.THREAD_QUERY_LIMITED_INFORMATION, false, ThreadInfo.ThreadID);
                                    if (ThreadHandle != IntPtr.Zero)
                                    {
                                        ThreadHandles.Add(ThreadHandle);
                                    }
                                    else
                                    {
                                        _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un thread", EventAction.ThreadEnumeration, null, ex.NativeErrorCode, ex.Message);
                                        Logger.WriteEntry(Entry);
                                        continue;
                                    }
                                }
                            }
                        }

                    }
                    _ = Win32OtherFunctions.CloseHandle(SnapshotHandle);
                }
                return ThreadHandles;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile prendere uno snapshot del sistema", EventAction.ThreadEnumeration, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        #region Process Token Manipulation/Query Methods
        #region Virtualization
        /// <summary>
        /// Abilita la virtualizzazione per un processo.
        /// </summary>
        /// <param name="Handle">Handle nativo al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool EnableVirtualizationForProcess(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, Handle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare la virtualizzazione per un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, Handle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    GCHandle ValueHandle;
                    if (Win32TokenFunctions.OpenProcessToken(Handle.DangerousGetHandle(), Win32Enumerations.TokenAccessRights.TOKEN_WRITE | Win32Enumerations.TokenAccessRights.TOKEN_QUERY, out IntPtr TokenHandle))
                    {
                        IntPtr Buffer = Marshal.AllocHGlobal(4);
                        if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationAllowed, Buffer, (uint)Marshal.SizeOf(typeof(uint)), out _))
                        {
                            if ((uint)Marshal.ReadInt32(Buffer) != 0)
                            {
                                Marshal.FreeHGlobal(Buffer);
                                Buffer = Marshal.AllocHGlobal(4);
                                if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationEnabled, Buffer, (uint)Marshal.SizeOf(typeof(uint)), out _))
                                {
                                    if ((uint)Marshal.ReadInt32(Buffer) != 0)
                                    {
                                        Win32OtherFunctions.CloseHandle(TokenHandle);
                                        Marshal.FreeHGlobal(Buffer);
                                        return true;
                                    }
                                    else
                                    {
                                        Marshal.FreeHGlobal(Buffer);
                                        ValueHandle = GCHandle.Alloc((uint)1, GCHandleType.Pinned);
                                        if (Win32TokenFunctions.SetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationEnabled, ValueHandle.AddrOfPinnedObject(), (uint)Marshal.SizeOf(typeof(uint))))
                                        {
                                            ValueHandle.Free();
                                            Win32OtherFunctions.CloseHandle(TokenHandle);
                                            return true;
                                        }
                                        else
                                        {
                                            ValueHandle.Free();
                                            Win32OtherFunctions.CloseHandle(TokenHandle);
                                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile modificare le informazioni presenti nel token di un processo, informazione: virtualizzazione", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                                            Logger.WriteEntry(Entry);
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    Marshal.FreeHGlobal(Buffer);
                                    Win32OtherFunctions.CloseHandle(TokenHandle);
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            }
                            else
                            {
                                Marshal.FreeHGlobal(Buffer);
                                Win32OtherFunctions.CloseHandle(TokenHandle);
                                return false;
                            }
                        }
                        else
                        {
                            Marshal.FreeHGlobal(Buffer);
                            Win32OtherFunctions.CloseHandle(TokenHandle);
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire il token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare la virtualizzazione per un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation, Handle);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare la virtualizzazione per un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Disabilita la virtualizzazione per un processo.
        /// </summary>
        /// <param name="Handle">Handle nativo al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool DisableVirtualizationForProcess(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, Handle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile disabilitare la virtualizzazione per un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, Handle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    GCHandle ValueHandle;
                    if (Win32TokenFunctions.OpenProcessToken(Handle.DangerousGetHandle(), Win32Enumerations.TokenAccessRights.TOKEN_WRITE | Win32Enumerations.TokenAccessRights.TOKEN_QUERY, out IntPtr TokenHandle))
                    {
                        IntPtr Buffer = Marshal.AllocHGlobal(4);
                        if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationAllowed, Buffer, (uint)Marshal.SizeOf(typeof(uint)), out _))
                        {
                            if ((uint)Marshal.ReadInt32(Buffer) != 0)
                            {
                                Marshal.FreeHGlobal(Buffer);
                                Buffer = Marshal.AllocHGlobal(4);
                                if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationEnabled, Buffer, (uint)Marshal.SizeOf(typeof(uint)), out _))
                                {
                                    if ((uint)Marshal.ReadInt32(Buffer) != 0)
                                    {
                                        Marshal.FreeHGlobal(Buffer);
                                        ValueHandle = GCHandle.Alloc((uint)0, GCHandleType.Pinned);
                                        if (Win32TokenFunctions.SetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationEnabled, ValueHandle.AddrOfPinnedObject(), (uint)Marshal.SizeOf(typeof(uint))))
                                        {
                                            ValueHandle.Free();
                                            Win32OtherFunctions.CloseHandle(TokenHandle);
                                            return true;
                                        }
                                        else
                                        {
                                            ValueHandle.Free();
                                            Win32OtherFunctions.CloseHandle(TokenHandle);
                                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile modificare le informazioni presenti sul token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                                            Logger.WriteEntry(Entry);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        Marshal.FreeHGlobal(Buffer);
                                        Win32OtherFunctions.CloseHandle(TokenHandle);
                                        return true;
                                    }
                                }
                                else
                                {
                                    Marshal.FreeHGlobal(Buffer);
                                    Win32OtherFunctions.CloseHandle(TokenHandle);
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            }
                            else
                            {
                                Marshal.FreeHGlobal(Buffer);
                                Win32OtherFunctions.CloseHandle(TokenHandle);
                                return true;
                            }
                        }
                        else
                        {
                            Marshal.FreeHGlobal(Buffer);
                            Win32OtherFunctions.CloseHandle(TokenHandle);
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire il token di un processo", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile disabilitare la virtualizzazione per un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation, Handle);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile disabilitare la virtualizzazione per un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
            
        }
        #endregion
        #region Token Info Setters
        /// <summary>
        /// Cambia il livello di integrità del token di accesso di un processo.
        /// </summary>
        /// <param name="Handle">Handle nativo al token di accesso.</param>
        /// <param name="Level">Nuovo livello di integrità.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ChangeTokenIntegrityLevel(SafeProcessHandle Handle, string Level)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, Handle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(Handle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, Handle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile caambiare il livello di integrità del token di un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, Handle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    IntPtr TokenHandle = GetProcessTokenHandle(Handle);
                    if (TokenHandle != IntPtr.Zero)
                    {
                        if (!IsCurrentIntegrityLevelLower(TokenHandle, Level))
                        {
                            return Level switch
                            {
                                "Untrusted" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinUntrustedLabelSid),
                                "Low" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinLowLabelSid),
                                "Medium" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinMediumLabelSid),
                                "MediumPlus" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinMediumPlusLabelSid),
                                "High" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinHighLabelSid),
                                "System" => ChangeTokenIntegrityLevel(TokenHandle, Win32Enumerations.WellKnownSid.WinSystemLabelSid),
                                _ => false,
                            };
                        }
                        else
                        {
                            _ = CloseHandle(TokenHandle);
                            return false;
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare il livello di integrità del token di un processo, impossibile aprire il token", EventAction.TokenInfoManipulation, Handle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare il livello di integrità del token di un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare il livello di integrità del token di un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Cambia il livello di integrità del token di accesso di un processo.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <param name="NewLevel">SID noto che identifica il nuovo livello di integrità.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        private static bool ChangeTokenIntegrityLevel(IntPtr TokenHandle, Win32Enumerations.WellKnownSid NewLevel)
        {
            IntPtr NewLevelSID = IntPtr.Zero;
            uint SIDSize = 0;
            _ = Win32OtherFunctions.CreateWellKnownSid(NewLevel, IntPtr.Zero, NewLevelSID, ref SIDSize);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                _ = CloseHandle(TokenHandle);
                string AliasString = Enum.GetName(typeof(Win32Enumerations.WellKnownSid), NewLevel);
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un SID per un alias noto, alias: " + AliasString, EventAction.TokenInfoManipulation, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            else
            {
                NewLevelSID = Marshal.AllocHGlobal((int)SIDSize);
                if (!Win32OtherFunctions.CreateWellKnownSid(NewLevel, IntPtr.Zero, NewLevelSID, ref SIDSize))
                {
                    _ = CloseHandle(TokenHandle);
                    string AliasString = Enum.GetName(typeof(Win32Enumerations.WellKnownSid), NewLevel);
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un SID per un alias noto, alias: " + AliasString, EventAction.TokenInfoManipulation, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    Win32Structures.SID_AND_ATTRIBUTES IntegrityLevelStructure = new()
                    {
                        Sid = NewLevelSID,
                        Attributes = 0
                    };
                    Win32Structures.TOKEN_MANDATORY_LABEL MandatoryLevel = new()
                    {
                        Label = IntegrityLevelStructure
                    };
                    IntPtr MandatoryLabelInfoBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(MandatoryLevel));
                    Marshal.StructureToPtr(MandatoryLevel, MandatoryLabelInfoBuffer, false);
                    if (!Win32TokenFunctions.SetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenIntegrityLevel, MandatoryLabelInfoBuffer, (uint)Marshal.SizeOf(MandatoryLevel)))
                    {
                        Marshal.FreeHGlobal(MandatoryLabelInfoBuffer);
                        _ = CloseHandle(TokenHandle);
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile modificare le informazioni presenti nel token di un processo, informazione: livello integrità", EventAction.TokenInfoManipulation, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        Marshal.FreeHGlobal(MandatoryLabelInfoBuffer);
                        _ = CloseHandle(TokenHandle);
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Determina se il livello attuale di integrità del token di accesso di un processo è più basso del nuovo livello da impostare.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <param name="NewLevel">Nuovo livello da impostare.</param>
        /// <returns>true se l'attuale livello di integrità è minore di quello da impostare, false altrimenti.</returns>
        private static bool IsCurrentIntegrityLevelLower(IntPtr TokenHandle, string NewLevel)
        {
            string CurrentLevel = GetTokenIntegrityLevel(TokenHandle);
            if (!string.IsNullOrWhiteSpace(CurrentLevel))
            {
                if (CurrentLevel == NewLevel)
                {
                    return true;
                }
                return NewLevel switch
                {
                    "Untrusted" => true,
                    "Low" => CurrentLevel == "Untrusted",
                    "Medium" => CurrentLevel == "Untrusted" || CurrentLevel == "Low",
                    "Medium Plus" => CurrentLevel == "Untrusted" || CurrentLevel == "Low" || CurrentLevel == "Medium",
                    "High" => CurrentLevel == "Untrusted" || CurrentLevel == "Low" || CurrentLevel == "Medium" || CurrentLevel == "Medium Plus",
                    "System" => CurrentLevel == "Untrusted" || CurrentLevel == "Low" || CurrentLevel == "Medium" || CurrentLevel == "Medium Plus" || CurrentLevel == "High",
                    _ => true
                };
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Abilita un privilegio in un token di accesso di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token di accesso.</param>
        /// <param name="TokenHandle">Handle al token.</param>
        /// <param name="PrivilegeName">Nome del privilegio da abilitare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool EnableTokenPrivilege(SafeProcessHandle ProcessHandle, SafeAccessTokenHandle TokenHandle, string PrivilegeName)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare un privilegio in un token di un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, ProcessHandle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    if (Win32SecurityFunctions.LookupPrivilegeValue(null, PrivilegeName, out Win32Structures.LUID Luid))
                    {
                        Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                        {
                            PrivilegeCount = 1,
                            LUID = Luid,
                            Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                        };
                        if (Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle.DangerousGetHandle(), false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                        {
                            return true;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato di un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare il valore di un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare un privilegio nel token di un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare un privilegio nel token di un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Disabilita un privilegio in un token di accesso di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token di accesso.</param>
        /// <param name="TokenHandle">Handle al token.</param>
        /// <param name="PrivilegeName">Nome del privilegio da disabilitare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool DisableTokenPrivilege(SafeProcessHandle ProcessHandle, SafeAccessTokenHandle TokenHandle, string PrivilegeName)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile abilitare un privilegio in un token di un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, ProcessHandle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    if (Win32SecurityFunctions.LookupPrivilegeValue(null, PrivilegeName, out Win32Structures.LUID Luid))
                    {
                        Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                        {
                            PrivilegeCount = 1,
                            LUID = Luid,
                            Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_DISABLED
                        };
                        if (Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle.DangerousGetHandle(), false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                        {
                            return true;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato di un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare il valore di un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile disabilitare un privilegio nel token di un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile disabilitare un privilegio nel token di un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Rimuove un privilegio da un token di accesso di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo a cui appartiene il token di accesso.</param>
        /// <param name="TokenHandle">Handle al token.</param>
        /// <param name="PrivilegeName">Nome del privilegio da rimuovere.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool RemovePrivilegeFromToken(SafeProcessHandle ProcessHandle, SafeAccessTokenHandle TokenHandle, string PrivilegeName)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile rimuovere un privilegio dal token di un processo, azioni su processi di sistema non sono permesse", EventAction.TokenInfoManipulation, ProcessHandle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    if (Win32SecurityFunctions.LookupPrivilegeValue(null, PrivilegeName, out Win32Structures.LUID Luid))
                    {
                        Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                        {
                            PrivilegeCount = 1,
                            LUID = Luid,
                            Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_REMOVED
                        };
                        if (Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle.DangerousGetHandle(), false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                        {
                            return true;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile rimuovere un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare il valore di un privilegio, privilegio: " + PrivilegeName, EventAction.TokenInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile rimuovere un privilegio nel token di un processo, azioni sul processo corrente non sono permesse", EventAction.TokenInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile rimuovere un privilegio nel token di un processo, handle al processo non valido", EventAction.TokenInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Token Info Getters
        /// <summary>
        /// Recupera un handle nativo a un token di accesso.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Handle nativo al token di accesso.</returns>
        public static IntPtr GetProcessTokenHandle(SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Win32TokenFunctions.OpenProcessToken(Handle.DangerousGetHandle(), Win32Enumerations.TokenAccessRights.TOKEN_WRITE | Win32Enumerations.TokenAccessRights.TOKEN_READ | Win32Enumerations.TokenAccessRights.TOKEN_QUERY_SOURCE, out IntPtr TokenHandle))
                {
                    return TokenHandle;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire il token di un processo", EventAction.TokenInfoRead, Handle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return IntPtr.Zero;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile aprire il token di un processo, handle al processo non valido", EventAction.TokenInfoRead, null);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Recupera un handle nativo al token di accesso del processo corrente con permessi di lettura.
        /// </summary>
        /// <returns>Handle nativo al token di accesso.</returns>
        private static IntPtr GetCurrentProcessTokenForQuery()
        {
            if (Win32TokenFunctions.OpenProcessToken(Win32OtherFunctions.GetCurrentProcess(), Win32Enumerations.TokenAccessRights.TOKEN_QUERY, out IntPtr TokenHandle))
            {
                return TokenHandle;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire il token del processo corrente", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Recupera un handle nativo al token di accesso del processo corrente con permessi di scrittura.
        /// </summary>
        /// <returns>Handle nativo al token di accesso.</returns>
        private static IntPtr GetCurrentProcessTokenForWriting()
        {
            IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
            if (Win32OtherFunctions.DuplicateHandle(CurrentProcessHandle, CurrentProcessHandle, CurrentProcessHandle, out IntPtr TargetHandle, (uint)Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, 0))
            {
                if (Win32TokenFunctions.OpenProcessToken(TargetHandle, Win32Enumerations.TokenAccessRights.TOKEN_WRITE, out IntPtr TokenHandle))
                {
                    CloseHandle(TargetHandle);
                    return TokenHandle;
                }
                else
                {
                    CloseHandle(TargetHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire il token del processo corrente", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return IntPtr.Zero;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire aprire un handle al processo corrente", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Recupera informazioni sul token di accesso di un processo.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Un istanza di <see cref="TokenInfo"/> con le informazioni.</returns>
        public static TokenInfo GetProcessTokenInfo(IntPtr TokenHandle, bool? IsTokenElevated)
        {
            if (TokenHandle != IntPtr.Zero)
            {
                TokenGeneralInfo GeneralInfo = GetTokenGeneralInfo(TokenHandle, IsTokenElevated);
                GetTokenGroupsAndPrivilegesInfo(TokenHandle, out List<TokenGroupInfo> Groups, out List<TokenPrivilegeInfo> Privileges);
                TokenStatistics Statistics = GetTokenStatistics(TokenHandle);
                List<TokenGroupInfo> TokenCapabilities = GetTokenCapabilities(TokenHandle);
#if DEBUG
                List<ClaimSecurityAttribute> TokenUserClaims = GetTokenClaims(TokenHandle, "User");
                List<ClaimSecurityAttribute> TokenDeviceClaims = GetTokenClaims(TokenHandle, "Device");
#else
            List<ClaimSecurityAttribute> TokenUserClaims = null;
            List<ClaimSecurityAttribute> TokenDeviceClaims = null;
#endif
                return new TokenInfo(TokenHandle, GeneralInfo, Groups, Privileges, Statistics, TokenCapabilities, TokenUserClaims, TokenDeviceClaims);
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sul token di accesso di un processo, handle al token non valido", EventAction.TokenInfoRead);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la lista e le relative informazioni sui privilegi presenti in un token di accesso di un processo.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Una lista di istanze di <see cref="TokenPrivilegeInfo"/> con le informazioni.</returns>
        public static List<TokenPrivilegeInfo> GetUpdatedTokenPrivileges(IntPtr TokenHandle)
        {
            List<TokenPrivilegeInfo> Privileges = new();
            _ = Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenGroupsAndPrivileges, IntPtr.Zero, 0, out uint ReturnLength);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: gruppi e privilegi", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnLength);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenGroupsAndPrivileges, Buffer, ReturnLength, out _))
            {
                Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES GroupsAndPrivilegesInfo = (Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES));
                if (GroupsAndPrivilegesInfo.Sids != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.Sids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                }
                if (GroupsAndPrivilegesInfo.RestrictedSids != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.RestrictedSids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                }
                if (GroupsAndPrivilegesInfo.Privileges != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.Privileges, typeof(Win32Structures.LUID_AND_ATTRIBUTES));
                }
                Marshal.FreeHGlobal(Buffer);
                string PrivilegeName;
                string PrivilegeStatus;
                string PrivilegeDescription;
                Win32Structures.LUID_AND_ATTRIBUTES Privilege;
                for (int i = 0; i < GroupsAndPrivilegesInfo.PrivilegeCount; i++)
                {
                    Privilege = (Win32Structures.LUID_AND_ATTRIBUTES)Marshal.PtrToStructure(GroupsAndPrivilegesInfo.Privileges + i * Marshal.SizeOf(typeof(Win32Structures.LUID_AND_ATTRIBUTES)), typeof(Win32Structures.LUID_AND_ATTRIBUTES));
                    PrivilegeName = GetPrivilegeName(Privilege.Luid) ?? Properties.Resources.UnavailableText;
                    PrivilegeStatus = GetPrivilegeFlags((Win32Enumerations.PrivilegeLUIDAttributes)Privilege.Attributes) ?? Properties.Resources.UnavailableText;
                    PrivilegeDescription = GetPrivilegeDescription(PrivilegeName) ?? Properties.Resources.UnavailableText;
                    Privileges.Add(new(PrivilegeName, PrivilegeStatus, PrivilegeDescription));
                }
                Privileges.Sort((x, y) => x.Name.CompareTo(y.Name));
                return Privileges;
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: gruppi e privilegi", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #region Token Specific Info
        /// <summary>
        /// Recupera informazioni generali su un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token di accesso.</param>
        /// <returns>Un'istanza della classe <see cref="TokenGeneralInfo"/> con le informazioni.</returns>
        private static TokenGeneralInfo GetTokenGeneralInfo(IntPtr TokenHandle, bool? IsTokenElevated)
        {
            Contract.Requires(TokenHandle != null);
            GetTokenAssociatedUserInfo(TokenHandle, out string Username, out string UserSID);
            string SessionID = GetTokenSessionID(TokenHandle) ?? Properties.Resources.UnavailableText;
            bool?[] ElevationAndVirtualizationStatus = GetTokenElevationAndVirtualizationStatus(TokenHandle, IsTokenElevated);
            string AppContainerSID = GetAppContainerSID(TokenHandle) ?? Properties.Resources.UnavailableText;
            string IntegrityLevel = GetTokenIntegrityLevel(TokenHandle) ?? Properties.Resources.UnavailableText;
            string Owner = GetTokenOwner(TokenHandle) ?? Properties.Resources.UnavailableText;
            string PrimaryGroup = GetTokenPrimaryGroup(TokenHandle) ?? Properties.Resources.UnavailableText;
            GetTokenSourceNameAndLUID(TokenHandle, out string SourceName, out string SourceLUID);
            return new TokenGeneralInfo(Username, UserSID, SessionID, ElevationAndVirtualizationStatus[0], ElevationAndVirtualizationStatus[1], ElevationAndVirtualizationStatus[2], AppContainerSID, IntegrityLevel, Owner, PrimaryGroup, SourceName, SourceLUID);
        }
        #region Token General Info
        /// <summary>
        /// Recupera informazioni sull'utente associato a un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <param name="Username">Nome dell'utente associato al token.</param>
        /// <param name="UserSID">SID, in forma di stringa, che identifica l'utente.</param>
        private static void GetTokenAssociatedUserInfo(IntPtr TokenHandle, out string Username, out string UserSID)
        {
            Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenUser, IntPtr.Zero, 0, out uint ReturnLength);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Username = Properties.Resources.UnavailableText;
                UserSID = Properties.Resources.UnavailableText;
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: utente associato", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            else
            {
                IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnLength);
                if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenUser, Buffer, ReturnLength, out _))
                {
                    Win32Structures.TOKEN_USER UserInfo = (Win32Structures.TOKEN_USER)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_USER));
                    if (Win32OtherFunctions.ConvertSidToStringSid(UserInfo.User.Sid, out IntPtr StringSID))
                    {
                        UserSID = Marshal.PtrToStringUni(StringSID);
                        if (Win32OtherFunctions.LocalFree(StringSID) != IntPtr.Zero)
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria utilizzata dalla funzione, nome funzione: ConvertSidToStringSidW", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                    }
                    else
                    {
                        UserSID = Properties.Resources.UnavailableText;
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile convertire una struttura SID in una stringa", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                    }
                    Username = GetAccountName(UserInfo.User.Sid);
                    Marshal.FreeHGlobal(Buffer);
                }
                else
                {
                    Marshal.FreeHGlobal(Buffer);
                    Username = Properties.Resources.UnavailableText;
                    UserSID = Properties.Resources.UnavailableText;
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: utente associato", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                }
            }
        }

        /// <summary>
        /// Recupera l'ID della sessione Terminal Services associata a un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Una stringa che rappresenta l'ID della sessione.</returns>
        private static string GetTokenSessionID(IntPtr TokenHandle)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenSessionId, Buffer, 4, out _))
            {
                uint SessionID = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return SessionID.ToString("D0", CultureInfo.InvariantCulture);
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: ID sessione", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni sullo stato di elevazione e di virtualizzazione di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Un array con le informazioni.</returns>
        /// <remarks>Il primo elemento dell'array indica se il token ha privilegi amministrativi, il secondo indica se la virtualizzazione è permessa, il terzo indica se la virtualizzazione è abilitata.</remarks>
        private static bool?[] GetTokenElevationAndVirtualizationStatus(IntPtr TokenHandle, bool? IsTokenElevated)
        {
            bool?[] Status = new bool?[3];
            IntPtr Buffer = IntPtr.Zero;
            if (IsTokenElevated.HasValue)
            {
                Status[0] = IsTokenElevated;
            }
            else
            {
                Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_ELEVATION)));
                if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenElevation, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_ELEVATION)), out _))
                {
                    Win32Structures.TOKEN_ELEVATION ElevationStatus = (Win32Structures.TOKEN_ELEVATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_ELEVATION));
                    if (ElevationStatus.IsElevated != 0)
                    {
                        Status[0] = true;
                    }
                    else
                    {
                        Status[0] = false;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: stato elevazione", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    Status[0] = null;
                }
            }
            Marshal.FreeHGlobal(Buffer);
            Buffer = Marshal.AllocHGlobal(4);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationAllowed, Buffer, 4, out _))
            {
                if ((uint)Marshal.ReadInt32(Buffer) != 0)
                {
                    Status[1] = true;
                }
                else
                {
                    Status[1] = false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: permesso virtualizzazione", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Status[1] = null;
            }
            Marshal.FreeHGlobal(Buffer);
            Buffer = Marshal.AllocHGlobal(4);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenVirtualizationEnabled, Buffer, 4, out _))
            {
                if ((uint)Marshal.ReadInt32(Buffer) != 0)
                {
                    Status[2] = true;
                }
                else
                {
                    Status[2] = false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: abilitazione virtualizzazione", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Status[2] = null;
            }
            Marshal.FreeHGlobal(Buffer);
            return Status;
        }

        /// <summary>
        /// Recupera informazioni sullo stato di elevazione di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>true se il token è un token amministrativo.</returns>
        public static bool? GetTokenElevationStatus(IntPtr TokenHandle)
        {
            if (TokenHandle != IntPtr.Zero)
            {
                IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_ELEVATION)));
                if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenElevation, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_ELEVATION)), out _))
                {
                    Win32Structures.TOKEN_ELEVATION ElevationStatus = (Win32Structures.TOKEN_ELEVATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_ELEVATION));
                    Marshal.FreeHGlobal(Buffer);
                    Win32OtherFunctions.CloseHandle(TokenHandle);
                    if (ElevationStatus.IsElevated != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: stato elevazione", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: stato elevazione, handle al token non disponibile", EventAction.TokenInfoRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera una stringa che rappresenta il SID dell'app containter di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Una stringa che rappresenta il SID.</returns>
        private static string GetAppContainerSID(IntPtr TokenHandle)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_APPCONTAINER_INFORMATION)));
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenAppContainerSid, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_APPCONTAINER_INFORMATION)), out _))
            {
                Win32Structures.TOKEN_APPCONTAINER_INFORMATION AppContainerInfo = (Win32Structures.TOKEN_APPCONTAINER_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_APPCONTAINER_INFORMATION));
                Marshal.FreeHGlobal(Buffer);
                if (AppContainerInfo.TokenAppContainer != IntPtr.Zero)
                {
                    if (Win32OtherFunctions.ConvertSidToStringSid(AppContainerInfo.TokenAppContainer, out IntPtr StringSID))
                    {
                        string SID = Marshal.PtrToStringUni(StringSID);
                        if (Win32OtherFunctions.LocalFree(StringSID) != IntPtr.Zero)
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria utilizzata dalla funzione, nome funzione: ConvertSidToStringSidW", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                        return SID;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile convertire una struttura SID in una stringa", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: SID Appcontainer", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: SID Appcontainer", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il livello di integrità del token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Una stringa che rappresenta il livello di integrità del token di accesso.</returns>
        private static string GetTokenIntegrityLevel(IntPtr TokenHandle)
        {
            Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenIntegrityLevel, IntPtr.Zero, 0, out uint ReturnLength);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: livello integrità", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnLength);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenIntegrityLevel, Buffer, ReturnLength, out _))
            {
                Win32Structures.TOKEN_MANDATORY_LABEL IntegrityLabel = (Win32Structures.TOKEN_MANDATORY_LABEL)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_MANDATORY_LABEL));
                IntPtr SubAuthorityCountPointer = Win32OtherFunctions.GetSIDSubAuthorityCount(IntegrityLabel.Label.Sid);
                if (SubAuthorityCountPointer != IntPtr.Zero)
                {
                    byte SubAuthorityCount = Marshal.ReadByte(SubAuthorityCountPointer);
                    IntPtr SubAuthorityPointer = Win32OtherFunctions.GetSIDSubAuthority(IntegrityLabel.Label.Sid, (uint)SubAuthorityCount - 1);
                    if (SubAuthorityPointer != IntPtr.Zero)
                    {
                        uint SubAuthority = (uint)Marshal.ReadInt32(SubAuthorityPointer);
                        return (Win32Enumerations.ProcessIntegrityLevel)SubAuthority switch
                        {
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_UNTRUSTED_RID => "Untrusted",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_LOW_RID => "Low",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_MEDIUM_RID => "Medium",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_MEDIUM_PLUS => "Medium Plus",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_HIGH_RID => "High",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_SYSTEM_RID => "System",
                            Win32Enumerations.ProcessIntegrityLevel.SECURITY_MANDATORY_PROTECTED_PROCESS_RID => "Protected Process",
                            _ => "Unknown",
                        };
                    }
                    else
                    {
                        Win32Exception ex = new(ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un SID", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un SID", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: livello integrità", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il nome dell'account proprietario di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Il nome dell'account.</returns>
        private static string GetTokenOwner(IntPtr TokenHandle)
        {
            Win32SecurityFunctions.GetKernelObjectSecurity(TokenHandle, Win32Enumerations.SecurityInformations.OwnerSecurityInformation, IntPtr.Zero, 0, out uint LengthNeeded);
            if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il descrittore di sicurezza di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            IntPtr SecurityDescriptor = Marshal.AllocHGlobal((int)LengthNeeded);
            if (Win32SecurityFunctions.GetKernelObjectSecurity(TokenHandle, Win32Enumerations.SecurityInformations.OwnerSecurityInformation, SecurityDescriptor, LengthNeeded, out _))
            {
                if (Win32SecurityFunctions.GetSecurityDescriptorOwner(SecurityDescriptor, out IntPtr SID, out _))
                {
                    Marshal.FreeHGlobal(SecurityDescriptor);
                    if (SID != IntPtr.Zero)
                    {
                        return GetAccountName(SID);
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il proprietario di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il proprietario di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il descrittore di sicurezza di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il nome del gruppo primario di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Il nome del gruppo primario.</returns>
        private static string GetTokenPrimaryGroup(IntPtr TokenHandle)
        {
            Win32SecurityFunctions.GetKernelObjectSecurity(TokenHandle, Win32Enumerations.SecurityInformations.GroupSecurityInformation, IntPtr.Zero, 0, out uint LengthNeeded);
            if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il descrittore di sicurezza di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            IntPtr SecurityDescriptor = Marshal.AllocHGlobal((int)LengthNeeded);
            if (Win32SecurityFunctions.GetKernelObjectSecurity(TokenHandle, Win32Enumerations.SecurityInformations.GroupSecurityInformation, SecurityDescriptor, LengthNeeded, out _))
            {
                if (Win32SecurityFunctions.GetSecurityDescriptorGroup(SecurityDescriptor, out IntPtr SID, out _))
                {
                    Marshal.FreeHGlobal(SecurityDescriptor);
                    if (SID != IntPtr.Zero)
                    {
                        return GetAccountName(SID);
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il gruppo primario di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il gruppo primario di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il descrittore di sicurezza di un token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la nome e il <see cref="Win32Structures.LUID"/> della fonte del token.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <param name="SourceName">Nome della fonte.</param>
        /// <param name="SourceLUID"><see cref="Win32Structures.LUID"/> della fonte.</param>
        private static void GetTokenSourceNameAndLUID(IntPtr TokenHandle, out string SourceName, out string SourceLUID)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_SOURCE)));
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenSource, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_SOURCE)), out _))
            {
                Win32Structures.TOKEN_SOURCE Source = (Win32Structures.TOKEN_SOURCE)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_SOURCE));
                Marshal.FreeHGlobal(Buffer);
                SourceName = new string(Source.SourceName).Remove(Source.SourceName.Length - 1);
                SourceLUID = "0x" + LUIDToInt64(Source.SourceID).ToString("X", CultureInfo.InvariantCulture);
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                SourceName = Properties.Resources.UnavailableText;
                SourceLUID = Properties.Resources.UnavailableText;
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: fonte token", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
        }
        #endregion
        /// <summary>
        /// Recupera le informazioni sui gruppi presenti e sui privilegi applicati a un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token di accesso.</param>
        /// <param name="Groups">Lista di gruppi presenti nel token.</param>
        /// <param name="Privileges">Lista di privilegi applicati al token.</param>
        private static void GetTokenGroupsAndPrivilegesInfo(IntPtr TokenHandle, out List<TokenGroupInfo> Groups, out List<TokenPrivilegeInfo> Privileges)
        {
            Contract.Requires(TokenHandle != null);
            Groups = new();
            Privileges = new();
            _ = Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenGroupsAndPrivileges, IntPtr.Zero, 0, out uint ReturnLength);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: gruppi e privilegi", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Groups = null;
                Privileges = null;
            }
            IntPtr Buffer = Marshal.AllocHGlobal((int)ReturnLength);
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenGroupsAndPrivileges, Buffer, ReturnLength, out _))
            {
                Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES GroupsAndPrivilegesInfo = (Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_GROUPS_AND_PRIVILEGES));
                if (GroupsAndPrivilegesInfo.Sids != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.Sids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                }
                if (GroupsAndPrivilegesInfo.RestrictedSids != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.RestrictedSids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                }
                if (GroupsAndPrivilegesInfo.Privileges != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(GroupsAndPrivilegesInfo.Privileges, typeof(Win32Structures.LUID_AND_ATTRIBUTES));
                }
                Marshal.FreeHGlobal(Buffer);
                string AccountName;
                string AccountFlags;
                string PrivilegeName;
                string PrivilegeStatus;
                string PrivilegeDescription;
                Win32Structures.SID_AND_ATTRIBUTES SID;
                Win32Structures.LUID_AND_ATTRIBUTES Privilege;
                for (int i = 0; i < GroupsAndPrivilegesInfo.SidCount; i++)
                {
                    SID = (Win32Structures.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(GroupsAndPrivilegesInfo.Sids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                    AccountName = GetAccountName(SID.Sid) ?? Properties.Resources.UnavailableText;
                    AccountFlags = GetAccountFlags((Win32Enumerations.GroupsSIDAttributes)SID.Attributes) ?? Properties.Resources.UnavailableText;
                    if (AccountName.StartsWith("\\", StringComparison.CurrentCulture))
                    {
                        AccountName = AccountName.Remove(0, 1);
                    }
                    Groups.Add(new(AccountName, AccountFlags));
                    GroupsAndPrivilegesInfo.Sids += Marshal.SizeOf(SID);
                }
                for (int i = 0; i < GroupsAndPrivilegesInfo.RestrictedSidCount; i++)
                {
                    SID = (Win32Structures.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(GroupsAndPrivilegesInfo.RestrictedSids, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                    AccountName = GetAccountName(SID.Sid) ?? Properties.Resources.UnavailableText;
                    AccountFlags = GetAccountFlags((Win32Enumerations.GroupsSIDAttributes)SID.Attributes) ?? Properties.Resources.UnavailableText;
                    if (AccountName.StartsWith("\\", StringComparison.CurrentCulture))
                    {
                        AccountName = AccountName.Remove(0, 1);
                    }
                    Groups.Add(new(AccountName, AccountFlags));
                    GroupsAndPrivilegesInfo.RestrictedSids += Marshal.SizeOf(SID);
                }
                for (int i = 0; i < GroupsAndPrivilegesInfo.PrivilegeCount; i++)
                {
                    Privilege = (Win32Structures.LUID_AND_ATTRIBUTES)Marshal.PtrToStructure(GroupsAndPrivilegesInfo.Privileges + i * Marshal.SizeOf(typeof(Win32Structures.LUID_AND_ATTRIBUTES)), typeof(Win32Structures.LUID_AND_ATTRIBUTES));
                    PrivilegeName = GetPrivilegeName(Privilege.Luid) ?? Properties.Resources.UnavailableText;
                    PrivilegeStatus = GetPrivilegeFlags((Win32Enumerations.PrivilegeLUIDAttributes)Privilege.Attributes) ?? Properties.Resources.UnavailableText;
                    PrivilegeDescription = GetPrivilegeDescription(PrivilegeName) ?? Properties.Resources.UnavailableText;
                    Privileges.Add(new(PrivilegeName, PrivilegeStatus, PrivilegeDescription));
                }
                Groups.Sort((x, y) => x.Name.CompareTo(y.Name));
                Privileges.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Groups = null;
                Privileges = null;
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: gruppi e privilegi", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
        }
        #region Token Groups And Privileges
        /// <summary>
        /// Recupera la descrizione di un privilegio.
        /// </summary>
        /// <param name="PrivilegeName">Nome del privilegio.</param>
        /// <returns>La descrizione del privilegio.</returns>
        private static string GetPrivilegeDescription(string PrivilegeName)
        {
            uint DisplayNameLength = 0;
            _ = Win32SecurityFunctions.LookupPrivilegeDisplayName(null, PrivilegeName, null, ref DisplayNameLength, out _);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la descrizione di un privilegio, nome privilegio: " + PrivilegeName, EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message); ;
                Logger.WriteEntry(Entry);
                return null;
            }
            StringBuilder DisplayName = new((int)DisplayNameLength);
            if (Win32SecurityFunctions.LookupPrivilegeDisplayName(null, PrivilegeName, DisplayName, ref DisplayNameLength, out _))
            {
                return DisplayName.ToString();
            }
            else
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la descrizione di un privilegio, nome privilegio: " + PrivilegeName, EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message); ;
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il nome di un privilegio.
        /// </summary>
        /// <param name="LUID"><see cref="Win32Structures.LUID"/> che indentifica il privilegio.</param>
        /// <returns>Il nome del privilegio.</returns>
        private static string GetPrivilegeName(Win32Structures.LUID LUID)
        {
            uint PrivilegeNameLength = 0;
            _ = Win32SecurityFunctions.LookupPrivilegeName(null, ref LUID, null, ref PrivilegeNameLength);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di un privilegio", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message); ;
                Logger.WriteEntry(Entry);
            }
            StringBuilder PrivilegeName = new((int)PrivilegeNameLength);
            if (Win32SecurityFunctions.LookupPrivilegeName(null, ref LUID, PrivilegeName, ref PrivilegeNameLength))
            {
                return PrivilegeName.ToString();
            }
            else
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di un privilegio", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera le caratteristiche di un privilegio.
        /// </summary>
        /// <param name="Flags">Valore numerico che definisce le caratteristiche del privilegio.</param>
        /// <returns>Una stringa che rappresenta le caratteristiche del privilegio.</returns>
        private static string GetPrivilegeFlags(Win32Enumerations.PrivilegeLUIDAttributes Flags)
        {
            StringBuilder PrivilegeFlags = new();
            if (Flags == Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_DISABLED)
            {
                return "Disabled";
            }
            if (Flags.HasFlag(Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED))
            {
                if (PrivilegeFlags.Length == 0)
                {
                    PrivilegeFlags.Append("Enabled");
                }
                else
                {
                    PrivilegeFlags.Append(", Enabled");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED_BY_DEFAULT))
            {
                if (PrivilegeFlags.Length == 0)
                {
                    PrivilegeFlags.Append("Default Enabled");
                }
                else
                {
                    PrivilegeFlags.Append(", Default Enabled");
                }
            }
            return PrivilegeFlags.ToString();
        }

        /// <summary>
        /// Recupera il nome di un account da un SID.
        /// </summary>
        /// <param name="SID">SID dell'account.</param>
        /// <returns>Il nome dell'account.</returns>
        private static string GetAccountName(IntPtr SID)
        {
            uint UserNameLength = 0;
            uint DomainNameLength = 0;
            _ = Win32UserAccountFunctions.LookupAccountSid(null, SID, null, ref UserNameLength, null, ref DomainNameLength, out _);
            int ErrorCode = Marshal.GetLastWin32Error();
            if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di un account", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            StringBuilder UserNameBuilder = new((int)UserNameLength);
            StringBuilder DomainNameBuilder = new((int)DomainNameLength);
            if (Win32UserAccountFunctions.LookupAccountSid(null, SID, UserNameBuilder, ref UserNameLength, DomainNameBuilder, ref DomainNameLength, out _))
            {
                return DomainNameBuilder.ToString() + "\\" + UserNameBuilder.ToString();
            }
            else
            {
                Win32Exception ex = new(ErrorCode);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome di un account", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera le caratteristiche del SID di un account.
        /// </summary>
        /// <param name="Flags">Valore numerico che descrive le caratteristiche dell'account.</param>
        /// <returns>Una stringa che descrive le caratteristiche dell'account.</returns>
        private static string GetAccountFlags(Win32Enumerations.GroupsSIDAttributes Flags)
        {
            StringBuilder AccountFlags = new();
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_ENABLED))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Enabled");
                }
                else
                {
                    AccountFlags.Append(", Enabled");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_ENABLED_BY_DEFAULT))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Default Enabled");
                }
                else
                {
                    AccountFlags.Append(", Default Enabled");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_INTEGRITY) || Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_INTEGRITY_ENABLED))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Integrity");
                }
                else
                {
                    AccountFlags.Append(", Integrity");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_LOGON_ID))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Logon ID");
                }
                else
                {
                    AccountFlags.Append(", Logon ID");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_MANDATORY))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Mandatory");
                }
                else
                {
                    AccountFlags.Append(", Mandatory");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_OWNER))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Group Owner");
                }
                else
                {
                    AccountFlags.Append(", Group Owner");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_RESOURCE))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Resource");
                }
                else
                {
                    AccountFlags.Append(", Resource");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.GroupsSIDAttributes.SE_GROUP_USE_FOR_DENY_ONLY))
            {
                if (AccountFlags.Length == 0)
                {
                    AccountFlags.Append("Deny-Only");
                }
                else
                {
                    AccountFlags.Append(", Deny-Only");
                }
            }
            return AccountFlags.ToString();
        }
        #endregion
        /// <summary>
        /// Recupera le statistiche di un token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Un'istanza di <see cref="TokenStatistics"/> con le informazioni.</returns>
        private static TokenStatistics GetTokenStatistics(IntPtr TokenHandle)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_STATISTICS)));
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenStatistics, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_STATISTICS)), out _))
            {
                Win32Structures.TOKEN_STATISTICS Statistics = (Win32Structures.TOKEN_STATISTICS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_STATISTICS));
                Marshal.FreeHGlobal(Buffer);
                string TokenType = GetTokenTypeString(Statistics.TokenType);
                string TokenImpersonationLevel = TokenType == "Impersonation" ? GetTokenImpersonationLevel(Statistics.ImpersonationLevel) : Properties.Resources.UnavailableText;
                string TokenLUID = "0x" + LUIDToInt64(Statistics.TokenID).ToString("X", CultureInfo.InvariantCulture);
                string AuthenticationLUID = "0x" + LUIDToInt64(Statistics.AuthenticationID).ToString("X", CultureInfo.InvariantCulture);
                string MemoryUsed = null;
                string MemoryAvailable = null;
                uint CalculatedValue;
                #region Memory Usage Calculation
                if (Statistics.DynamicCharged >= 1048576 && Statistics.DynamicCharged < 1073741824)
                {
                    CalculatedValue = Statistics.DynamicCharged / 1024 / 1024;
                    MemoryUsed = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " MB";
                }
                else if (Statistics.DynamicCharged >= 1073741824)
                {
                    CalculatedValue = Statistics.DynamicCharged / 1024 / 1024 / 1024;
                    MemoryUsed = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " GB";
                }
                else if (Statistics.DynamicCharged >= 1024 && Statistics.DynamicCharged < 1048576)
                {
                    CalculatedValue = Statistics.DynamicCharged / 1024;
                    MemoryUsed = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " KB";
                }
                else if (Statistics.DynamicCharged < 1024)
                {
                    MemoryUsed = Statistics.DynamicCharged.ToString("N0", CultureInfo.CurrentCulture) + " B";
                }
                #endregion
                #region Memory Available Calculation
                if (Statistics.DynamicAvailable >= 1048576 && Statistics.DynamicAvailable < 1073741824)
                {
                    CalculatedValue = Statistics.DynamicAvailable / 1024 / 1024;
                    MemoryAvailable = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " MB";
                }
                else if (Statistics.DynamicAvailable >= 1073741824)
                {
                    CalculatedValue = Statistics.DynamicAvailable / 1024 / 1024 / 1024;
                    MemoryAvailable = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " GB";
                }
                else if (Statistics.DynamicAvailable >= 1024 && Statistics.DynamicAvailable < 1048576)
                {
                    CalculatedValue = Statistics.DynamicAvailable / 1024;
                    MemoryAvailable = CalculatedValue.ToString("N0", CultureInfo.CurrentCulture) + " KB";
                }
                else if (Statistics.DynamicAvailable < 1024)
                {
                    MemoryAvailable = Statistics.DynamicAvailable.ToString("N0", CultureInfo.CurrentCulture) + " B";
                }
                #endregion
                return new(TokenType, TokenImpersonationLevel, TokenLUID, AuthenticationLUID, MemoryUsed, MemoryAvailable);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: statistiche", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #region Token Statistics
        /// <summary>
        /// Converte un valore di enumerazione <see cref="Win32Enumerations.TokenType"/> in una stringa.
        /// </summary>
        /// <param name="Type">Valore di enumerazione <see cref="Win32Enumerations.TokenType"/>.</param>
        /// <returns>Una stringa che indica il tipo di token.</returns>
        private static string GetTokenTypeString(Win32Enumerations.TokenType Type)
        {
            return Type switch
            {
                Win32Enumerations.TokenType.TokenImpersonation => "Impersonation",
                Win32Enumerations.TokenType.TokenPrimary => "Primary",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// Converte un valore di enumerazione <see cref="Win32Enumerations.SecurityImpersonationLevel"/> in una stringa.
        /// </summary>
        /// <param name="Level">Valore di enumerazione <see cref="Win32Enumerations.SecurityImpersonationLevel"/>.</param>
        /// <returns>Una stringa che indica il livello di impersonazione del token.</returns>
        private static string GetTokenImpersonationLevel(Win32Enumerations.SecurityImpersonationLevel Level)
        {
            return Level switch
            {
                Win32Enumerations.SecurityImpersonationLevel.SecurityAnonymous => "Anonymous",
                Win32Enumerations.SecurityImpersonationLevel.SecurityDelegation => "Delegation",
                Win32Enumerations.SecurityImpersonationLevel.SecurityIdentification => "Identification",
                Win32Enumerations.SecurityImpersonationLevel.SecurityImpersonation => "Impersonation",
                _ => "Unknown",
            };
        }

        #endregion
        /// <summary>
        /// Recupera le capacità del token di accesso.
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <returns>Una lista con le informazioni.</returns>
        private static List<TokenGroupInfo> GetTokenCapabilities(IntPtr TokenHandle)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.TOKEN_GROUPS)));
            if (Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenCapabilities, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.TOKEN_GROUPS)), out _))
            {
                List<TokenGroupInfo> CapabilitiesList = new();
                Win32Structures.TOKEN_GROUPS Capabilities = (Win32Structures.TOKEN_GROUPS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOKEN_GROUPS));
                Marshal.FreeHGlobal(Buffer);
                Win32Structures.SID_AND_ATTRIBUTES SID;
                string Name;
                string Flags;
                for (int i = 0; i < Capabilities.GroupCount; i++)
                {
                    SID = (Win32Structures.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(Capabilities.Groups, typeof(Win32Structures.SID_AND_ATTRIBUTES));
                    Name = GetAccountName(SID.Sid);
                    Flags = GetAccountFlags((Win32Enumerations.GroupsSIDAttributes)SID.Attributes);
                    CapabilitiesList.Add(new(Name, Flags));
                    Capabilities.Groups += Marshal.SizeOf(SID);
                }
                return CapabilitiesList;
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: capacità", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera i claim del token
        /// </summary>
        /// <param name="TokenHandle">Handle nativo al token.</param>
        /// <param name="ClaimType">Tipo di claim da recuperare.</param>
        /// <returns>Una lista con tutti i claim recuperati.</returns>
        private static List<ClaimSecurityAttribute> GetTokenClaims(IntPtr TokenHandle, string ClaimType)
        {
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION)));
            bool Result;
            if (ClaimType == "User")
            {
                Result = Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenUserClaimAttributes, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION)), out _);
            }
            else
            {
                Result = Win32TokenFunctions.GetTokenInformation(TokenHandle, Win32Enumerations.TokenInformationClass.TokenDeviceClaimAttributes, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION)), out _);
            }
            if (Result)
            {
                List<ClaimSecurityAttribute> Claims = new();
                Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION AttributesInformation = (Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTES_INFORMATION));
                Marshal.FreeHGlobal(Buffer);
                Win32Structures.CLAIM_SECURITY_ATTRIBUTE_V1 Attribute;
                string ValueType;
                string Flags;
                List<string> ValuesStringList = new();
                for (int i = 0; i < AttributesInformation.AttributeCount; i++)
                {
                    Attribute = (Win32Structures.CLAIM_SECURITY_ATTRIBUTE_V1)Marshal.PtrToStructure(AttributesInformation.Attributes + i * Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_V1)), typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_V1));
                    ValueType = GetAttributeValueType(Attribute.ValueType);
                    Flags = GetAttributeFlags((Win32Enumerations.SecurityAttributesFlags)Attribute.Flags);
                    switch (Attribute.ValueType)
                    {
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_BOOLEAN:
                            long[] Booleans = new long[Attribute.ValueCount];
                            Marshal.Copy(Attribute.Values, Booleans, 0, (int)Attribute.ValueCount);
                            foreach (ulong value in Booleans)
                            {
                                if (value == 1)
                                {
                                    ValuesStringList.Add("TRUE");
                                }
                                else
                                {
                                    ValuesStringList.Add("FALSE");
                                }
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_FQBN:
                            Win32Structures.CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE FQBNValue;
                            for (int j = 0; j < Attribute.ValueCount; j++)
                            {
                                FQBNValue = (Win32Structures.CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE)Marshal.PtrToStructure(Attribute.Values + i * Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE)), typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_FQBN_VALUE));
                                ValuesStringList.Add(FQBNValue.Name);
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_INT64:
                            long[] Integers = new long[Attribute.ValueCount];
                            Marshal.Copy(Attribute.Values, Integers, 0, (int)Attribute.ValueCount);
                            foreach (long value in Integers)
                            {
                                ValuesStringList.Add(value.ToString("D0", CultureInfo.CurrentCulture));
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_OCTET_STRING:
                            Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE StringValue;
                            for (int j = 0; j < Attribute.ValueCount; j++)
                            {
                                StringValue = (Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE)Marshal.PtrToStructure(Attribute.Values + i * Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE)), typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE));
                                ValuesStringList.Add(Marshal.PtrToStringBSTR(StringValue.Value));
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_SID:
                            Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE SIDValue;
                            List<IntPtr> SIDPointers = new();
                            string SIDMemoryAddress;
                            for (int j = 0; j < Attribute.ValueCount; j++)
                            {
                                SIDValue = (Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE)Marshal.PtrToStructure(Attribute.Values + i * Marshal.SizeOf(typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE)), typeof(Win32Structures.CLAIM_SECURITY_ATTRIBUTE_OCTET_STRING_VALUE));
                                SIDMemoryAddress = Marshal.PtrToStringBSTR(SIDValue.Value);
                                if (IntPtr.Size == 4)
                                {
                                    SIDPointers.Add(new(Convert.ToInt32(SIDMemoryAddress, 16)));
                                }
                                else if (IntPtr.Size == 8)
                                {
                                    SIDPointers.Add(new(Convert.ToInt64(SIDMemoryAddress, 16)));
                                }
                            }
                            foreach (IntPtr SID in SIDPointers)
                            {
                                if (Win32OtherFunctions.ConvertSidToStringSid(SID, out IntPtr StringSID))
                                {
                                    ValuesStringList.Add(Marshal.PtrToStringUni(StringSID));
                                    if (Win32OtherFunctions.LocalFree(StringSID) != IntPtr.Zero)
                                    {
                                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria allocata da una funzione, nome funzione: ConvertSidToStringSidW", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                                        Logger.WriteEntry(Entry);
                                    }
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile convertire una struttura SID in una stringa", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                }
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_STRING:
                            string String;
                            for (int j = 0; j < Attribute.ValueCount; j++)
                            {
                                String = Marshal.PtrToStringUni(Attribute.Values);
                                Attribute.Values += Encoding.Unicode.GetByteCount(String);
                                ValuesStringList.Add(String);
                            }
                            break;
                        case Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_UINT64:
                            long[] UnsignedIntegers = new long[Attribute.ValueCount];
                            Marshal.Copy(Attribute.Values, UnsignedIntegers, 0, (int)Attribute.ValueCount);
                            foreach (long value in UnsignedIntegers)
                            {
                                ValuesStringList.Add(((ulong)value).ToString("D0", CultureInfo.CurrentCulture));
                            }
                            break;
                    }
                    Claims.Add(new(Attribute.Name, ValueType, Flags, ValuesStringList));
                }
                return Claims;
            }
            else
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sul token di un processo, informazione richiesta: claims", EventAction.TokenInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #region Token Claims
        /// <summary>
        /// Recupera una stringa che descrive il tipo di valore dei dati dell'attributo.
        /// </summary>
        /// <param name="Type">Valore di enumerazione <see cref="Win32Enumerations.SecurityAttributesValuesType"/> che indica il tipo di dati.</param>
        /// <returns>Una stringa che descrive il tipo di valore dei dati.</returns>
        private static string GetAttributeValueType(Win32Enumerations.SecurityAttributesValuesType Type)
        {
            return Type switch
            {
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_BOOLEAN => "Boolean",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_FQBN => "FQBN Value",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_INT64 => "Int64",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_OCTET_STRING => "Octet String",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_SID => "SID",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_STRING => "String",
                Win32Enumerations.SecurityAttributesValuesType.CLAIM_SECURITY_ATTRIBUTE_TYPE_UINT64 => "UInt64",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// Recupera le caratteristiche dell'attributo.
        /// </summary>
        /// <param name="Flags">Valore di enumerazione <see cref="Win32Enumerations.SecurityAttributesFlags"/>.</param>
        /// <returns></returns>
        private static string GetAttributeFlags(Win32Enumerations.SecurityAttributesFlags Flags)
        {
            StringBuilder AttributeFlags = new();
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTIBUTE_NON_INHERITABLE))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Non-Inheritable");
                }
                else
                {
                    AttributeFlags.Append(", Non-Inheritable");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTRIBUTE_DISABLED))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Disabled");
                }
                else
                {
                    AttributeFlags.Append(", Disabled");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTRIBUTE_DISABLED_BY_DEFAULT))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Default Disabled");
                }
                else
                {
                    AttributeFlags.Append(", Default Disabled");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTRIBUTE_MANDATORY))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Mandatory");
                }
                else
                {
                    AttributeFlags.Append(", Mandatory");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTRIBUTE_USE_FOR_DENY_ONLY))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Deny-Only");
                }
                else
                {
                    AttributeFlags.Append(", Deny-Only");
                }
            }
            if (Flags.HasFlag(Win32Enumerations.SecurityAttributesFlags.CLAIM_SECURITY_ATTRIBUTE_VALUE_CASE_SENSITIVE))
            {
                if (AttributeFlags.Length == 0)
                {
                    AttributeFlags.Append("Case Sensitive");
                }
                else
                {
                    AttributeFlags.Append(", Case Sensitive");
                }
            }
            return AttributeFlags.ToString();
        }
        #endregion
        #endregion
        #endregion
        #endregion
        #region Thread Query/Set Methods
        /// <summary>
        /// Termina un thread.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se il processo è quello corrente o uno di sistema, questo metodo non effettua operazioni.</remarks>
        public static bool TerminateThread(SafeProcessHandle ProcessHandle, IntPtr ThreadHandle)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ThreadTermination, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un thread, azioni su processi di sistema non sono permesse", EventAction.ThreadTermination, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Win32ProcessFunctions.TerminateThread(ThreadHandle, uint.MaxValue))
                    {
                        return true;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile terminare un thread", EventAction.ThreadTermination, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un thread, azioni sul processo corrente non sono permesse", EventAction.ThreadTermination);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile terminare un thread, handle al processo non valido", EventAction.ThreadTermination, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Sospende l'esecuzione di un thread.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>true se l'operazione è riuscità, false altrimenti.</returns>
        public static bool SuspendThread(SafeProcessHandle ProcessHandle, IntPtr ThreadHandle)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ThreadSuspension, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile sospendere un thread, azioni su processi di sistema non sono permesse", EventAction.ThreadSuspension, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    bool? Is32BitProcess = IsProcess32Bit(ProcessHandle);
                    if (Is32BitProcess.HasValue)
                    {
                        if (Is32BitProcess.Value)
                        {
                            if (Win32ProcessFunctions.Wow64SuspendThread(ThreadHandle) != uint.MaxValue)
                            {
                                return true;
                            }
                            else
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile sospendere un thread", EventAction.ThreadSuspension, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                        else
                        {
                            if (Win32ProcessFunctions.SuspendThread(ThreadHandle) != uint.MaxValue)
                            {
                                return true;
                            }
                            else
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile sospendere un thread", EventAction.ThreadSuspension, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile determinare se un processo è a 32 bit", EventAction.ThreadSuspension, ProcessHandle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile sospendere un thread, azioni sul processo corrente non sono permesse", EventAction.ThreadSuspension);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile sospendere un thread, handle al processo non valido", EventAction.ThreadSuspension, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Riprende l'esecuzione di un thread.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ResumeThread(SafeProcessHandle ProcessHandle, IntPtr ThreadHandle)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ThreadResume, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile ripredere l'attività di un thread, azioni su processi di sistema non sono permesse", EventAction.ThreadResume, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Win32ProcessFunctions.ResumeThread(ThreadHandle) != uint.MaxValue)
                    {
                        return true;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile riprendere l'attività di un thread", EventAction.ThreadResume, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile riprendere l'attività di un thread, azioni sul processo corrente non sono permesse", EventAction.ThreadResume);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile ripredere l'attività di un thread, handle al processo non valido", EventAction.ThreadResume, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Recupera l'affinità di un thread.
        /// </summary>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>Un valore a 64 bit che rappresenta l'affinità del thread.</returns>
        public static ulong? GetThreadAffinity(IntPtr ThreadHandle)
        {
            if (ThreadHandle != IntPtr.Zero)
            {
                IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.THREAD_BASIC_INFORMATION)));
                uint Result = Win32ProcessFunctions.NtQueryInformationThread(ThreadHandle, Win32Enumerations.ThreadInformationClass.ThreadBasicInformation, Buffer, (uint)Marshal.SizeOf(typeof(Win32Structures.THREAD_BASIC_INFORMATION)), out _);
                if (Result == 0)
                {
                    Win32Structures.THREAD_BASIC_INFORMATION BasicInfo = (Win32Structures.THREAD_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.THREAD_BASIC_INFORMATION));
                    Marshal.FreeHGlobal(Buffer);
                    return (uint)BasicInfo.AffinityMask.ToInt64();
                }
                else
                {
                    Marshal.FreeHGlobal(Buffer);
                    LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile determinare l'affinità di un thread", EventAction.ThreadPropertiesRead, null, Result);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile determinare l'affinità di un thread, handle al thread non valido", EventAction.ThreadPropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Enumera le finestre appartenenti a un thread e recupera informazioni su di esse.
        /// </summary>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <param name="Handle">Handle al processo.</param>
        /// <returns>Un array di istanze di <see cref="WindowInfo"/> con le informazioni, nullo in caso di errore.</returns>
        public static WindowInfo[] GetThreadWindows(IntPtr ThreadHandle, SafeProcessHandle Handle)
        {
            if (Handle != null && !Handle.IsInvalid)
            {
                if (ThreadHandle != IntPtr.Zero)
                {
                    List<WindowInfo> WindowsInfo = new();
                    uint TID = Win32ProcessFunctions.GetThreadID(ThreadHandle);
                    if (TID != 0)
                    {
                        EnumThreadWindows(TID);
                        if (WindowHandles.Count > 0)
                        {
                            foreach (IntPtr windowhandle in WindowHandles)
                            {
                                WindowsInfo.Add(GetWindowInfo(windowhandle, Handle));
                            }
                        }
                        WindowHandles.Clear();
                        return WindowsInfo.ToArray();
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare l'ID di un thread", EventAction.ThreadWindowsEnum, Handle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile enumerare le finestre associate a un thread, handle al thread non valido", EventAction.ThreadWindowsEnum, Handle);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile enumerare le finestre associate a un thread, handle al processo non valido", EventAction.ThreadWindowsEnum, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Imposta la priorità di un thread.
        /// </summary>
        /// <param name="ProcessSafeHandle">Handle al processo.</param>
        /// <param name="ThreadPriority">Priorità del thread.</param>
        /// <param name="ThreadHandle">Handle nativo al thread.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetThreadPriority(SafeProcessHandle ProcessSafeHandle, string ThreadPriority, IntPtr ThreadHandle)
        {
            if (ProcessSafeHandle != null && !ProcessSafeHandle.IsInvalid)
            {
                if (ThreadHandle != IntPtr.Zero)
                {
                    if (Settings.SafeMode)
                    {
                        if (!Win32ProcessFunctions.IsProcessCritical(ProcessSafeHandle.DangerousGetHandle(), out bool IsCritical))
                        {
                            //Se il controllo non è riuscito, non viene eseguita alcuna azione e l'operazione è considerata fallita.
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                        else
                        {
                            if (IsCritical)
                            {
                                //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un thread, azioni su processi di sistema non sono permesse", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                        }
                    }
                    IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                    if (Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessSafeHandle.DangerousGetHandle()))
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un thread, azioni sul processo corrente non sono permesse", EventAction.ThreadPropertiesManipulation);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        switch (ThreadPriority)
                        {
                            case "Critica":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_TIME_CRITICAL))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Alta":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_HIGHEST))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Sopra il normale":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_ABOVE_NORMAL))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Normale":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_NORMAL))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Sotto al normale":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_BELOW_NORMAL))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Bassa":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_LOWEST))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            case "Inattivo":
                                if (Win32ProcessFunctions.SetThreadPriority(ThreadHandle, Win32Enumerations.ThreadPriority.THREAD_PRIORITY_IDLE))
                                {
                                    return true;
                                }
                                else
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare la priorità di un thread", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    return false;
                                }
                            default:
                                return false;
                        }
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un thread, handle al thread non valido", EventAction.ThreadPropertiesManipulation, ProcessSafeHandle);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile impostare la priorità di un thread, handle al processo non valido", EventAction.ThreadPropertiesManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Memory Management Methods
        /// <summary>
        /// Cambia la protezione di una regione di memoria.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata alla regione.</param>
        /// <param name="NewProtection">Nuova protezione da applicare.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ChangeMemoryRegionProtection(SafeProcessHandle ProcessHandle, MemoryRegionInfo Info, string NewProtection, out string OldProtection)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        OldProtection = null;
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare la protezione di una regione di memoria, azioni su processi di sistema non sono permesse", EventAction.MemoryInfoManipulation, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            OldProtection = null;
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Info.PagesState != Enum.GetName(typeof(Win32Enumerations.MemoryPageState), Win32Enumerations.MemoryPageState.MEM_COMMIT))
                    {
                        Win32Enumerations.MemoryProtections NewProtectionValue = (Win32Enumerations.MemoryProtections)Enum.Parse(typeof(Win32Enumerations.MemoryProtections), NewProtection);
                        bool Result;
                        if (IntPtr.Size == 4)
                        {
                            Result = Win32MemoryFunctions.VirtualProtect(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt32(Info.BaseAddress, 16)), new IntPtr(Convert.ToInt32(Info.Size)), NewProtectionValue, out Win32Enumerations.MemoryProtections OldValue);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare la protezione di una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                OldProtection = null;
                                return false;
                            }
                            else
                            {
                                OldProtection = Enum.GetName(typeof(Win32Enumerations.MemoryProtections), OldValue);
                                return true;
                            }
                        }
                        else
                        {
                            Result = Win32MemoryFunctions.VirtualProtect(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt64(Info.BaseAddress, 16)), new IntPtr(Convert.ToInt32(Info.Size)), NewProtectionValue, out Win32Enumerations.MemoryProtections OldValue);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare la protezione di una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                OldProtection = null;
                                return false;
                            }
                            else
                            {
                                OldProtection = Enum.GetName(typeof(Win32Enumerations.MemoryProtections), OldValue);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare la protezione di una regione di memoria, la regione non è mappata", EventAction.MemoryInfoManipulation, ProcessHandle);
                        Logger.WriteEntry(Entry);
                        OldProtection = null;
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare la protezione di una regione di memoria, azioni sul processo corrente non sono permesse", EventAction.MemoryInfoManipulation);
                    Logger.WriteEntry(Entry);
                    OldProtection = null;
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile cambiare la protezione di una regione di memoria, handle al processo non valido", EventAction.MemoryInfoManipulation, null);
                Logger.WriteEntry(Entry);
                OldProtection = null;
                return false;
            }
        }

        /// <summary>
        /// Libera una regione di memoria.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata alla regione.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti, questo metodo ritorna true anche se la regione è già libera.</returns>
        /// <remarks>Se la regione è già libera questo metodo non effettua alcuna operazione.</remarks>
        public static bool FreeMemoryRegion(SafeProcessHandle ProcessHandle, MemoryRegionInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare una regione di memoria, azioni su processi di sistema non sono permesse", EventAction.MemoryInfoManipulation, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Info.PagesState != "MEM_FREE")
                    {
                        if (IntPtr.Size == 4)
                        {
                            bool Result = Win32MemoryFunctions.VirtualFree(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt32(Info.BaseAddress, 16)), IntPtr.Zero, Win32Enumerations.FreeOperationType.MEM_RELEASE);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            bool Result = Win32MemoryFunctions.VirtualFree(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt64(Info.BaseAddress, 16)), IntPtr.Zero, Win32Enumerations.FreeOperationType.MEM_RELEASE);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare una regione di memoria, azioni sul processo corrente non sono permesse", EventAction.MemoryInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare di una regione di memoria, handle al processo non valido", EventAction.MemoryInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Annulla la mappatura di una regione di memoria.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="MemoryRegionInfo"/> associata alla regione.</param>
        /// <returns>true se l'operazione è riuscita.</returns>
        public static bool DecommitMemoryRegion(SafeProcessHandle ProcessHandle, MemoryRegionInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile annullare la mappatura di una regione di memoria, azioni su processi di sistema non sono permesse", EventAction.MemoryInfoManipulation, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Info.PagesState != "MEM_FREE")
                    {
                        if (IntPtr.Size == 4)
                        {
                            bool Result = Win32MemoryFunctions.VirtualFree(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt32(Info.BaseAddress, 16)), IntPtr.Zero, Win32Enumerations.FreeOperationType.MEM_DECOMMIT);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile annullare la mappatura una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            bool Result = Win32MemoryFunctions.VirtualFree(ProcessHandle.DangerousGetHandle(), new IntPtr(Convert.ToInt64(Info.BaseAddress, 16)), IntPtr.Zero, Win32Enumerations.FreeOperationType.MEM_DECOMMIT);
                            if (!Result)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile annullare la mappatura una regione di memoria", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile annullare la mappatura di una regione di memoria, la regione è libera", EventAction.MemoryInfoManipulation, ProcessHandle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile annullare la mappatura di una regione di memoria, azioni sul processo corrente non sono permesse", EventAction.MemoryInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile annullare la mappatura di una regione di memoria, handle al processo non valido", EventAction.MemoryInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Elimina il numero massimo di pagine dal working set di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool EmptyProcessWorkingSet(SafeProcessHandle ProcessHandle)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.ProcessWorkingSetCleaning, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare il working set di un processo, azioni su processi di sistema non sono permesse", EventAction.ProcessWorkingSetCleaning, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    if (Win32MemoryFunctions.EmptyWorkingSet(ProcessHandle.DangerousGetHandle()))
                    {
                        Logger.WriteEntry(BuildLogEntryForInformation("Working set di un processo liberato", EventAction.ProcessWorkingSetCleaning, ProcessHandle));
                        return true;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare il working set di un processo", EventAction.ProcessWorkingSetCleaning, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare il working set di un processo, azioni sul processo corrente non sono permesse", EventAction.ProcessWorkingSetCleaning);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile liberare il working set di un processo, handle al processo non valido", EventAction.ProcessWorkingSetCleaning, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Cambia il valore massimo del working set di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="NewSize">Nuova dimensione massima del working set.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetProcessMaximumWorkingSetSize(SafeProcessHandle ProcessHandle, ulong NewSize, IntPtr CurrentMinimumSize)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set massimo di un processo, azioni su processi di sistema non sono permesse", EventAction.MemoryInfoManipulation, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    try
                    {
                        IntPtr NewWorkingSetSize = new((long)NewSize);
                        if (Win32MemoryFunctions.SetProcessWorkingSetSize(ProcessHandle.DangerousGetHandle(), CurrentMinimumSize, NewWorkingSetSize, Win32Enumerations.ProcessWorkingSetQuotaLimitsOptions.QUOTA_LIMITS_HARDWS_MAX_ENABLE))
                        {
                            LogEntry Entry = BuildLogEntryForInformation("Modificato il working set massimo di un processo", EventAction.MemoryInfoManipulation, ProcessHandle);
                            return true;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile modificare il working set massimo di un processo", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    catch (OverflowException)
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set massimo di un processo, il nuovo valore è troppo grande", EventAction.MemoryInfoManipulation, ProcessHandle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set massimo di un processo, azioni sul processo corrente non sono permesse", EventAction.MemoryInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set massimo di un processo, handle al processo non valido", EventAction.MemoryInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Cambia il valore minimo del working set di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="NewSize">Nuova dimensione minima del working set.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetProcessMinimumWorkingSetSize(SafeProcessHandle ProcessHandle, ulong NewSize, IntPtr CurrentMaximumSize)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Settings.SafeMode)
                {
                    if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è di sistema", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                    else
                    {
                        if (IsCritical)
                        {
                            //Se il processo a cui l'handle fa riferimento è un processo di sistema, non viene eseguita alcuna operazione e l'operazione è considerata fallita.
                            LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set minimo di un processo, azioni su processi di sistema non sono permesse", EventAction.MemoryInfoManipulation, ProcessHandle);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                }
                IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
                if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
                {
                    try
                    {
                        IntPtr NewWorkingSetSize = new((long)NewSize);
                        if (Win32MemoryFunctions.SetProcessWorkingSetSize(ProcessHandle.DangerousGetHandle(), NewWorkingSetSize, CurrentMaximumSize, Win32Enumerations.ProcessWorkingSetQuotaLimitsOptions.QUOTA_LIMITS_HARDWS_MIN_ENABLE))
                        {
                            LogEntry Entry = BuildLogEntryForInformation("Modificato il working set minimo di un processo", EventAction.MemoryInfoManipulation, ProcessHandle);
                            return true;
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile modificare il working set minimo di un processo", EventAction.MemoryInfoManipulation, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            return false;
                        }
                    }
                    catch (OverflowException)
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set minimo di un processo, il nuovo valore è troppo grande", EventAction.MemoryInfoManipulation, ProcessHandle);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set minimo di un processo, azioni sul processo corrente non sono permesse", EventAction.MemoryInfoManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare il working set minimo di un processo, handle al processo non valido", EventAction.MemoryInfoManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Recupera i limiti di dimensione del working set di un processo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <returns>Una tupla con le informazioni.</returns>
        public static (IntPtr MinimumSize, IntPtr MaximumSize) GetProcessWorkingSetSize(SafeProcessHandle ProcessHandle)
        {
            IntPtr MaximumWorkingSetSize = Marshal.AllocHGlobal(IntPtr.Size);
            IntPtr MinimumWorkingSetSize = Marshal.AllocHGlobal(IntPtr.Size);
            _ = Win32MemoryFunctions.GetProcessWorkingSetSize(ProcessHandle.DangerousGetHandle(), MinimumWorkingSetSize, MaximumWorkingSetSize, out _);
            IntPtr MinimumSize = Marshal.ReadIntPtr(MinimumWorkingSetSize);
            IntPtr MaximumSize = Marshal.ReadIntPtr(MaximumWorkingSetSize);
            Marshal.FreeHGlobal(MaximumWorkingSetSize);
            Marshal.FreeHGlobal(MinimumWorkingSetSize);
            return (MinimumSize, MaximumSize);
        }
        #endregion
        #region Handle Advanced Info Methods
        /// <summary>
        /// Recupera informazioni su una window station.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo proprietario dell'handle.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <returns>Un'istanza di <see cref="WindowStationInfo"/> con le informazioni.</returns>
        public static WindowStationInfo GetWindowStationSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.USEROBJECTFLAGS));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    bool? IsVisible = null;
                    string UserSID;
                    string UserName;
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_FLAGS, Buffer, BufferSize, out _))
                    {
                        Win32Structures.USEROBJECTFLAGS Flags = (Win32Structures.USEROBJECTFLAGS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.USEROBJECTFLAGS));
                        Marshal.FreeHGlobal(Buffer);
                        IsVisible = Flags.Flags == Win32Enumerations.UserObjectFlag.WSF_VISIBLE;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: WindowStation", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Marshal.FreeHGlobal(Buffer);
                    }
                    _ = Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_USER_SID, IntPtr.Zero, 0, out uint LengthNeeded);
                    Buffer = Marshal.AllocHGlobal((int)LengthNeeded);
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_USER_SID, Buffer, LengthNeeded, out _))
                    {
                        if (Win32OtherFunctions.ConvertSidToStringSid(Buffer, out IntPtr StringSid))
                        {
                            UserSID = Marshal.PtrToStringUni(StringSid);
                            if (Win32OtherFunctions.LocalFree(StringSid) != IntPtr.Zero)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria utilizzata da una funzione, nome funzione: ConvertSidToStringSidW", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                            }
                            UserName = GetAccountName(Buffer);
                        }
                        else
                        {
                            UserSID = Properties.Resources.UnavailableText;
                            UserName = Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        UserSID = Properties.Resources.UnavailableText;
                        UserName = Properties.Resources.UnavailableText;
                    }
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    Marshal.FreeHGlobal(Buffer);
                    return new WindowStationInfo(IsVisible, UserSID, UserName);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: WindowStation, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un desktop.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo proprietario dell'handle.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <returns>Un'istanza di <see cref="DesktopInfo"/> con le informazioni.</returns>
        public static DesktopInfo GetDesktopSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(uint));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    bool? IsReceivingInput = null;
                    bool? OtherUserProcessesHooksAllowed = null;
                    uint? HeapSize = null;
                    string UserSID;
                    string UserName;
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_IO, Buffer, BufferSize, out _))
                    {
                        IsReceivingInput = Marshal.ReadInt32(Buffer) == 0;
                        Marshal.FreeHGlobal(Buffer);
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: Desktop", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Marshal.FreeHGlobal(Buffer);
                    }
                    Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_HEAPSIZE, Buffer, BufferSize, out _))
                    {
                        HeapSize = (uint)Marshal.ReadInt32(Buffer);
                        Marshal.FreeHGlobal(Buffer);
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: Desktop", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Marshal.FreeHGlobal(Buffer);
                    }
                    BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.USEROBJECTFLAGS));
                    Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_FLAGS, Buffer, BufferSize, out _))
                    {
                        Win32Structures.USEROBJECTFLAGS Flags = (Win32Structures.USEROBJECTFLAGS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.USEROBJECTFLAGS));
                        Marshal.FreeHGlobal(Buffer);
                        OtherUserProcessesHooksAllowed = Flags.Flags == Win32Enumerations.UserObjectFlag.DF_ALLOWOTHERACCOUNTHOOK;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: Desktop", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Marshal.FreeHGlobal(Buffer);
                    }
                    _ = Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_USER_SID, IntPtr.Zero, 0, out uint LengthNeeded);
                    Buffer = Marshal.AllocHGlobal((int)LengthNeeded);
                    if (Win32OtherFunctions.GetUserObjectInformation(NewHandle, Win32Enumerations.UserObjectInfo.UOI_USER_SID, Buffer, LengthNeeded, out _))
                    {
                        if (Win32OtherFunctions.ConvertSidToStringSid(Buffer, out IntPtr StringSid))
                        {
                            UserSID = Marshal.PtrToStringUni(StringSid);
                            if (Win32OtherFunctions.LocalFree(StringSid) != IntPtr.Zero)
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria utilizzata da una funzione, nome funzione: ConvertSidToStringSidW", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                            }
                            UserName = GetAccountName(Buffer);
                        }
                        else
                        {
                            UserSID = Properties.Resources.UnavailableText;
                            UserName = Properties.Resources.UnavailableText;
                        }
                    }
                    else
                    {
                        UserSID = Properties.Resources.UnavailableText;
                        UserName = Properties.Resources.UnavailableText;
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: Desktop", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                    }
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    Marshal.FreeHGlobal(Buffer);
                    return new DesktopInfo(IsReceivingInput, OtherUserProcessesHooksAllowed, HeapSize, UserSID, UserName);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un oggetto utente, tipo di oggetto: WindowStation, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un timer.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <param name="Update">Indica se la chiamata al metodo viene eseguita per aggiornare i dati.</param>
        /// <returns>Un'istanza di <see cref="TimerInfo"/> con le informazioni.</returns>
        public static TimerInfo GetTimerSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info, bool Update)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                long? RemainingTime = null;
                bool? TimerState = null;
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.TIMER_BASIC_INFORMATION));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    uint Result = Win32OtherFunctions.NtQueryTimer(NewHandle, Win32Enumerations.TimerInformationClass.TimerBasicInformation, Buffer, BufferSize, out _);
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.TIMER_BASIC_INFORMATION BasicInfo = (Win32Structures.TIMER_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TIMER_BASIC_INFORMATION));
                        RemainingTime = BasicInfo.RemainingTime;
                        TimerState = BasicInfo.TimerState;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un timer", EventAction.HandlePropertiesRead, ProcessHandle, Result);
                        Logger.WriteEntry(Entry);
                    }
                    Marshal.FreeHGlobal(Buffer);
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    return !Update ? new TimerInfo(RemainingTime, TimerState, ProcessHandle, Info) : new TimerInfo(RemainingTime, TimerState, null, null);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un timer, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un semaforo.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <param name="Handle">Handle nativo al semaforo (disponibile sono durante l'aggiornamento dei dati)</param>
        /// <param name="Update">Indica se la chiamata al metodo è stata eseguita per aggiornare i dati.</param>
        /// <returns>Un'istanza di <see cref="SemaphoreInfo"/> con le informazioni.</returns>
        public static SemaphoreInfo GetSemaphoreSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info, IntPtr Handle, bool Update = false)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                if (Handle == IntPtr.Zero)
                {
                    Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                    if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                    {
                        return GetSemaphoreInfo(NewHandle, Update);
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    return GetSemaphoreInfo(Handle, Update);
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un semaforo, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un semaforo.
        /// </summary>
        /// <param name="Handle">Handle nativo al semaforo.</param>
        /// <param name="Update">Indica se la chiamata al metodo è stata eseguita per aggiornare i dati.</param>
        /// <returns>Un'istanza di <see cref="SemaphoreInfo"/> con le informazioni.</returns>
        private static SemaphoreInfo GetSemaphoreInfo(IntPtr Handle, bool Update)
        {
            uint? CurrentCount = null;
            uint? MaximumCount = null;
            uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.SEMAPHORE_BASIC_INFORMATION));
            IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
            uint Result = Win32OtherFunctions.NtQuerySemaphore(Handle, Win32Enumerations.SemaphoreInformationClass.SemaphoreBasicInformation, Buffer, BufferSize, out _);
            if (Result == Win32Constants.STATUS_SUCCESS)
            {
                Win32Structures.SEMAPHORE_BASIC_INFORMATION BasicInfo = (Win32Structures.SEMAPHORE_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SEMAPHORE_BASIC_INFORMATION));
                CurrentCount = BasicInfo.CurrentCount;
                MaximumCount = BasicInfo.MaximumCount;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un semaforo", EventAction.HandlePropertiesRead, null, Result);
                Logger.WriteEntry(Entry);
            }
            Marshal.FreeHGlobal(Buffer);
            return Update ? new SemaphoreInfo(IntPtr.Zero, CurrentCount, null) : new SemaphoreInfo(Handle, CurrentCount, MaximumCount);

        }

        /// <summary>
        /// Recupera informazioni su una sezione.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <returns>Un'istanza di <see cref="SectionInfo"/> con le informazioni.</returns>
        public static SectionInfo GetSectionSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                string Attributes = Properties.Resources.UnavailableText;
                ulong? Size = null;
                IntPtr EntryPoint;
                uint? StackZeroBits;
                uint? StackReserved;
                uint? StackCommit;
                Subsystem? Subsystem;
                short? SubsystemVersionLow;
                short? SubsystemVersionHigh;
                DllCharacteristics? Characteristics;
                MachineType? MachineType;
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    bool IsImageSection = false;
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.SECTION_BASIC_INFORMATION));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    uint Result = Win32OtherFunctions.NtQuerySection(NewHandle, Win32Enumerations.SectionInformationClass.SectionBasicInformation, Buffer, BufferSize, out _);
                    while (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        BufferSize *= 2;
                        Buffer = Marshal.ReAllocHGlobal(Buffer, (IntPtr)BufferSize);
                        Result = Win32OtherFunctions.NtQuerySection(NewHandle, Win32Enumerations.SectionInformationClass.SectionBasicInformation, Buffer, BufferSize, out _);
                    }
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.SECTION_BASIC_INFORMATION BasicInfo = (Win32Structures.SECTION_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SECTION_BASIC_INFORMATION));
                        Attributes = BasicInfo.SectionAttributes.ToString("f");
                        Size = BasicInfo.SectionSize;
                        IsImageSection = BasicInfo.SectionAttributes.HasFlag(Win32Enumerations.SectionAttributes.SEC_IMAGE);
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su una sezione", EventAction.HandlePropertiesRead, null, Result);
                        Logger.WriteEntry(Entry);
                    }
                    SectionBasicInfo SectionBasicInfo = new(Attributes, Size);
                    Marshal.FreeHGlobal(Buffer);
                    SectionImageInfo SectionImageInfo = null;
                    if (IsImageSection)
                    {
                        BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.SECTION_IMAGE_INFORMATION));
                        Buffer = Marshal.AllocHGlobal((int)BufferSize);
                        Result = Win32OtherFunctions.NtQuerySection(NewHandle, Win32Enumerations.SectionInformationClass.SectionImageInformation, Buffer, BufferSize, out _);
                        if (Result == Win32Constants.STATUS_SUCCESS)
                        {
                            Win32Structures.SECTION_IMAGE_INFORMATION ImageInfo = (Win32Structures.SECTION_IMAGE_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SECTION_IMAGE_INFORMATION));
                            EntryPoint = ImageInfo.EntryPoint;
                            StackZeroBits = ImageInfo.StackZeroBits;
                            StackReserved = ImageInfo.StackReserved;
                            StackCommit = ImageInfo.StackCommit;
                            Subsystem = ImageInfo.ImageSubsystem;
                            SubsystemVersionLow = ImageInfo.SubsystemVersionLow;
                            SubsystemVersionHigh = ImageInfo.SubsystemVersionHigh;
                            Characteristics = ImageInfo.ImageCharacteristics;
                            MachineType = ImageInfo.ImageMachineType;
                            string SubsystemVersion = Convert.ToString(SubsystemVersionLow, CultureInfo.CurrentCulture) + "." + Convert.ToString(SubsystemVersionHigh, CultureInfo.CurrentCulture);
                            SectionImageInfo = new(EntryPoint, StackZeroBits, StackReserved, StackCommit, Subsystem, SubsystemVersion, Characteristics, MachineType);
                        }
                        else
                        {
                            LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su una sezione", EventAction.HandlePropertiesRead, null, Result);
                            Logger.WriteEntry(Entry);
                        }
                    }
                    else
                    {
                        SectionImageInfo = new(IntPtr.Zero, null, null, null, null, null, null, null);
                    }
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    return new SectionInfo(SectionBasicInfo, SectionImageInfo);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su una sezione, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
            
        }

        /// <summary>
        /// Recupera informazioni su un mutante.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <param name="Update">Indica se la chiamata al metodo viene eseguita per aggiornare i dati.</param>
        /// <returns>Un'istanza di <see cref="MutantInfo"/> con le informazioni.</returns>
        public static MutantInfo GetMutantSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info, bool Update)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                int? CurrentCount = null;
                bool? OwnedByCaller = null;
                bool? AbandonedState = null;
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.MUTANT_BASIC_INFORMATION));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    uint Result = Win32OtherFunctions.NtQueryMutant(NewHandle, Win32Enumerations.MutantInformationClass.MutantBasicInformation, Buffer, BufferSize, out _);
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.MUTANT_BASIC_INFORMATION BasicInfo = (Win32Structures.MUTANT_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.MUTANT_BASIC_INFORMATION));
                        CurrentCount = BasicInfo.CurrentCount;
                        OwnedByCaller = BasicInfo.OwnedByCaller;
                        AbandonedState = BasicInfo.AbandonedState;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un mutex", EventAction.HandlePropertiesRead, ProcessHandle, Result);
                        Logger.WriteEntry(Entry);
                    }
                    Marshal.FreeHGlobal(Buffer);
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    return !Update
                        ? new MutantInfo(CurrentCount, OwnedByCaller, AbandonedState, ProcessHandle, Info)
                        : new MutantInfo(CurrentCount, null, AbandonedState, null, null);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un mutex, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un mutante.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <param name="Update">Indica se la chiamata al metodo viene eseguita per aggiornare i dati.</param>
        /// <returns>Un'istanza di <see cref="EventInfo"/> con le informazioni.</returns>
        public static InfoClasses.HandleSpecificInfo.EventInfo GetEventSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info, bool Update)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle = IntPtr.Size == 4 ? (new(Convert.ToInt32(Info.Value, 16))) : (new(Convert.ToInt64(Info.Value, 16)));
                string EventType = Properties.Resources.UnavailableText;
                bool? EventState = null;
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.EVENT_BASIC_INFORMATION));
                    IntPtr Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    uint Result = Win32OtherFunctions.NtQueryEvent(NewHandle, Win32Enumerations.EventInformationClass.EventBasicInformation, Buffer, BufferSize, out _);
                    if (Result == Win32Constants.STATUS_SUCCESS)
                    {
                        Win32Structures.EVENT_BASIC_INFORMATION BasicInfo = (Win32Structures.EVENT_BASIC_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.EVENT_BASIC_INFORMATION));
                        switch (BasicInfo.EventType)
                        {
                            case Win32Enumerations.EventType.NotificationEvent:
                                EventType = "Manual Reset Event";
                                break;
                            case Win32Enumerations.EventType.SynchronizationEvent:
                                EventType = "Auto Reset Event";
                                break;
                        }
                        EventState = BasicInfo.EventState != 0;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni su un evento", EventAction.HandlePropertiesRead, ProcessHandle, Result);
                        Logger.WriteEntry(Entry);
                    }
                    Marshal.FreeHGlobal(Buffer);
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    return !Update
                        ? new InfoClasses.HandleSpecificInfo.EventInfo(EventType, EventState, ProcessHandle, Info)
                        : new InfoClasses.HandleSpecificInfo.EventInfo(null, EventState, null, null);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un evento, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni su un file.
        /// </summary>
        /// <param name="ProcessHandle">Handle al processo.</param>
        /// <param name="Info">Istanza di <see cref="HandleInfo"/> associata all'handle.</param>
        /// <returns>Un'istanza di <see cref="FileInfo"/> con le informazioni.</returns>
        public static InfoClasses.HandleSpecificInfo.FileInfo GetFileSpecificInfo(SafeProcessHandle ProcessHandle, HandleInfo Info)
        {
            if (ProcessHandle != null && !ProcessHandle.IsInvalid)
            {
                IntPtr Handle;
                if (IntPtr.Size == 4)
                {
                    Handle = new(Convert.ToInt32(Info.Value, 16));
                }
                else
                {
                    Handle = new(Convert.ToInt64(Info.Value, 16));
                }
                DateTime? CreationTime = null;
                DateTime? LastAccessTime = null;
                DateTime? LastWriteTime = null;
                string Attributes = null;
                string Type;
                long? Size;
                ulong? CompressedSize;
                long? AllocationSize;
                uint? NumberOfLinks;
                bool? DeletePending;
                bool? Directory;
                ulong? VolumeSerialNumber;
                string Identifier;
                uint ErrorCode;
                IntPtr Buffer;
                if (!string.IsNullOrWhiteSpace(Info.Name))
                {
                    Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.WIN32_FILE_ATTRIBUTE_DATA)));
                    if (Win32FileFunctions.GetFileAttributes(Info.Name, Win32Enumerations.FileAttributesInfoLevel.GetFileExInfoStandard, Buffer))
                    {
                        Win32Structures.WIN32_FILE_ATTRIBUTE_DATA AttributeData = (Win32Structures.WIN32_FILE_ATTRIBUTE_DATA)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.WIN32_FILE_ATTRIBUTE_DATA));
                        Marshal.DestroyStructure(Buffer, typeof(Win32Structures.FILETIME));
                        Marshal.FreeHGlobal(Buffer);
                        CreationTime = FileTimeToDateTime(AttributeData.CreationTime);
                        LastAccessTime = FileTimeToDateTime(AttributeData.LastAccessTime);
                        LastWriteTime = FileTimeToDateTime(AttributeData.LastWriteTime);
                        Attributes = AttributeData.FileAttributes.ToString("g");
                    }
                    else
                    {
                        Marshal.FreeHGlobal(Buffer);
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare gli attributi di un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        CreationTime = null;
                        LastAccessTime = null;
                        LastWriteTime = null;
                        Attributes = null;
                    }
                }
                if (Win32OtherFunctions.DuplicateHandle(ProcessHandle.DangerousGetHandle(), Handle, Win32OtherFunctions.GetCurrentProcess(), out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                {
                    Win32Enumerations.FileType2 FileType = Win32FileFunctions.GetFileType(NewHandle);
                    if (FileType != Win32Enumerations.FileType2.Unknown)
                    {
                        Type = FileType.ToString("g");
                    }
                    else
                    {
                        ErrorCode = (uint)Marshal.GetLastWin32Error();
                        if (ErrorCode == Win32Constants.NO_ERROR)
                        {
                            Type = FileType.ToString("g");
                        }
                        else
                        {
                            Win32Exception ex = new((int)ErrorCode);
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare il tipo di un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            Type = null;
                        }
                    }
                    if (Win32FileFunctions.GetFileSize(NewHandle, out long FileSize))
                    {
                        Size = FileSize;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare la dimensione di un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Size = null;
                    }
                    uint CompressedFileSize = Win32FileFunctions.GetCompressedFileSize(Info.Name, out IntPtr FileSizeHigh);
                    if (CompressedFileSize == Win32Constants.INVALID_FILE_SIZE)
                    {
                        if (FileSizeHigh == IntPtr.Zero)
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare la dimensione effettiva di un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            CompressedSize = null;
                        }
                        else
                        {
                            ErrorCode = (uint)Marshal.GetLastWin32Error();
                            if (ErrorCode != Win32Constants.NO_ERROR)
                            {
                                Win32Exception ex = new((int)ErrorCode);
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare la dimensione effettiva di un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                CompressedSize = null;
                            }
                            else
                            {
                                uint CompressedFileSizeHigh = (uint)Marshal.ReadInt32(FileSizeHigh);
                                CompressedSize = BuildFileSizeValue(CompressedFileSize, CompressedFileSizeHigh);
                            }
                        }
                    }
                    else
                    {
                        CompressedSize = CompressedFileSize;
                    }
                    uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.FILE_STANDARD_INFO));
                    Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    if (Win32FileFunctions.GetFileInformationByHandle(NewHandle, Win32Enumerations.FileHandleQueryClass.FileStandardInfo, Buffer, BufferSize))
                    {
                        Win32Structures.FILE_STANDARD_INFO StandardInfo = (Win32Structures.FILE_STANDARD_INFO)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.FILE_STANDARD_INFO));
                        Marshal.FreeHGlobal(Buffer);
                        AllocationSize = StandardInfo.AllocationSize;
                        NumberOfLinks = StandardInfo.NumberOfLinks;
                        DeletePending = StandardInfo.DeletePending;
                        Directory = StandardInfo.Directory;
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni su un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        AllocationSize = null;
                        NumberOfLinks = null;
                        DeletePending = null;
                        Directory = null;
                    }
                    BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.FILE_ID_INFO));
                    Buffer = Marshal.AllocHGlobal((int)BufferSize);
                    if (Win32FileFunctions.GetFileInformationByHandle(NewHandle, Win32Enumerations.FileHandleQueryClass.FileIdInfo, Buffer, BufferSize))
                    {
                        Win32Structures.FILE_ID_INFO IdInfo = (Win32Structures.FILE_ID_INFO)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.FILE_ID_INFO));
                        Marshal.DestroyStructure(Buffer, typeof(Win32Structures.FILE_ID_128));
                        Marshal.FreeHGlobal(Buffer);
                        VolumeSerialNumber = IdInfo.VolumeSerialNumber;
                        StringBuilder IdBuilder = new();
                        foreach (byte number in IdInfo.FileID.Identifier)
                        {
                            IdBuilder.Append(number.ToString("N0", CultureInfo.CurrentCulture));
                        }
                        Identifier = IdBuilder.ToString();
                    }
                    else
                    {
                        Marshal.FreeHGlobal(Buffer);
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare le informazioni su un file", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        VolumeSerialNumber = null;
                        Identifier = null;
                    }
                    _ = Win32OtherFunctions.CloseHandle(NewHandle);
                    return new InfoClasses.HandleSpecificInfo.FileInfo(CreationTime, LastAccessTime, LastWriteTime, Attributes, Type, Size, CompressedSize, AllocationSize, NumberOfLinks, DeletePending, Directory, VolumeSerialNumber, Identifier);
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile duplicare un handle", EventAction.HandlePropertiesRead, ProcessHandle, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare informazioni su un file, handle al processo non valido", EventAction.HandlePropertiesRead, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Unisce i due componenti della dimensione di un file.
        /// </summary>
        /// <param name="LowBits">32 bit meno significativi della dimensione del file.</param>
        /// <param name="HighBits">32 bit più significativi della dimensione del file.</param>
        /// <returns>La dimensione del file.</returns>
        private static ulong BuildFileSizeValue(uint LowBits, uint HighBits)
        {
            byte[] FileSize = new byte[8];
            BitArray LowBitsArray = new(BitConverter.GetBytes(LowBits));
            BitArray HighBitsArray = new(BitConverter.GetBytes(HighBits));
            BitArray FileSizeBits = new(64);
            for (int i = 0; i < LowBitsArray.Length; i++)
            {
                FileSizeBits.Set(i, LowBitsArray[i]);
            }
            for (int i = 32; i < HighBitsArray.Length; i++)
            {
                FileSizeBits.Set(i, HighBitsArray[i - 32]);
            }
            FileSizeBits.CopyTo(FileSize, 0);
            return BitConverter.ToUInt64(FileSize, 0);
        }
        #endregion
        #region Other Enumerations Methods
        /// <summary>
        /// Recupera la lista di utenti locali appartenenti ai gruppi Users e Administrators.
        /// </summary>
        /// <returns>Una lista con i nomi utente.</returns>
        public static List<string> GetLocalUsers()
        {
            List<string> Usernames = new();
            List<string> AllUsernames = new();
            uint Success = Win32UserAccountFunctions.NetUserEnum(null, Win32Enumerations.UserInfoDataType.UserAccountNames, Win32Constants.FILTER_NORMAL_ACCOUNT, out IntPtr Buffer, Win32Constants.MAX_PREFERRED_LENGTH, out uint EntriesRead, out _, out _);
            if (Success == 0)
            {
                if (EntriesRead > 0)
                {
                    IntPtr DataBuffer = Buffer;
                    for (int i = 0; i < EntriesRead; i++)
                    {
                        Win32Structures.USER_INFO_0 UserInfo = (Win32Structures.USER_INFO_0)Marshal.PtrToStructure(DataBuffer, typeof(Win32Structures.USER_INFO_0));
                        DataBuffer += Marshal.SizeOf(typeof(Win32Structures.USER_INFO_0));
                        AllUsernames.Add(UserInfo.Name);
                    }
                    if (Win32UserAccountFunctions.NetApiBufferFree(Buffer) != 0)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare il buffer allocato da una funzione di gestione rete", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                    else
                    {
                        foreach (string user in AllUsernames)
                        {
                            uint Success2 = Win32UserAccountFunctions.NetUserGetLocalGroups(null, user, 0, Win32Constants.LG_INCLUDE_INDIRECT, out IntPtr Buffer2, Win32Constants.MAX_PREFERRED_LENGTH, out uint EntriesRead2, out _);
                            if (Success2 == 0)
                            {
                                if (EntriesRead2 > 0)
                                {
                                    IntPtr DataBuffer2 = Buffer2;
                                    for (int i = 0; i < EntriesRead2; i++)
                                    {
                                        Win32Structures.LOCALGROUP_USERS_INFO_0 GroupInfo = (Win32Structures.LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(DataBuffer2, typeof(Win32Structures.LOCALGROUP_USERS_INFO_0));
                                        DataBuffer2 += Marshal.SizeOf(typeof(Win32Structures.LOCALGROUP_USERS_INFO_0));
                                        if (GroupInfo.Name == "Users" || GroupInfo.Name == "Administrators")
                                        {
                                            if (!Usernames.Exists(User => user == User))
                                            {
                                                Usernames.Add(user);
                                            }
                                        }
                                    }
                                    if (Win32UserAccountFunctions.NetApiBufferFree(Buffer2) != 0)
                                    {
                                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare il buffer allocato da una funzione di gestione rete", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                                        Logger.WriteEntry(Entry);
                                        return null;
                                    }
                                }
                                else
                                {
                                    return Usernames;
                                }
                            }
                            else
                            {
                                string ErrorString = null;
                                switch (Success2)
                                {
                                    case Win32Constants.ERROR_ACCESS_DENIED:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumAccessDeniedText;
                                        break;
                                    case Win32Constants.ERROR_INVALID_LEVEL:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumInvalidLevelText;
                                        break;
                                    case Win32Constants.ERROR_INVALID_PARAMETER:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumInvalidParameterText;
                                        break;
                                    case Win32Constants.ERROR_NOT_ENOUGH_MEMORY:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumInsufficientMemoryText;
                                        break;
                                    case Win32Constants.NERR_DCNotFound:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumDCNotFoundText;
                                        break;
                                    case Win32Constants.NERR_UserNotFound:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumUserNotFoundText;
                                        break;
                                    case Win32Constants.RPC_S_SERVER_UNAVAILABLE:
                                        ErrorString = Properties.Resources.SystemErrorUserEnumRPCServerUnavailableText;
                                        break;
                                }
                                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare la lista di gruppi a cui l'utente appartiene, " + ErrorString, EventAction.OtherActions, null);
                                Logger.WriteEntry(Entry);
                                return null;
                            }
                        }
                        return Usernames;
                    }
                }
                else
                {
                    return Usernames;
                }
            }
            else
            {
                string ErrorString = null;
                switch (Success)
                {
                    case Win32Constants.ERROR_ACCESS_DENIED:
                        ErrorString = Properties.Resources.SystemErrorUserEnumAccessDeniedText;
                        break;
                    case Win32Constants.ERROR_INVALID_LEVEL:
                        ErrorString = Properties.Resources.SystemErrorUserEnumInvalidLevelText;
                        break;
                    case Win32Constants.NERR_BufTooSmall:
                        ErrorString = Properties.Resources.SystemErrorUserEnumBufferToSmallText;
                        break;
                    case Win32Constants.NERR_InvalidComputer:
                        ErrorString = Properties.Resources.SystemErrorUserEnumInvalidComputerText;
                        break;
                }
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare le informazioni sugli account del computer, " + ErrorString, EventAction.OtherActions, null);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera informazioni sulle finestre aperte.
        /// </summary>
        /// <returns>Un array di istanze di <see cref="WindowInfo"/> con le informazioni, nullo in caso di errore.</returns>
        public static WindowInfo[] GetWindowsInfo()
        {
            List<WindowInfo> WindowsInfo = new();
            _ = Win32OtherFunctions.EnumWindows(WindowsEnumerationCallback, IntPtr.Zero);
            if (WindowHandles.Count > 0)
            {
                foreach (IntPtr windowhandle in WindowHandles)
                {
                    WindowsInfo.Add(GetWindowInfo(windowhandle, null, 0));
                }
            }
            WindowHandles.Clear();
            return WindowsInfo.ToArray();
        }

        /// <summary>
        /// Recupera informazioni sui moduli caricati.
        /// </summary>
        /// <returns>Array di istanze di <see cref="ModuleInfo"/> con le informazioni.</returns>
        public static ModuleInfo[] GetLoadedModulesInfo()
        {
            List<ModuleInfo> LoadedModules = new();
            uint Size = (uint)Marshal.SizeOf(typeof(Win32Structures.RTL_PROCESS_MODULE_INFORMATION));
            IntPtr ModulesInfoBuffer = Marshal.AllocHGlobal((int)Size);
            uint Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemModuleInformation, ModulesInfoBuffer, Size, out _);
            while (Result == Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
            {
                Size *= 2;
                ModulesInfoBuffer = Marshal.ReAllocHGlobal(ModulesInfoBuffer, (IntPtr)Size);
                Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemModuleInformation, ModulesInfoBuffer, Size, out _);
            }
            if (Result == Win32Constants.STATUS_SUCCESS)
            {
                IntPtr SecondBuffer = ModulesInfoBuffer;
                uint ModulesCount = (uint)Marshal.ReadInt32(SecondBuffer);
                SecondBuffer += 4;
                Win32Structures.RTL_PROCESS_MODULE_INFORMATION Info;
                FileVersionInfo VersionInfo;
                for (int i = 0; i < ModulesCount; i++)
                {
                    Info = (Win32Structures.RTL_PROCESS_MODULE_INFORMATION)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.RTL_PROCESS_MODULE_INFORMATION));
                    SecondBuffer += Marshal.SizeOf(Info);
                    try
                    {
                        VersionInfo = FileVersionInfo.GetVersionInfo(Info.FullPathName);
                        LoadedModules.Add(new(Info.FullPathName, Info.MappedBase.ToString("X"), UtilityMethods.ConvertSizeValueToString(Info.ImageSize), VersionInfo.FileDescription));
                    }
                    catch (FileNotFoundException)
                    {
                        LoadedModules.Add(new(Info.FullPathName, Info.MappedBase.ToString("X"), UtilityMethods.ConvertSizeValueToString(Info.ImageSize), null));
                    }
                }
                Marshal.FreeHGlobal(ModulesInfoBuffer);
                return LoadedModules.ToArray();
            }
            else
            {
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare le informazioni sui moduli caricati", EventAction.ModulesEnumeration2, null, Result);
                Logger.WriteEntry(Entry);
                Marshal.FreeHGlobal(ModulesInfoBuffer);
                return null;
            }
        }
        #endregion
        #region Other Methods
        /// <summary>
        /// Abilita i privilegi necessari per il processo corrente.
        /// </summary>
        /// <param name="RequiredPrivilegesGranted">Indica se il privilegio SeDebugPrivilege, necessario per l'esecuzione dell'applicazione, è stato abilitato.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool EnableRequiredPrivilegesForCurrentProcess(out bool RequiredPrivilegesGranted)
        {
            RequiredPrivilegesGranted = true;
            IntPtr TokenHandle = GetCurrentProcessTokenForWriting();
            if (Win32SecurityFunctions.LookupPrivilegeValue(null, Win32Constants.SE_DEBUG_NAME, out Win32Structures.LUID DebugPrivilegeLUID))
            {
                Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                {
                    PrivilegeCount = 1,
                    LUID = DebugPrivilegeLUID,
                    Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                };
                if (!Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle, false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                {
                    RequiredPrivilegesGranted = false;
                    _ = Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato dei privilegi nel token, privilegi: SeDebugPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un privilegio, privilegio: SeDebugPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            if (Win32SecurityFunctions.LookupPrivilegeValue(null, Win32Constants.SE_SHUTDOWN_NAME, out Win32Structures.LUID ShutdownPrivilegeLUID))
            {
                Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                {
                    PrivilegeCount = 1,
                    LUID = ShutdownPrivilegeLUID,
                    Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                };
                if (!Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle, false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                {
                    _ = Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato dei privilegi nel token, privilegi: SeShutdownPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un privilegio, privilegio: SeShutdownPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            if (Win32SecurityFunctions.LookupPrivilegeValue(null, Win32Constants.SE_INC_WORKING_SET_NAME, out Win32Structures.LUID WorkingSetIncreasePrivilegeLUID))
            {
                Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                {
                    PrivilegeCount = 1,
                    LUID = WorkingSetIncreasePrivilegeLUID,
                    Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                };
                if (!Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle, false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                {
                    _ = Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato dei privilegi nel token, privilegi: SeIncreaseWorkingSetPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un privilegio, privilegio: SeIncreaseWorkingSetPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            if (Win32SecurityFunctions.LookupPrivilegeValue(null, Win32Constants.SE_PROF_SINGLE_PROCESS_NAME, out Win32Structures.LUID ProfileSingleProcessPrivilegeLUID))
            {
                Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                {
                    PrivilegeCount = 1,
                    LUID = ProfileSingleProcessPrivilegeLUID,
                    Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                };
                if (!Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle, false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                {
                    _ = Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato dei privilegi nel token, privilegi: SeProfileSingleProcessPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un privilegio, privilegio: SeIncreaseWorkingSetPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            if (Win32SecurityFunctions.LookupPrivilegeValue(null, Win32Constants.SE_INCREASE_QUOTA_NAME, out Win32Structures.LUID IncreaseQuotaPrivilegeLUID))
            {
                Win32Structures.TOKEN_PRIVILEGES2 NewPrivileges = new()
                {
                    PrivilegeCount = 1,
                    LUID = IncreaseQuotaPrivilegeLUID,
                    Attributes = Win32Enumerations.PrivilegeLUIDAttributes.SE_PRIVILEGE_ENABLED
                };
                if (!Win32TokenFunctions.AdjustTokenPrivileges(TokenHandle, false, ref NewPrivileges, 0, IntPtr.Zero, out _))
                {
                    _ = Win32OtherFunctions.CloseHandle(TokenHandle);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile cambiare lo stato dei privilegi nel token, privilegi: SeIncreaseQuotaPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un privilegio, privilegio: SeIncreaseQuotaPrivilege", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            _ = Win32OtherFunctions.CloseHandle(TokenHandle);
            return true;
        }
        #region Handle Methods
        /// <summary>
        /// Recupera l'handle di un processo in esecuzione.
        /// </summary>
        /// <param name="ProcessID">PID del processo.</param>
        /// <returns>Handle al processo, se si è verificato un errore l'handle restituito da questo metodo non è valido.</returns>
        public static SafeProcessHandle GetProcessHandle(uint ProcessID)
        {
            if (ProcessID == 0)
            {
                return new(IntPtr.Zero, false);
            }
            else
            {
                IntPtr UnsafeProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessID);
                if (UnsafeProcessHandle == IntPtr.Zero)
                {
                    UnsafeProcessHandle = Win32ProcessFunctions.OpenProcess(Win32Enumerations.ProcessAccessRights.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessID);
                    if (UnsafeProcessHandle == IntPtr.Zero)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un processo", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return new SafeProcessHandle(UnsafeProcessHandle, false);
                    }
                    else
                    {
                        return new SafeProcessHandle(UnsafeProcessHandle, true);
                    }
                }
                else
                {
                    return new SafeProcessHandle(UnsafeProcessHandle, true);
                }
            }
        }

        /// <summary>
        /// Chiude un handle aperto da un altro processo.
        /// </summary>
        /// <param name="RemoteProcessHandle">Handle al processo.</param>
        /// <param name="Handle">Handle da chiudere.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool CloseRemoteHandle(SafeProcessHandle RemoteProcessHandle, IntPtr Handle)
        {
            if (RemoteProcessHandle != null && !RemoteProcessHandle.IsInvalid)
            {
                bool Result = Win32OtherFunctions.DuplicateHandle(RemoteProcessHandle.DangerousGetHandle(), Handle, IntPtr.Zero, out IntPtr NewHandle, 0, false, Win32Enumerations.DuplicateHandleOptions.DUPLICATE_CLOSE_SOURCE);
                if (Result)
                {
                    if (Win32OtherFunctions.CloseHandle(NewHandle))
                    {
                        return true;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Handle remoto chiuso ma non è stato possibile chiudere l'handle duplicato", EventAction.HandleManipulation, RemoteProcessHandle);
                        Logger.WriteEntry(Entry);
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile chiudere un handle remoto, handle al processo remoto non valido", EventAction.HandleManipulation, null);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Chiude un handle nativo.
        /// </summary>
        /// <param name="Handle">Handle nativo da chiudere.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool CloseHandle(IntPtr Handle)
        {
            bool Result = Win32OtherFunctions.CloseHandle(Handle);
            if (Result)
            {
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile chiudere un handle", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Semaphore Acquisition/Release Methods
        /// <summary>
        /// Diminuisce il conteggio di un semaforo.
        /// </summary>
        /// <param name="SemaphoreHandle">Handle al semaforo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AcquireSemaphore(IntPtr SemaphoreHandle)
        {
            Win32Enumerations.WaitResult Result = Win32OtherFunctions.WaitForSingleObject(SemaphoreHandle, 0);
            if (Result == Win32Enumerations.WaitResult.WAIT_OBJECT_0 || Result == Win32Enumerations.WaitResult.WAIT_TIMEOUT)
            {
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile acquisire un semaforo", EventAction.SemaphoreOperation, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Aumenta il conteggio di un semaforo.
        /// </summary>
        /// <param name="SemaphoreHandle">Handle al semaforo.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ReleaseSemaphore(IntPtr SemaphoreHandle)
        {
            bool Result = Win32OtherFunctions.ReleaseSemaphore(SemaphoreHandle, 1, out _);
            if (!Result)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile rilasciare un semaforo", EventAction.SemaphoreOperation, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            return Result;
        }
        #endregion
        #region System Info Retrieval Methods
        /// <summary>
        /// Recupera la dimensione di una pagina in memoria, in bytes.
        /// </summary>
        /// <returns>La dimensione di una pagina in memoria.</returns>
        public static uint GetMemoryPageSize()
        {
            Win32ComputerInfoFunctions.GetNativeSystemInfo(out Win32Structures.SYSTEM_INFO SystemInfo);
            return SystemInfo.PageSize;
        }

        /// <summary>
        /// Recupera la dimensione della memoria di sistema.
        /// </summary>
        /// <returns>La dimensione della memoria di sistema, in MB, nullo in caso di errore.</returns>
        public static uint? GetSystemMemorySize()
        {
            Win32Structures.MEMORYSTATUSEX MemoryInfo = new()
            {
                Length = (uint)Marshal.SizeOf(typeof(Win32Structures.MEMORYSTATUSEX))
            };
            IntPtr MemoryInfoBuffer = Marshal.AllocHGlobal((int)MemoryInfo.Length);
            Marshal.StructureToPtr(MemoryInfo, MemoryInfoBuffer, false);
            if (!Win32ComputerInfoFunctions.GlobalMemoryStatus(MemoryInfoBuffer))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sull'utilizzo della memoria del sistema", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            else
            {
                MemoryInfo = (Win32Structures.MEMORYSTATUSEX)Marshal.PtrToStructure(MemoryInfoBuffer, typeof(Win32Structures.MEMORYSTATUSEX));
            }
            Marshal.FreeHGlobal(MemoryInfoBuffer);
            return (uint)(MemoryInfo.TotalPhys / 1024 / 1024);
        }

        /// <summary>
        /// Recupera il numero di core del processore.
        /// </summary>
        /// <returns>Numero di core, nullo in caso di errore.</returns>
        public static uint? GetNumberOfProcessorCores()
        {
            uint Length = 0;
            if (!Win32ComputerInfoFunctions.GetLogicalProcessorInformation(Win32Enumerations.ProcessorRelationshipType.RelationProcessorCore, IntPtr.Zero, ref Length))
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di core o di package presenti nel sistema", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
                else
                {
                    IntPtr Buffer = Marshal.AllocHGlobal((int)Length);
                    if (Win32ComputerInfoFunctions.GetLogicalProcessorInformation(Win32Enumerations.ProcessorRelationshipType.RelationProcessorCore, Buffer, ref Length))
                    {
                        IntPtr SecondBuffer = Buffer + 4;
                        int StructureSize = Marshal.ReadInt32(SecondBuffer);
                        Marshal.FreeHGlobal(Buffer);
                        return Length / (uint)StructureSize;
                    }
                    else
                    {
                        Marshal.FreeHGlobal(Buffer);
                        ErrorCode = Marshal.GetLastWin32Error();
                        Win32Exception ex = new(ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di core o di package presenti nel sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion
        #region WinEvents Hooking Methods
        /// <summary>
        /// Installa un hook che riceve gli eventi relativi alla creazione e alla distruzione di oggetti.
        /// </summary>
        /// <param name="PID">ID del processo da cui ricevere gli eventi.</param>
        /// <param name="TID">ID del thread da cui ricevere gli eventi.</param>
        /// <returns>Handle nativo all'istanza dell'hook installato, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr HookObjectCreationDestructionEvents(uint PID = 0, uint TID = 0)
        {
            WindowEventProcessingCallback = new(ProcessWindowEvent);
            return Win32OtherFunctions.SetWinEventHook((uint)Win32Enumerations.WinEvents.EVENT_OBJECT_CREATE, (uint)Win32Enumerations.WinEvents.EVENT_OBJECT_DESTROY, IntPtr.Zero, WindowEventProcessingCallback, PID, TID, Win32Enumerations.EventHookingFlags.WINEVENT_OUTOFCONTEXT);
        }

        /// <summary>
        /// Disinstalla un hook precedente installato.
        /// </summary>
        /// <param name="HookInstanceHandle">Handle nativo all'instanza dell'hook.</param>
        public static void UnhookEvent(IntPtr HookInstanceHandle)
        {
            if (HookInstanceHandle != IntPtr.Zero)
            {
                _ = Win32OtherFunctions.UnhookWinEvent(HookInstanceHandle);
                WindowEventProcessingCallback = null;
            }
        }

        /// <summary>
        /// Elabora gli eventi in arrivo da un hook.
        /// </summary>
        /// <param name="WindowEventHookHandle">Handle nativo alla funzione di hook dell'evento.</param>
        /// <param name="Event">L'evento che si è verificato.</param>
        /// <param name="WindowHandle">Handle alla finestra che ha generato l'evento, <see cref="IntPtr.Zero"/> se nessuna finestra è associata all'evento.</param>
        /// <param name="idObject">Identificatore dell'oggetto associato all'evento.</param>
        /// <param name="idChild">Identifica se l'evento è stato generato da un oggetto o da un elemento figlio di un oggetto.</param>
        /// <param name="idEventThread"></param>
        /// <param name="EventTimeMilliseconds">Tempo, in millisecondi, da quando l'evento è stato generato.</param>
        /// <remarks>Se il valore del parametro idChild ha valore <see cref="Win32Enumerations.ObjectIDs.CHILDID_SELF"/> (0), l'evento è stato generato da un oggetto, altrimenti questo valore e l'ID dell'elemento che ha generato l'evento.</remarks>
        private static void ProcessWindowEvent(IntPtr WindowEventHookHandle, Win32Enumerations.WinEvents Event, IntPtr WindowHandle, Win32Enumerations.ObjectIDs idObject, Win32Enumerations.ObjectIDs idChild, uint idEventThread, uint EventTimeMilliseconds)
        {
            if (Event is Win32Enumerations.WinEvents.EVENT_OBJECT_CREATE or Win32Enumerations.WinEvents.EVENT_OBJECT_DESTROY)
            {
                lock (LockObject)
                {
                    if (Event is Win32Enumerations.WinEvents.EVENT_OBJECT_CREATE && idObject is Win32Enumerations.ObjectIDs.OBJID_WINDOW)
                    {
                        if (WindowCreatedEvent is not null)
                        {
                            WindowCreatedEvent(WindowHandle);
                        }
                    }
                    else if (Event is Win32Enumerations.WinEvents.EVENT_OBJECT_DESTROY && idObject is Win32Enumerations.ObjectIDs.OBJID_WINDOW)
                    {
                        if (WindowDestroyedEvent is not null)
                        {
                            WindowDestroyedEvent(WindowHandle);
                        }
                    }
                }
            }
        }
        #endregion
        #endregion
        #region PE Header Parsing Methods
        /// <summary>
        /// Controlla se un immagine è a 32 o 64 bit.
        /// </summary>
        /// <param name="FullPath">Percorso completo all'immagine.</param>
        /// <returns>true se l'immagine è a 32 bit, false altrimenti.</returns>
        private static bool IsImage32Bit(string FullPath)
        {
            using FileStream fs = new(FullPath, FileMode.Open, FileAccess.Read);
            using BinaryReader br = new(fs);
            _ = fs.Seek(0x3c, SeekOrigin.Begin);
            int PEOffset = br.ReadInt32();
            _ = fs.Seek(PEOffset, SeekOrigin.Begin);
            FileHeader FileHeader = GetFileHeader(br);
            return FileHeader.Machine is MachineType.I386;
        }

        /// <summary>
        /// Recupera il checksum di un immagine eseguibile.
        /// </summary>
        /// <param name="FullPath">Percorso completo al file.</param>
        /// <param name="Is32BitImage">Indica se l'immagine è a 32 bit.</param>
        /// <returns>Il checksum dell'immagine.</returns>
        private static uint GetImageChecksum(string FullPath, bool Is32BitImage)
        {
            using FileStream fs = new(FullPath, FileMode.Open, FileAccess.Read);
            using BinaryReader br = new(fs);
            _ = fs.Seek(0x3c, SeekOrigin.Begin);
            int PEOffset = br.ReadInt32();
            _ = fs.Seek(PEOffset + 20, SeekOrigin.Begin);
            if (Is32BitImage)
            {
                _ = fs.Seek(28, SeekOrigin.Current);
                OptionalHeaderWindowsSpecific32 OptionalHeader = GetOptionalHeaderWindowsSpecific32(br);
                return OptionalHeader.Checksum;
            }
            else
            {
                _ = fs.Seek(24, SeekOrigin.Current);
                OptionalHeaderWindowsSpecific64 OptionalHeader = GetOptionalHeaderWindowsSpecific64(br);
                return OptionalHeader.Checksum;
            }
        }

        /// <summary>
        /// Recupera dati sull'intestazione PE dell'eseguibile del processo.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="FullPath"></param>
        /// <returns>Un'istanza di <see cref="PEHeaderInfo"/> con le informazioni.</returns>
        public static PEHeaderInfo GetPeHeaderInfo(SafeProcessHandle Handle, string FullPath)
        {
            List<PESectionInfo> Sections = new();
            FileHeader FileHeader;
            OptionalHeaderStandard32 OptionalHeaderStandard32;
            OptionalHeaderStandard64 OptionalHeaderStandard64;
            OptionalHeaderWindowsSpecific32 OptionalHeaderWindowsSpecific32;
            OptionalHeaderWindowsSpecific64 OptionalHeaderWindowsSpecific64;
            SectionHeader[] SectionHeaders;
            PESectionInfo SectionInfo;
            if (FullPath != Properties.Resources.UnavailableText)
            {
                using FileStream fs = new(FullPath, FileMode.Open, FileAccess.Read);
                bool? Is32BitProcess = IsProcess32Bit(Handle);
                if (Is32BitProcess.HasValue)
                {
                    using BinaryReader br = new(fs);
                    _ = fs.Seek(0x3c, SeekOrigin.Begin);
                    int PEOffset = br.ReadInt32();
                    _ = fs.Seek(PEOffset, SeekOrigin.Begin);
                    if (br.ReadUInt32() == 0x00004550)
                    {
                        FileHeader = GetFileHeader(br);
                        if (Is32BitProcess.Value)
                        {
                            OptionalHeaderStandard32 = GetOptionalHeaderStandard32(br);
                            OptionalHeaderWindowsSpecific32 = GetOptionalHeaderWindowsSpecific32(br);
                            _ = fs.Seek(8 * OptionalHeaderWindowsSpecific32.NumberOfRvaAndSizes, SeekOrigin.Current);
                            SectionHeaders = GetSectionHeaders(br, FileHeader.NumberOfSections);
                            foreach (SectionHeader header in SectionHeaders)
                            {
                                SectionInfo = new(header.Name, (IntPtr)header.VirtualAddress, header.VirtualSize);
                                Sections.Add(SectionInfo);
                            }
                            bool IsChecksumValid = ValidateImage(FullPath);
                            PEHeaderGeneralInfo GeneralInfo = new(FileHeader.Machine, FileHeader.TimeDateStamp, (IntPtr)OptionalHeaderWindowsSpecific32.ImageBase, OptionalHeaderWindowsSpecific32.Checksum, IsChecksumValid, OptionalHeaderWindowsSpecific32.Subsystem, OptionalHeaderWindowsSpecific32.MajorSubsystemVersion, OptionalHeaderWindowsSpecific32.MinorSubsystemVersion, FileHeader.Characteristics, OptionalHeaderWindowsSpecific32.DllCharacteristics, Sections);
                            return new PEHeaderInfo(GeneralInfo);
                        }
                        else
                        {
                            OptionalHeaderStandard64 = GetOptionalHeaderStandard64(br);
                            OptionalHeaderWindowsSpecific64 = GetOptionalHeaderWindowsSpecific64(br);
                            _ = fs.Seek(8 * OptionalHeaderWindowsSpecific64.NumberOfRvaAndSizes, SeekOrigin.Current);
                            SectionHeaders = GetSectionHeaders(br, FileHeader.NumberOfSections);
                            foreach (SectionHeader header in SectionHeaders)
                            {
                                SectionInfo = new(header.Name, (IntPtr)header.VirtualAddress, header.VirtualSize);
                                Sections.Add(SectionInfo);
                            }
                            bool IsChecksumValid = ValidateImage(FullPath);
                            PEHeaderGeneralInfo GeneralInfo = new(FileHeader.Machine, FileHeader.TimeDateStamp, (IntPtr)OptionalHeaderWindowsSpecific64.ImageBase, OptionalHeaderWindowsSpecific64.Checksum, IsChecksumValid, OptionalHeaderWindowsSpecific64.Subsystem, OptionalHeaderWindowsSpecific64.MajorSubsystemVersion, OptionalHeaderWindowsSpecific64.MinorSubsystemVersion, FileHeader.Characteristics, OptionalHeaderWindowsSpecific64.DllCharacteristics, Sections);
                            return new(GeneralInfo);
                        }
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile trovare l'intestazione PE dell'eseguibile di un processo", EventAction.OtherPropertiesRead, Handle);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #region Header Data Reading Methods
        /// <summary>
        /// Recupera i dati dell'intestazione standard del file.
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <returns>Un'istanza di <see cref="FileHeader"/>.</returns>
        private static FileHeader GetFileHeader(BinaryReader br)
        {
            MachineType MachineType = (MachineType)br.ReadUInt16();
            ushort NumberOfSections = br.ReadUInt16();
            uint TimedateStamp = br.ReadUInt32();
            uint PointerToSymbolTable = br.ReadUInt32();
            uint NumberOfSymbols = br.ReadUInt32();
            ushort SizeOfOptionalHeader = br.ReadUInt16();
            FileCharacteristics Characteristics = (FileCharacteristics)br.ReadUInt16();
            return new(MachineType, NumberOfSections, TimedateStamp, PointerToSymbolTable, NumberOfSymbols, SizeOfOptionalHeader, Characteristics);
        }

        /// <summary>
        /// Recupera i dati standard dell'intestazione opzionale del file (32 bit).
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <returns>Un'istanza di <see cref="OptionalHeaderStandard32"/>.</returns>
        private static OptionalHeaderStandard32 GetOptionalHeaderStandard32(BinaryReader br)
        {
            PEType Magic = (PEType)br.ReadUInt16();
            byte MajorLinkerVersion = br.ReadByte();
            byte MinorLinkerVersion = br.ReadByte();
            uint SizeOfCode = br.ReadUInt32();
            uint SizeOfInitializedData = br.ReadUInt32();
            uint SizeOfUninitializedData = br.ReadUInt32();
            uint AddressOfEntryPoint = br.ReadUInt32();
            uint BaseOfCode = br.ReadUInt32();
            uint BaseOfData = br.ReadUInt32();
            return new(Magic, MajorLinkerVersion, MinorLinkerVersion, SizeOfCode, SizeOfInitializedData, SizeOfUninitializedData, AddressOfEntryPoint, BaseOfCode, BaseOfData);
        }

        /// <summary>
        /// Recupera i dati standard dell'intestazione opzionale del file (64 bit).
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <returns>Un'istanza di <see cref="OptionalHeaderStandard64"/>.</returns>
        private static OptionalHeaderStandard64 GetOptionalHeaderStandard64(BinaryReader br)
        {
            PEType Magic = (PEType)br.ReadUInt16();
            byte MajorLinkerVersion = br.ReadByte();
            byte MinorLinkerVersion = br.ReadByte();
            uint SizeOfCode = br.ReadUInt32();
            uint SizeOfInitializedData = br.ReadUInt32();
            uint SizeOfUninitializedData = br.ReadUInt32();
            uint AddressOfEntryPoint = br.ReadUInt32();
            uint BaseOfCode = br.ReadUInt32();
            return new(Magic, MajorLinkerVersion, MinorLinkerVersion, SizeOfCode, SizeOfInitializedData, SizeOfUninitializedData, AddressOfEntryPoint, BaseOfCode);
        }

        /// <summary>
        /// Recupera i dati specifici di Windows dell'intestazione opzionale del file (32 bit).
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <returns>Un'istanza di <see cref="OptionalHeaderWindowsSpecific32"/>.</returns>
        private static OptionalHeaderWindowsSpecific32 GetOptionalHeaderWindowsSpecific32(BinaryReader br)
        {
            uint ImageBase = br.ReadUInt32();
            uint SectionAlignment = br.ReadUInt32();
            uint FileAlignment = br.ReadUInt32();
            ushort MajorOperatingSystemVersion = br.ReadUInt16();
            ushort MinorOperatingSystemVersion = br.ReadUInt16();
            ushort MajorImageVersion = br.ReadUInt16();
            ushort MinorImageVersion = br.ReadUInt16();
            ushort MajorSubsystemVersion = br.ReadUInt16();
            ushort MinorSubsystemVersion = br.ReadUInt16();
            uint Win32VersionValue = br.ReadUInt32();
            uint SizeOfImage = br.ReadUInt32();
            uint SizeOfHeaders = br.ReadUInt32();
            uint Checksum = br.ReadUInt32();
            Subsystem Subsystem = (Subsystem)br.ReadUInt16();
            DllCharacteristics DllCharacteristics = (DllCharacteristics)br.ReadUInt16();
            uint SizeOfStackReserve = br.ReadUInt32();
            uint SizeOfStackCommit = br.ReadUInt32();
            uint SizeOfHeapReserve = br.ReadUInt32();
            uint SizeOfHeapCommit = br.ReadUInt32();
            uint LoaderFlags = br.ReadUInt32();
            uint NumberOfRvaAndSizes = br.ReadUInt32();
            return new(ImageBase, SectionAlignment, FileAlignment, MajorOperatingSystemVersion, MinorOperatingSystemVersion, MajorImageVersion, MinorImageVersion, MajorSubsystemVersion, MinorSubsystemVersion, Win32VersionValue, SizeOfImage, SizeOfHeaders, Checksum, Subsystem, DllCharacteristics, SizeOfStackReserve, SizeOfStackCommit, SizeOfHeapReserve, SizeOfHeapCommit, LoaderFlags, NumberOfRvaAndSizes);
        }

        /// <summary>
        /// Recupera i dati specifici di Windows dell'intestazione opzionale del file (64 bit).
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <returns>Un'istanza di <see cref="OptionalHeaderWindowsSpecific64"/>.</returns>
        private static OptionalHeaderWindowsSpecific64 GetOptionalHeaderWindowsSpecific64(BinaryReader br)
        {
            ulong ImageBase = br.ReadUInt64();
            uint SectionAlignment = br.ReadUInt32();
            uint FileAlignment = br.ReadUInt32();
            ushort MajorOperatingSystemVersion = br.ReadUInt16();
            ushort MinorOperatingSystemVersion = br.ReadUInt16();
            ushort MajorImageVersion = br.ReadUInt16();
            ushort MinorImageVersion = br.ReadUInt16();
            ushort MajorSubsystemVersion = br.ReadUInt16();
            ushort MinorSubsystemVersion = br.ReadUInt16();
            uint Win32VersionValue = br.ReadUInt32();
            uint SizeOfImage = br.ReadUInt32();
            uint SizeOfHeaders = br.ReadUInt32();
            uint Checksum = br.ReadUInt32();
            Subsystem Subsystem = (Subsystem)br.ReadUInt16();
            DllCharacteristics DllCharacteristics = (DllCharacteristics)br.ReadUInt16();
            ulong SizeOfStackReserve = br.ReadUInt64();
            ulong SizeOfStackCommit = br.ReadUInt64();
            ulong SizeOfHeapReserve = br.ReadUInt64();
            ulong SizeOfHeapCommit = br.ReadUInt64();
            uint LoaderFlags = br.ReadUInt32();
            uint NumberOfRvaAndSizes = br.ReadUInt32();
            return new(ImageBase, SectionAlignment, FileAlignment, MajorOperatingSystemVersion, MinorOperatingSystemVersion, MajorImageVersion, MinorImageVersion, MajorSubsystemVersion, MinorSubsystemVersion, Win32VersionValue, SizeOfImage, SizeOfHeaders, Checksum, Subsystem, DllCharacteristics, SizeOfStackReserve, SizeOfStackCommit, SizeOfHeapReserve, SizeOfHeapCommit, LoaderFlags, NumberOfRvaAndSizes);
        }

        /// <summary>
        /// Recupera i dati sulle sezioni.
        /// </summary>
        /// <param name="br">Istanza di <see cref="BinaryReader"/> per la lettura del file.</param>
        /// <param name="SectionsCount">Numero di sezioni presenti.</param>
        /// <returns>Un array di strutture <see cref="SectionHeader"/>.</returns>
        private static SectionHeader[] GetSectionHeaders(BinaryReader br, uint SectionsCount)
        {
            SectionHeader[] Headers = new SectionHeader[(int)SectionsCount];
            SectionHeader Header;
            string NameFull;
            string Name;
            uint VirtualSize;
            uint VirtualAddress;
            uint SizeOfRawData;
            uint PointerToRawData;
            uint PointerToRelocations;
            uint PointerToLineNumbers;
            ushort NumberOfRelocations;
            ushort NumberOfLineNumbers;
            SectionCharacteristics Characteristics;
            byte[] StringBytes = new byte[8];
            byte Character;
            for (int i = 0; i < SectionsCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Character = br.ReadByte();
                    if (Character != 0)
                    {
                        StringBytes[j] = Character;
                    }
                }
                VirtualSize = br.ReadUInt32();
                VirtualAddress = br.ReadUInt32();
                SizeOfRawData = br.ReadUInt32();
                PointerToRawData = br.ReadUInt32();
                PointerToRelocations = br.ReadUInt32();
                PointerToLineNumbers = br.ReadUInt32();
                NumberOfRelocations = br.ReadUInt16();
                NumberOfLineNumbers = br.ReadUInt16();
                Characteristics = (SectionCharacteristics)br.ReadUInt32();
                NameFull = Encoding.UTF8.GetString(StringBytes);
                if (NameFull.Contains('\0'))
                {
                    Name = NameFull.Remove(NameFull.IndexOf('\0'));
                }
                else
                {
                    Name = NameFull;
                }
                Header = new(Name, VirtualSize, VirtualAddress, SizeOfRawData, PointerToRawData, PointerToRelocations, PointerToLineNumbers, NumberOfRelocations, NumberOfLineNumbers, Characteristics);
                Headers[i] = Header;
            }
            return Headers;
        }
        #endregion
        /// <summary>
        /// Convalida un file eseguibile.
        /// </summary>
        /// <param name="FullPath">Percorso del file.</param>
        /// <returns>true se la convalida è riuscita, false altrimenti.</returns>
        private static bool ValidateImage(string FullPath)
        {
            IntPtr FileHandle = Win32FileFunctions.CreateFile(FullPath, (uint)Win32Enumerations.GenericAccessRights.GENERIC_READ, Win32Enumerations.FileShareOptions.FILE_SHARE_READ, IntPtr.Zero, Win32Enumerations.FileCreationDisposition.OPEN_EXISTING, Win32Enumerations.FileAttributesSQOSInfoAndFlags.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            if (FileHandle != IntPtr.Zero)
            {
                IntPtr FileMappingHandle = Win32FileFunctions.CreateFileMapping(FileHandle, IntPtr.Zero, (uint)Win32Enumerations.MemoryProtections.PAGE_READONLY, 0, 0, null);
                if (FileMappingHandle != IntPtr.Zero)
                {
                    IntPtr MappedViewBase = Win32FileFunctions.MapViewOfFile(FileMappingHandle, Win32Enumerations.FileMappingAccessRightsAndFlags.FILE_MAP_READ, 0, 0, (IntPtr)0);
                    if (MappedViewBase != IntPtr.Zero)
                    {
                        if (Win32FileFunctions.GetFileSize(FileHandle, out long FileSize))
                        {
                            if (Win32FileFunctions.ChecksumMappedFile(MappedViewBase, (uint)FileSize, out uint HeaderChecksum, out uint NewChecksum) != IntPtr.Zero)
                            {
                                _ = Win32FileFunctions.UnmapViewOfFile(MappedViewBase);
                                _ = Win32OtherFunctions.CloseHandle(FileMappingHandle);
                                _ = Win32OtherFunctions.CloseHandle(FileHandle);
                                return HeaderChecksum == NewChecksum;
                            }
                            else
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile calcolare il checksum di un immagine", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                _ = Win32FileFunctions.UnmapViewOfFile(MappedViewBase);
                                _ = Win32OtherFunctions.CloseHandle(FileMappingHandle);
                                _ = Win32OtherFunctions.CloseHandle(FileHandle);
                                return false;
                            }
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare la dimensione di un file", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                            _ = Win32FileFunctions.UnmapViewOfFile(MappedViewBase);
                            _ = Win32OtherFunctions.CloseHandle(FileMappingHandle);
                            _ = Win32OtherFunctions.CloseHandle(FileHandle);
                            return false;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile mappare un file in memoria", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        _ = Win32OtherFunctions.CloseHandle(FileMappingHandle);
                        _ = Win32OtherFunctions.CloseHandle(FileHandle);
                        return false;
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato creare un oggetto file mapping per un file", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    _ = Win32OtherFunctions.CloseHandle(FileHandle);
                    return false;
                }
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un file", EventAction.OtherPropertiesRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Services Query Methods
        /// <summary>
        /// Apre un handle a Gestore Controllo Servizi.
        /// </summary>
        /// <returns>Un handle a Gestione Controllo Servizi, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr OpenServiceControlManager()
        {
            IntPtr SCMHandle = Win32ServiceFunctions.OpenServiceControlManager(null, null, Win32Enumerations.ServiceControlManagerAccessRights.SC_MANAGER_ENUMERATE_SERVICE);
            if (SCMHandle == IntPtr.Zero)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un handle a Gestione Controllo Servizi", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            return SCMHandle;
        }

        /// <summary>
        /// Apre un servizio.
        /// </summary>
        /// <param name="SCMHandle">Handle nativo a Gestione Controllo Servizi.</param>
        /// <param name="ServiceName">Nome del servizio nel database.</param>
        /// <returns>Un handle nativo al servizio, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr OpenService(IntPtr SCMHandle, string ServiceName)
        {
            if (SCMHandle != IntPtr.Zero)
            {
                IntPtr ServiceHandle = Win32ServiceFunctions.OpenService(SCMHandle, ServiceName, Win32Enumerations.ServiceAccessRights.SERVICE_QUERY_STATUS | Win32Enumerations.ServiceAccessRights.SERVICE_QUERY_CONFIG);
                if (ServiceHandle == IntPtr.Zero)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                }
                return ServiceHandle;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile aprire un servizio", EventAction.ServicesGeneral);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }
        #region Service Data Getters Methods
        /// <summary>
        /// Enumera i servizi presenti nel sistema.
        /// </summary>
        /// <param name="SCMHandle">Handle a Gestione Controllo Servizi.</param>
        /// <returns>Un array di istanze di <see cref="Service"/> con le informazioni sui servizi.</returns>
        public static List<Service> EnumerateServices(IntPtr SCMHandle)
        {
            if (SCMHandle != IntPtr.Zero)
            {
                List<Service> Services = new();
                IntPtr ServiceHandle;
                uint ResumeHandle = 0;
                if (!Win32ServiceFunctions.EnumServicesStatus(SCMHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_ENUM_PROCESS_INFO, Win32Enumerations.ServiceType.SERVICE_WIN32, Win32Enumerations.ServiceState2.SERVICE_STATE_ALL, IntPtr.Zero, 0, out uint BytesNeeded, out _, ref ResumeHandle, null))
                {
                    int LastError = Marshal.GetLastWin32Error();
                    if (LastError != Win32Constants.ERROR_MORE_DATA)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile enumerare i servizi presenti nel sistema", EventAction.ServicesEnumeration, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                IntPtr ServicesDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
                IntPtr SecondBuffer = ServicesDataBuffer;
                if (Win32ServiceFunctions.EnumServicesStatus(SCMHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_ENUM_PROCESS_INFO, Win32Enumerations.ServiceType.SERVICE_WIN32, Win32Enumerations.ServiceState2.SERVICE_STATE_ALL, ServicesDataBuffer, BytesNeeded, out _, out uint ServicesReturned, ref ResumeHandle, null))
                {
                    int StructureSize = Marshal.SizeOf(typeof(Win32Structures.ENUM_SERVICE_STATUS_PROCESS));
                    Win32Structures.ENUM_SERVICE_STATUS_PROCESS ServiceData;
                    for (int i = 0; i < ServicesReturned; i++)
                    {
                        ServiceData = (Win32Structures.ENUM_SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.ENUM_SERVICE_STATUS_PROCESS));
                        SecondBuffer += StructureSize;
                        ServiceHandle = OpenService(SCMHandle, ServiceData.ServiceName);
                        if (ServiceHandle != IntPtr.Zero)
                        {
                            Win32Structures.QUERY_SERVICE_CONFIG ServiceConfiguration;
                            List<string> Dependencies = new();
                            if (!Win32ServiceFunctions.QueryServiceConfig(ServiceHandle, IntPtr.Zero, 0, out BytesNeeded))
                            {
                                int LastError = Marshal.GetLastWin32Error();
                                if (LastError != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                                {
                                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio", EventAction.ServicesEnumeration, null, ex.NativeErrorCode, ex.Message);
                                    Logger.WriteEntry(Entry);
                                    continue;
                                }
                            }
                            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
                            if (Win32ServiceFunctions.QueryServiceConfig(ServiceHandle, ConfigurationDataBuffer, BytesNeeded, out _))
                            {
                                ServiceConfiguration = (Win32Structures.QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.QUERY_SERVICE_CONFIG));
                                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                                string Dependency;
                                IntPtr SecondDependencyBuffer;
                                if (ServiceConfiguration.Dependencies != IntPtr.Zero)
                                {
                                    SecondDependencyBuffer = ServiceConfiguration.Dependencies;
                                    Dependency = Marshal.PtrToStringUni(SecondDependencyBuffer);
                                    while (!string.IsNullOrWhiteSpace(Dependency))
                                    {
                                        Dependencies.Add(Dependency);
                                        SecondDependencyBuffer += Encoding.Unicode.GetByteCount(Dependency);
                                        Dependency = Marshal.PtrToStringUni(SecondDependencyBuffer);
                                    }
                                    for (int j = 0; j < Dependencies.Count; j++)
                                    {
                                        if (Dependencies[j].StartsWith(Win32Constants.SC_GROUP_INDENTIFIER.ToString()))
                                        {
                                            Dependencies[j] = Dependencies[j].Remove(0, 1);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Win32Exception ex = new(Marshal.GetLastWin32Error());
                                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio", EventAction.ServicesEnumeration, null, ex.NativeErrorCode, ex.Message);
                                Logger.WriteEntry(Entry);
                                continue;
                            }
                            string ServiceState = GetServiceStateAsString(ServiceData.ServiceStatusProcess.CurrentState);
                            string AcceptedControls = ServiceData.ServiceStatusProcess.ControlsAccepted.ToString("g").Replace(" ", string.Empty);
                            List<string> ServiceAcceptedControls = AcceptedControls.Split(',').ToList();
                            bool RunsInSystemProcess = ServiceData.ServiceStatusProcess.ServiceFlags == Win32Enumerations.ServiceFlags.SERVICE_RUNS_IN_SYSTEM_PROCESS;
                            string ServiceType = GetServiceTypeAsString(ServiceConfiguration.ServiceType);
                            string ServiceStartType = GetServiceStartTypeAsString(ServiceConfiguration.StartType);
                            string ServiceErrorControlMode = GetServiceErrorControlModeAsString(ServiceConfiguration.ErrorControlMode);
                            bool? DelayedAutostart = GetServiceAutoStartDelayedInfo(ServiceHandle);
                            string Description = GetServiceDescription(ServiceHandle);
                            ServiceFailureActions FailureActions = GetServiceFailureActions(ServiceHandle);
                            bool? FailureActionsOnNonCrash = GetServiceFailureActionsFlag(ServiceHandle);
                            ushort? PreferredNode = GetServicePreferredNode(ServiceHandle);
                            uint? PreshutdownTimeout = GetServicePreshutdownTimeout(ServiceHandle);
                            List<string> RequiredPrivileges = GetServiceRequiredPrivileges(ServiceHandle);
                            string SIDType = GetServiceSIDType(ServiceHandle);
                            List<ServiceTrigger> Triggers = GetServiceTriggers(ServiceHandle);
                            string ProtectionType = GetServiceLaunchProtectedType(ServiceHandle);
                            Services.Add(new Service(ServiceHandle, ServiceData.ServiceName, ServiceData.DisplayName, ServiceData.ServiceStatusProcess.ProcessID, ServiceState, ServiceType, ServiceAcceptedControls, RunsInSystemProcess, ServiceStartType, ServiceErrorControlMode, ServiceConfiguration.LoadOrderGroup, ServiceConfiguration.TagID, Dependencies, ServiceConfiguration.ServiceStartName, DelayedAutostart, Description, FailureActions, FailureActionsOnNonCrash, PreferredNode, PreshutdownTimeout, RequiredPrivileges, Triggers, ProtectionType, SIDType));
                        }
                        else
                        {
                            Win32Exception ex = new(Marshal.GetLastWin32Error());
                            LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile aprire un servizio", EventAction.ServicesEnumeration, null, ex.NativeErrorCode, ex.Message);
                            Logger.WriteEntry(Entry);
                        }
                    }
                    Marshal.DestroyStructure(ServicesDataBuffer, typeof(Win32Structures.SERVICE_STATUS_PROCESS));
                    Marshal.FreeHGlobal(ServicesDataBuffer);
                    return Services;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile enumerare i servizi presenti nel sistema", EventAction.ServicesEnumeration, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    Marshal.FreeHGlobal(ServicesDataBuffer);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile enumerare i servizi presenti nel sistema, handle a Gestione Controllo Servizi non valido", EventAction.ServicesEnumeration);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #region String Conversion Methods
        /// <summary>
        /// Recupera la modalità di controllo errori come stringa.
        /// </summary>
        /// <param name="ErrorControlMode">Modalità controlo errori.</param>
        /// <returns>La modalità controllo errori come stringa.</returns>
        private static string GetServiceErrorControlModeAsString(Win32Enumerations.ServiceErrorControlMode ErrorControlMode)
        {
            return ErrorControlMode switch
            {
                Win32Enumerations.ServiceErrorControlMode.SERVICE_ERROR_CRITICAL => Properties.Resources.ServiceErrorControlCriticalText,
                Win32Enumerations.ServiceErrorControlMode.SERVICE_ERROR_IGNORE => Properties.Resources.ServiceErrorControlIgnoreText,
                Win32Enumerations.ServiceErrorControlMode.SERVICE_ERROR_NORMAL => Properties.Resources.ServiceErrorControlNormalText,
                Win32Enumerations.ServiceErrorControlMode.SERVICE_ERROR_SEVERE => Properties.Resources.ServiceErrorControlSevereText,
                _ => null,
            };
        }

        /// <summary>
        /// Recupera la modalità di avvio come stringa.
        /// </summary>
        /// <param name="StartType">Modalità di avvio.</param>
        /// <returns>La modalità di avvio come stringa.</returns>
        private static string GetServiceStartTypeAsString(Win32Enumerations.ServiceStartType StartType)
        {
            return StartType switch
            {
                Win32Enumerations.ServiceStartType.SERVICE_AUTO_START => Properties.Resources.ServiceStartTypeAutoStartText,
                Win32Enumerations.ServiceStartType.SERVICE_DEMAND_START => Properties.Resources.ServiceStartTypeDemandStartText,
                Win32Enumerations.ServiceStartType.SERVICE_DISABLED => Properties.Resources.ServiceStartTypeDisabledText,
                _ => null,
            };
        }

        /// <summary>
        /// Recupera il tipo di un servizio come stringa.
        /// </summary>
        /// <param name="Type">Tipo di servizio.</param>
        /// <returns>Il tipo di servizio come stringa.</returns>
        private static string GetServiceTypeAsString(Win32Enumerations.ServiceType Type)
        {
            return Type switch
            {
                Win32Enumerations.ServiceType.SERVICE_WIN32_OWN_PROCESS => "Own process",
                Win32Enumerations.ServiceType.SERVICE_WIN32_SHARE_PROCESS => "Share process",
                _ => null,
            };
        }

        /// <summary>
        /// Recupera lo stato di un servizio come stringa.
        /// </summary>
        /// <param name="State">Stato del servizio.</param>
        /// <returns>Lo stato del servizio come stringa.</returns>
        private static string GetServiceStateAsString(Win32Enumerations.ServiceState State)
        {
            return State switch
            {
                Win32Enumerations.ServiceState.SERVICE_CONTINUE_PENDING => Properties.Resources.ServiceStateContinuePendingText,
                Win32Enumerations.ServiceState.SERVICE_PAUSED => Properties.Resources.ServiceStatePausedText,
                Win32Enumerations.ServiceState.SERVICE_PAUSE_PENDING => Properties.Resources.ServiceStatePausePendingText,
                Win32Enumerations.ServiceState.SERVICE_RUNNING => Properties.Resources.ServiceRunningText,
                Win32Enumerations.ServiceState.SERVICE_START_PENDING => Properties.Resources.ServiceStartPendingText,
                Win32Enumerations.ServiceState.SERVICE_STOPPED => Properties.Resources.ServiceStoppedText,
                Win32Enumerations.ServiceState.SERVICE_STOP_PENDING => Properties.Resources.ServiceStopPendingText,
                _ => null,
            };
        }
        #endregion
        #region Service Optional Configuration Getter Methods
        /// <summary>
        /// Recupera l'informazione che indica se l'avvio automatico del servizio è ritardato.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>true se l'avvio automatico del servizio è ritardato, false altrimenti, nullo in caso di errore.</returns>
        private static bool? GetServiceAutoStartDelayedInfo(IntPtr ServiceHandle)
        {
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal(4);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_DELAYED_AUTO_START_INFO, ConfigurationDataBuffer, 4, out _))
            {
                Win32Structures.SERVICE_DELAYED_AUTO_START_INFO DelayedAutoStartInfo = (Win32Structures.SERVICE_DELAYED_AUTO_START_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_DELAYED_AUTO_START_INFO));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return DelayedAutoStartInfo.DelayedAutoStart;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: ritardo avvio automatico", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera la descrizione di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>La descrizione del servizio, nullo in caso di errore.</returns>
        private static string GetServiceDescription(IntPtr ServiceHandle)
        {
            if (!Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_DESCRIPTION, IntPtr.Zero, 0, out uint BytesNeeded))
            {
                int LastError = Marshal.GetLastWin32Error();
                if (LastError != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: descrizione", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_DESCRIPTION, ConfigurationDataBuffer, BytesNeeded, out _))
            {
                Win32Structures.SERVICE_DESCRIPTION DescriptionStructure = (Win32Structures.SERVICE_DESCRIPTION)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_DESCRIPTION));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return DescriptionStructure.Description;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: descrizione", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera le azioni in caso di crash di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Un'istanza di <see cref="ServiceFailureActions"/> con le informazioni, nullo in caso di errore.</returns>
        private static ServiceFailureActions GetServiceFailureActions(IntPtr ServiceHandle)
        {
            List<ServiceFailureAction> FailureActions = new();
            string ActionType = null;
            if (!Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS, IntPtr.Zero, 0, out uint BytesNeeded))
            {
                if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazioni richiesta: azioni in caso di crash", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS, ConfigurationDataBuffer, BytesNeeded, out _))
            {
                Win32Structures.SERVICE_FAILURE_ACTIONS FailureActionsStructure = (Win32Structures.SERVICE_FAILURE_ACTIONS)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_FAILURE_ACTIONS));
                if (FailureActionsStructure.Actions != IntPtr.Zero)
                {
                    IntPtr SecondBuffer = FailureActionsStructure.Actions;
                    Win32Structures.SC_ACTION Action;
                    int StructureSize = Marshal.SizeOf(typeof(Win32Structures.SC_ACTION));
                    for (int i = 0; i < FailureActionsStructure.ActionsCount; i++)
                    {
                        Action = (Win32Structures.SC_ACTION)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.SC_ACTION));
                        SecondBuffer += StructureSize;
                        switch (Action.Type)
                        {
                            case Win32Enumerations.ServiceControlManagerAction.SC_ACTION_NONE:
                                ActionType = Properties.Resources.NoneText2;
                                break;
                            case Win32Enumerations.ServiceControlManagerAction.SC_ACTION_REBOOT:
                                ActionType = Properties.Resources.RebootText;
                                break;
                            case Win32Enumerations.ServiceControlManagerAction.SC_ACTION_RESTART:
                                ActionType = Properties.Resources.RestartServiceText;
                                break;
                            case Win32Enumerations.ServiceControlManagerAction.SC_ACTION_RUN_COMMAND:
                                ActionType = Properties.Resources.RunCommandText;
                                break;
                        }
                        FailureActions.Add(new ServiceFailureAction(ActionType, Action.Delay));
                    }
                    Marshal.DestroyStructure(FailureActionsStructure.Actions, typeof(Win32Structures.SC_ACTION));
                }
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return new ServiceFailureActions(FailureActionsStructure.ResetPeriod, FailureActionsStructure.RebootMessage, FailureActionsStructure.Command, FailureActions);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: azioni in caso di crash", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera l'informazione che indica se le azioni in caso di crash vengono eseguite anche in caso di interruzione programmata.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>true se le azioni in caso di crash vengono eseguite anche in caso di interruzione programmata, false altrimenti, nullo in caso di errore.</returns>
        private static bool? GetServiceFailureActionsFlag(IntPtr ServiceHandle)
        {
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal(4);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, ConfigurationDataBuffer, 4, out _))
            {
                Win32Structures.SERVICE_FAILURE_ACTIONS_FLAG FailureActionsFlag = (Win32Structures.SERVICE_FAILURE_ACTIONS_FLAG)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_FAILURE_ACTIONS_FLAG));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return FailureActionsFlag.FailureActionsOnNonCrashFailures;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: flag azioni in caso di crash", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il nodo preferito di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Il nodo preferito, nullo in caso di errore.</returns>
        public static ushort? GetServicePreferredNode(IntPtr ServiceHandle)
        {
            uint BufferSize = (uint)Marshal.SizeOf(typeof(Win32Structures.SERVICE_PREFERRED_NODE_INFO));
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BufferSize);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_PREFERRED_NODE, ConfigurationDataBuffer, BufferSize, out _))
            {
                Win32Structures.SERVICE_PREFERRED_NODE_INFO PreferredNode = (Win32Structures.SERVICE_PREFERRED_NODE_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_PREFERRED_NODE_INFO));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return PreferredNode.PreferredNode;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: nodo preferito", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il tempo di timeout per le azioni di prespegnimento di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Il tempo di timeout, nullo in caso di errore.</returns>
        public static uint? GetServicePreshutdownTimeout(IntPtr ServiceHandle)
        {
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal(4);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_PRESHUTDOWN_INFO, ConfigurationDataBuffer, 4, out _))
            {
                Win32Structures.SERVICE_PRESHUTDOWN_INFO PreshutdownInfo = (Win32Structures.SERVICE_PRESHUTDOWN_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_PRESHUTDOWN_INFO));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return PreshutdownInfo.PreshutdownTimeout;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: timeout azioni prespegnimento", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera i privilegi necessari a un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Lista dei privilegi richiesti al servizio, nullo in caso di errore.</returns>
        public static List<string> GetServiceRequiredPrivileges(IntPtr ServiceHandle)
        {
            if (!Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO, IntPtr.Zero, 0, out uint BytesNeeded))
            {
                if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: elenco privilegi richiesti", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO, ConfigurationDataBuffer, BytesNeeded, out _))
            {
                List<string> RequiredPrivileges = new();
                Win32Structures.SERVICE_REQUIRED_PRIVILEGES_INFO RequiredPrivilegesInfo = (Win32Structures.SERVICE_REQUIRED_PRIVILEGES_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_REQUIRED_PRIVILEGES_INFO));
                IntPtr SecondBuffer = RequiredPrivilegesInfo.RequiredPrivileges;
                if (RequiredPrivilegesInfo.RequiredPrivileges != IntPtr.Zero)
                {
                    string SinglePrivilege = Marshal.PtrToStringUni(SecondBuffer);
                    while (!string.IsNullOrWhiteSpace(SinglePrivilege))
                    {
                        RequiredPrivileges.Add(SinglePrivilege);
                        SecondBuffer += Encoding.Unicode.GetByteCount(SinglePrivilege);
                        SinglePrivilege = Marshal.PtrToStringUni(SecondBuffer);
                    }
                    Marshal.FreeHGlobal(ConfigurationDataBuffer);
                }
                return RequiredPrivileges;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: elenco privilegi richiesti", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il tipo di SID di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Il tipo di SID del servizio come stringa, nullo in caso di errore.</returns>
        public static string GetServiceSIDType(IntPtr ServiceHandle)
        {
            string SIDType = null;
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal(4);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_SERVICE_SID_INFO, ConfigurationDataBuffer, 4, out _))
            {
                Win32Structures.SERVICE_SID_INFO SIDInfo = (Win32Structures.SERVICE_SID_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_SID_INFO));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                switch (SIDInfo.ServiceSidType)
                {
                    case Win32Enumerations.ServiceSIDType.SERVICE_SID_TYPE_NONE:
                        SIDType = Properties.Resources.NoneText;
                        break;
                    case Win32Enumerations.ServiceSIDType.SERVICE_SID_TYPE_RESTRICTED:
                        SIDType = Properties.Resources.RestrictedText;
                        break;
                    case Win32Enumerations.ServiceSIDType.SERVICE_SID_TYPE_UNRESTRICTED:
                        SIDType = Properties.Resources.UnrestrictedText;
                        break;
                }
                return SIDType;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: tipo di SID", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera i trigger del servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Lista di trigger del servizio, nullo in caso di errore.</returns>
        public static List<ServiceTrigger> GetServiceTriggers(IntPtr ServiceHandle)
        {
            List<ServiceTrigger> Triggers = new();
            if (!Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_TRIGGER_INFO, IntPtr.Zero, 0, out uint BytesNeeded))
            {
                if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: triggers", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_TRIGGER_INFO, ConfigurationDataBuffer, BytesNeeded, out _))
            {
                string TriggerType;
                string Action = null;
                Guid SubType;
                Win32Structures.SERVICE_TRIGGER_INFO TriggersInfo = (Win32Structures.SERVICE_TRIGGER_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_TRIGGER_INFO));
                if (TriggersInfo.Triggers != IntPtr.Zero)
                {
                    IntPtr SecondBuffer = TriggersInfo.Triggers;
                    Win32Structures.SERVICE_TRIGGER TriggerInfo;
                    string TriggerSubType;
                    for (int i = 0; i < TriggersInfo.TriggersCount; i++)
                    {
                        TriggerInfo = (Win32Structures.SERVICE_TRIGGER)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.SERVICE_TRIGGER));
                        SecondBuffer += Marshal.SizeOf(TriggerInfo);
                        switch (TriggerInfo.Action)
                        {
                            case Win32Enumerations.ServiceTriggerAction.SERVICE_TRIGGER_ACTION_SERVICE_START:
                                Action = Properties.Resources.ServiceStartText;
                                break;
                            case Win32Enumerations.ServiceTriggerAction.SERVICE_TRIGGER_ACTION_SERVICE_STOP:
                                Action = Properties.Resources.ServiceStopText;
                                break;
                        }
                        switch (TriggerInfo.TriggerType)
                        {
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_CUSTOM:
                                TriggerType = Properties.Resources.ServiceTriggerTypeCustom;
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, Properties.Resources.ServiceTriggerSubTypeCustomText));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DEVICE_INTERFACE_ARRIVAL:
                                TriggerType = Properties.Resources.ServiceTriggerTypeDeviceInterfaceArrival;
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, Properties.Resources.ServiceTriggerSubTypeCustomText));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_DOMAIN_JOIN:
                                TriggerType = Properties.Resources.ServiceTriggerTypeDomainJoin;
                                SubType = TriggerInfo.TriggerSubType;
                                if (SubType == Win32Constants.DOMAIN_JOIN_GUID)
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeDomainJoinText;
                                }
                                else
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeDomainLeaveText;
                                }
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, TriggerSubType));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_FIREWALL_PORT_EVENT:
                                TriggerType = Properties.Resources.ServiceTriggerTypeFirewallPort;
                                SubType = TriggerInfo.TriggerSubType;
                                if (SubType == Win32Constants.FIREWALL_PORT_OPEN_GUID)
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeFirewallPortOpenText;
                                }
                                else
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeFirewallPortCloseText;
                                }
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, TriggerSubType));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_GROUP_POLICY:
                                TriggerType = Properties.Resources.ServiceTriggerTypeGroupPolicy;
                                SubType = TriggerInfo.TriggerSubType;
                                if (SubType == Win32Constants.MACHINE_POLICY_PRESENT_GUID)
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeSystemPolicyChangeText;
                                }
                                else
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeUserPolicyChangeText;
                                }
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, TriggerSubType));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_IP_ADDRESS_AVAILABILITY:
                                TriggerType = Properties.Resources.ServiceTriggerTypeIPAddressAvailability;
                                SubType = TriggerInfo.TriggerSubType;
                                if (SubType == Win32Constants.NETWORK_MANAGER_FIRST_IP_ADDRESS_ARRIVAL_GUID)
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeFirstIPAvailableText;
                                }
                                else
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeLastIPUnavailableText;
                                }
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, TriggerSubType));
                                break;
                            case Win32Enumerations.ServiceTriggerType.SERVICE_TRIGGER_TYPE_NETWORK_ENDPOINT:
                                TriggerType = Properties.Resources.ServiceTriggerTypeNetworkEndpoint;
                                SubType = TriggerInfo.TriggerSubType;
                                if (SubType == Win32Constants.NAMED_PIPE_EVENT_GUID)
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeNetworkEndpointNamedPipeText;
                                }
                                else
                                {
                                    TriggerSubType = Properties.Resources.ServiceTriggerSubTypeNetworkEndpointRPCInterfaceText;
                                }
                                Triggers.Add(new ServiceTrigger(TriggerType, Action, TriggerSubType));
                                break;
                        }
                    }
                    Marshal.DestroyStructure(TriggersInfo.Triggers, typeof(Win32Structures.SERVICE_TRIGGER));
                }
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                return Triggers;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: triggers", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il tipo di protezione di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Il tipo di protezione del servizio, nullo in caso di errore.</returns>
        public static string GetServiceLaunchProtectedType(IntPtr ServiceHandle)
        {
            string ProtectionType = null;
            IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal(4);
            if (Win32ServiceFunctions.QueryServiceOptionalConfig(ServiceHandle, Win32Enumerations.ServiceOptionalConfigurationInfoLevel.SERVICE_CONFIG_LAUNCH_PROTECTED, ConfigurationDataBuffer, 4, out _))
            {
                Win32Structures.SERVICE_LAUNCH_PROTECTED_INFO LaunchInfo = (Win32Structures.SERVICE_LAUNCH_PROTECTED_INFO)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.SERVICE_LAUNCH_PROTECTED_INFO));
                Marshal.FreeHGlobal(ConfigurationDataBuffer);
                switch (LaunchInfo.LaunchProtected)
                {
                    case Win32Enumerations.ServiceLaunchProtectionType.SERVICE_LAUNCH_PROTECTED_NONE:
                        ProtectionType = Properties.Resources.NoneText2;
                        break;
                    case Win32Enumerations.ServiceLaunchProtectionType.SERVICE_LAUNCH_PROTECTED_WINDOWS:
                        ProtectionType = "Windows";
                        break;
                    case Win32Enumerations.ServiceLaunchProtectionType.SERVICE_LAUNCH_PROTECTED_WINDOWS_LIGHT:
                        ProtectionType = "Windows Light";
                        break;
                    case Win32Enumerations.ServiceLaunchProtectionType.SERVICE_LAUNCH_PROTECTED_ANTIMALWARE_LIGHT:
                        ProtectionType = "Antimalware Light";
                        break;
                }
                return ProtectionType;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio, informazione richiesta: tipo di SID", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        /// <summary>
        /// Recupera le informazioni su un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <param name="Name">Nome del servizio.</param>
        /// <returns>Un'istanza di <see cref="Service"/> con le informazioni.</returns>
        public static Service GetServiceData(IntPtr ServiceHandle, string Name)
        {
            if (!Win32ServiceFunctions.QueryServiceStatus(ServiceHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_STATUS_PROCESS_INFO, IntPtr.Zero, 0, out uint BytesNeeded))
            {
                int LastError = Marshal.GetLastWin32Error();
                if (LastError != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare lo stato di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            IntPtr StatusBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
            if (Win32ServiceFunctions.QueryServiceStatus(ServiceHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_STATUS_PROCESS_INFO, StatusBuffer, BytesNeeded, out _))
            {
                Win32Structures.SERVICE_STATUS_PROCESS Status = (Win32Structures.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(StatusBuffer, typeof(Win32Structures.SERVICE_STATUS_PROCESS));
                Marshal.FreeHGlobal(StatusBuffer);
                Win32Structures.QUERY_SERVICE_CONFIG ServiceConfiguration;
                List<string> Dependencies = new();
                if (!Win32ServiceFunctions.QueryServiceConfig(ServiceHandle, IntPtr.Zero, 0, out BytesNeeded))
                {
                    int LastError = Marshal.GetLastWin32Error();
                    if (LastError != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                IntPtr ConfigurationDataBuffer = Marshal.AllocHGlobal((int)BytesNeeded);
                if (Win32ServiceFunctions.QueryServiceConfig(ServiceHandle, ConfigurationDataBuffer, BytesNeeded, out _))
                {
                    ServiceConfiguration = (Win32Structures.QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ConfigurationDataBuffer, typeof(Win32Structures.QUERY_SERVICE_CONFIG));
                    Marshal.FreeHGlobal(ConfigurationDataBuffer);
                    string Dependency;
                    IntPtr SecondDependencyBuffer;
                    if (ServiceConfiguration.Dependencies != IntPtr.Zero)
                    {
                        SecondDependencyBuffer = ServiceConfiguration.Dependencies;
                        Dependency = Marshal.PtrToStringUni(SecondDependencyBuffer);
                        while (!string.IsNullOrWhiteSpace(Dependency))
                        {
                            Dependencies.Add(Dependency);
                            SecondDependencyBuffer += Encoding.Unicode.GetByteCount(Dependency);
                            Dependency = Marshal.PtrToStringUni(SecondDependencyBuffer);
                        }
                        for (int j = 0; j < Dependencies.Count; j++)
                        {
                            if (Dependencies[j].StartsWith(Win32Constants.SC_GROUP_INDENTIFIER.ToString()))
                            {
                                Dependencies[j] = Dependencies[j].Remove(0, 1);
                            }
                        }
                    }
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la configurazione di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
                string ServiceState = GetServiceStateAsString(Status.CurrentState);
                string AcceptedControls = Status.ControlsAccepted.ToString("g").Replace(" ", string.Empty);
                List<string> ServiceAcceptedControls = AcceptedControls.Split(',').ToList();
                bool RunsInSystemProcess = Status.ServiceFlags == Win32Enumerations.ServiceFlags.SERVICE_RUNS_IN_SYSTEM_PROCESS;
                string ServiceType = GetServiceTypeAsString(ServiceConfiguration.ServiceType);
                string ServiceStartType = GetServiceStartTypeAsString(ServiceConfiguration.StartType);
                string ServiceErrorControlMode = GetServiceErrorControlModeAsString(ServiceConfiguration.ErrorControlMode);
                bool? DelayedAutostart = GetServiceAutoStartDelayedInfo(ServiceHandle);
                string Description = GetServiceDescription(ServiceHandle);
                ServiceFailureActions FailureActions = GetServiceFailureActions(ServiceHandle);
                bool? FailureActionsOnNonCrash = GetServiceFailureActionsFlag(ServiceHandle);
                ushort? PreferredNode = GetServicePreferredNode(ServiceHandle);
                uint? PreshutdownTimeout = GetServicePreshutdownTimeout(ServiceHandle);
                List<string> RequiredPrivileges = GetServiceRequiredPrivileges(ServiceHandle);
                string SIDType = GetServiceSIDType(ServiceHandle);
                List<ServiceTrigger> Triggers = GetServiceTriggers(ServiceHandle);
                string ProtectionType = GetServiceLaunchProtectedType(ServiceHandle);
                return new Service(ServiceHandle, Name, ServiceConfiguration.DisplayName, Status.ProcessID, ServiceState, ServiceType, ServiceAcceptedControls, RunsInSystemProcess, ServiceStartType, ServiceErrorControlMode, ServiceConfiguration.LoadOrderGroup, ServiceConfiguration.TagID, Dependencies, ServiceConfiguration.ServiceStartName, DelayedAutostart, Description, FailureActions, FailureActionsOnNonCrash, PreferredNode, PreshutdownTimeout, RequiredPrivileges, Triggers, ProtectionType, SIDType);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare lo stato di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
        #endregion
        /// <summary>
        /// Registra un callback per gestire un evento relativo alla creazione o all'eliminazione di un servizio.
        /// </summary>
        /// <param name="SCMHandle">Handle nativo a Gestione Controllo Servizi.</param>
        /// <param name="Callback">Callback che gestirà l'evento.</param>
        /// <param name="ClientLagging">Indica se il client delle notifiche non riesce a stare al passo con lo stato del sistema.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se il client delle notifiche non riesce a stare al passo con lo stato del sistema è necessario chiudere l'handle nativo a Gestore Controllo Servizi, aprirne uno nuovo e riprovare.</remarks>
        public static bool RegisterCallbackForServiceCreationAndDeletion(IntPtr SCMHandle, ServiceStatusChangeCallback Callback, out bool ClientLagging)
        {
            if (SCMHandle != IntPtr.Zero)
            {
                Win32Structures.SERVICE_NOTIFY_2 NotificationBuffer = new()
                {
                    Version = 2,
                    NotifyCallback = Callback,
                    Context = IntPtr.Zero
                };
                IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(NotificationBuffer));
                Marshal.StructureToPtr(NotificationBuffer, StructurePointer, false);
                uint Result = Win32ServiceFunctions.NotifyServiceStatusChange(SCMHandle, Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_CREATED | Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETED, StructurePointer);
                if (Result == Win32Constants.ERROR_SUCCESS)
                {
                    ClientLagging = false;
                    return true;
                }
                else if (Result == Win32Constants.ERROR_SERVICE_NOTIFY_CLIENT_LAGGING)
                {
                    ClientLagging = true;
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile registrare un callback per gli eventi di creazione ed eliminazione di servizi, le notifiche dei servizi non riescono a tenere il passo con lo stato del sistema", EventAction.ServicesEnumeration);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    ClientLagging = false;
                    Win32Exception ex = new((int)Result);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile registrare un callback per gli eventi di creazione ed eliminazione di servizi", EventAction.ServicesMonitoring, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile registrare un callback per gli eventi di creazione ed eliminazione di servizi, handle a Gestione Controllo Servizi non valido", EventAction.ServicesEnumeration);
                Logger.WriteEntry(Entry);
                ClientLagging = false;
                return false;
            }
        }
        #region Data Getter Methods For Service Creation And Deletion
        /// <summary>
        /// Recupera i nomi dei servizi coinvolti in una notifica relativa alla creazione o eliminazione di servizi.
        /// </summary>
        /// <param name="Buffer">Punatatore a una struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> con le informazioni.</param>
        /// <returns>Un array di tuple con due elementi, l'array restituito ha due elementi.</returns>
        /// <remarks>Le tuple contengono il tipo di servizi (creati, eliminati) come primo membro e un array con i nomi come secondo membro.</remarks>
        public static (string EventType, string[] Names)[] GetCreatedDeletedServiceNames(IntPtr Buffer)
        {
            Win32Structures.SERVICE_NOTIFY_2 NotificationStructure = (Win32Structures.SERVICE_NOTIFY_2)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SERVICE_NOTIFY_2));
            (string EventType, string[] Names)[] Data = new (string EventType, string[] Names)[2];
            List<string> ServiceNames = new();
            string ServiceName = null;
            IntPtr SecondBuffer = NotificationStructure.ServiceNames;
            while (ServiceName != string.Empty)
            {
                ServiceName = Marshal.PtrToStringUni(SecondBuffer);
                SecondBuffer += Encoding.Unicode.GetByteCount(ServiceName);
                if (!string.IsNullOrWhiteSpace(ServiceName))
                {
                    ServiceNames.Add(ServiceName);
                }
            }
            if (Win32OtherFunctions.LocalFree(NotificationStructure.ServiceNames) != IntPtr.Zero)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile liberare la memoria utilizzata da una funzione, nome funzione: NotifyServiceStatusChangeW", EventAction.ServicesMonitoring, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            FreeNotificationBufferMemory(Buffer, false, true);
            List<string> CreatedServices = new();
            List<string> DeletedServices = new();
            foreach (string name in ServiceNames)
            {
                string ServiceName2;
                if (name.StartsWith("/"))
                {
                    ServiceName2 = name.Remove(0, 1);
                    CreatedServices.Add(ServiceName2);
                }
                else
                {
                    DeletedServices.Add(name);
                }
            }
            Data[0] = ("Created", CreatedServices.ToArray());
            Data[1] = ("Deleted", DeletedServices.ToArray());
            return Data;
        }
        #endregion
        #region Generic Service Data Getter Methods
        /// <summary>
        /// Recupera il nome descrittivo di un servizio.
        /// </summary>
        /// <param name="SCMHandle">Handle nativo a Gestione Controllo Servizi.</param>
        /// <param name="ServiceName">Nome del servizio nel database.</param>
        /// <returns>Il nome descrittivo del servizio, nullo in caso di errore.</returns>
        public static string GetServiceDisplayName(IntPtr SCMHandle, string ServiceName)
        {
            uint BufferSize = 0;
            StringBuilder ServiceDisplayName;
            if (SCMHandle != IntPtr.Zero)
            {
                if (!Win32ServiceFunctions.GetServiceDisplayName(SCMHandle, ServiceName, null, ref BufferSize))
                {
                    if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome descrittivo di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                ServiceDisplayName = new((int)BufferSize + 1);
                if (Win32ServiceFunctions.GetServiceDisplayName(SCMHandle, ServiceName, ServiceDisplayName, ref BufferSize))
                {
                    return ServiceDisplayName.ToString();
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome descrittivo di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il nome descrittivo di un servizio, handle a Gestione Controllo Servizi non valido", EventAction.ServicesGeneral);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera il PID e lo stato di un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo al servizio.</param>
        /// <returns>Un array di due elementi.</returns>
        /// <remarks>Il primo elemento dell'array è l'ID del processo in cui il servizio è in esecuzione, il secondo elemento è lo stato del servizio.</remarks>
        public static object[] GetServicePIDAndState(IntPtr ServiceHandle)
        {
            if (ServiceHandle != IntPtr.Zero)
            {
                object[] PIDAndState = new object[2];
                if (!Win32ServiceFunctions.QueryServiceStatus(ServiceHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_STATUS_PROCESS_INFO, IntPtr.Zero, 0, out uint BytesNeeded))
                {
                    if (Marshal.GetLastWin32Error() != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                    {
                        Win32Exception ex = new(Marshal.GetLastWin32Error());
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il PID e lo stato di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                IntPtr Buffer = Marshal.AllocHGlobal((int)BytesNeeded);
                if (Win32ServiceFunctions.QueryServiceStatus(ServiceHandle, Win32Enumerations.ServiceEnumerationInfoLevel.SC_STATUS_PROCESS_INFO, Buffer, BytesNeeded, out _))
                {
                    Win32Structures.SERVICE_STATUS_PROCESS ServiceStatus = (Win32Structures.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SERVICE_STATUS_PROCESS));
                    Marshal.FreeHGlobal(Buffer);
                    PIDAndState[0] = ServiceStatus.ProcessID;
                    PIDAndState[1] = GetServiceStateAsString(ServiceStatus.CurrentState);
                    return PIDAndState;
                }
                else
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il PID e lo stato di un servizio", EventAction.ServicesGeneral, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    Marshal.FreeHGlobal(Buffer);
                    return null;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile recuperare il PID e lo stato di un servizio", EventAction.ServicesGeneral);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Recupera lo stato di un servizio.
        /// </summary>
        /// <param name="Buffer">Puntatore a struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> con le informazioni.</param>
        /// <returns>Stato del servizio come stringa.</returns>
        public static string GetServiceStatus(IntPtr Buffer)
        {
            Win32Structures.SERVICE_NOTIFY_2 NotificationStructure = (Win32Structures.SERVICE_NOTIFY_2)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SERVICE_NOTIFY_2));
            return GetServiceStateAsString(NotificationStructure.ServiceStatus.CurrentState);
        }
        #endregion
        /// <summary>
        /// Registra un callback per gestire un evento relativo a un servizio.
        /// </summary>
        /// <param name="ServiceHandle">Handle nativo a un servizio.</param>
        /// <param name="Callback">Callback che gestirà l'evento.</param>
        /// <param name="MarkedForDeletion">Indica se il servizio deve essere eliminato.</param>
        /// <param name="ServiceName">Nome del servizio nel database</param>
        /// <param name="ServiceState">Stato del servizio.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        /// <remarks>Se il servizio deve essere eliminato è necessario chiudere l'handle nativo che si riferisce ad esso.</remarks>
        public static bool RegisterCallbackForServiceEvent(IntPtr ServiceHandle, ServiceStatusChangeCallback Callback, string ServiceState, out bool MarkedForDeletion, string ServiceName)
        {
            Win32Structures.SERVICE_NOTIFY_2 NotificationBuffer = new()
            {
                Version = 2,
                NotifyCallback = Callback,
                Context = Marshal.StringToHGlobalUni(ServiceName)
            };
            IntPtr StructurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(NotificationBuffer));
            Marshal.StructureToPtr(NotificationBuffer, StructurePointer, false);
            uint Result;
            Win32Enumerations.ServiceNotificationReasons Reasons = BuildServiceNotificationValue(ServiceState);
            Result = Win32ServiceFunctions.NotifyServiceStatusChange(ServiceHandle, Reasons, StructurePointer);
            if (Result == Win32Constants.ERROR_SUCCESS)
            {
                MarkedForDeletion = false;
                return true;
            }
            else if (Result == Win32Constants.ERROR_SERVICE_MARKED_FOR_DELETE)
            {
                MarkedForDeletion = true;
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile registrare un callback per gli eventi relativi a un servizio, il servizio deve essere eliminato", EventAction.ServicesMonitoring);
                Logger.WriteEntry(Entry);
                return false;
            }
            else
            {
                MarkedForDeletion = false;
                Win32Exception ex = new((int)Result);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile registrare un callback per gli eventi relativi a un servizio", EventAction.ServicesMonitoring, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Costruisce il valore da utilizzare per indicare quali notifiche ricevere da un servizio.
        /// </summary>
        /// <param name="ServiceState">Stato attuale del servizio.</param>
        /// <returns>Insieme di valori dell'enumerazione <see cref="Win32Enumerations.ServiceNotificationReasons"/>.</returns>
        private static Win32Enumerations.ServiceNotificationReasons BuildServiceNotificationValue(string ServiceState)
        {
            Win32Enumerations.ServiceNotificationReasons Reasons = Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING;
            if (ServiceState == Properties.Resources.ServiceRunningText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_RUNNING && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStoppedText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_STOPPED && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStateContinuePendingText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_CONTINUE_PENDING && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStatePausePendingText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_PAUSE_PENDING && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStatePausedText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_PAUSED && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStartPendingText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_START_PENDING && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            else if (ServiceState == Properties.Resources.ServiceStopPendingText)
            {
                foreach (uint value in Enum.GetValues(typeof(Win32Enumerations.ServiceNotificationReasons)))
                {
                    if ((Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_STOP_PENDING && (Win32Enumerations.ServiceNotificationReasons)value != Win32Enumerations.ServiceNotificationReasons.SERVICE_NOTIFY_DELETE_PENDING)
                    {
                        Reasons |= (Win32Enumerations.ServiceNotificationReasons)value;
                    }
                }
            }
            return Reasons;
        }
        #region Data Getter Methods For Service Event
        /// <summary>
        /// Recupera il nome e lo stato di un servizio.
        /// </summary>
        /// <param name="Buffer">Puntatore a struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> con le informazioni.</param>
        /// <returns>Un array con due elementi, il primo elemento è il nome del servizio, il secondo elemento è lo stato del servizio in forma di stringa.</returns>
        public static string[] GetServiceNameAndState(IntPtr Buffer)
        {
            Win32Structures.SERVICE_NOTIFY_2 NotificationStructure = (Win32Structures.SERVICE_NOTIFY_2)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SERVICE_NOTIFY_2));
            string[] Data = new string[2];
            Data[0] = Marshal.PtrToStringUni(NotificationStructure.Context);
            Marshal.FreeHGlobal(NotificationStructure.Context);
            if (NotificationStructure.NotificationStatus == Win32Constants.ERROR_SUCCESS)
            {
                Data[1] = GetServiceStateAsString(NotificationStructure.ServiceStatus.CurrentState);
            }
            return Data;
        }

        /// <summary>
        /// Controlla se un servizio deve essere eliminato.
        /// </summary>
        /// <param name="Buffer">Puntatore a una struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> con le informazioni.</param>
        /// <returns>true se il servizio deve essere eliminato, false altrimenti.</returns>
        public static bool IsServicePendingDeletion(IntPtr Buffer)
        {
            Win32Structures.SERVICE_NOTIFY_2 NotificationStructure = (Win32Structures.SERVICE_NOTIFY_2)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SERVICE_NOTIFY_2));
            bool IsServicePendingDeletion = NotificationStructure.NotificationStatus == Win32Constants.ERROR_SERVICE_MARKED_FOR_DELETE;
            FreeNotificationBufferMemory(Buffer, IsServicePendingDeletion, false);
            return IsServicePendingDeletion;
        }
        #endregion
        #region Wait Methods
        /// <summary>
        /// Resta in attesa di un evento riguardante un servizio.
        /// </summary>
        public static void WaitForServiceEvent()
        {
            _ = Win32OtherFunctions.Sleep(Win32Constants.INFINITE, true);
        }

        /// <summary>
        /// Resta in attesa di un evento riguardante un servizio.
        /// </summary>
        /// <param name="EventHandle">Handle nativo all'evento.</param>
        /// <returns>true se l'attesa si è interrotta a causa di un APC, false in caso di errore oppure se l'attesa è terminata a causa della segnalazione dell'evento.</returns>
        /// <remarks>Questo metodo restituisce il controllo anche quando l'oggetto a cui <paramref name="EventHandle"/> si riferisce passa allo stato segnalato.</remarks>
        public static bool WaitForServiceEvent(IntPtr EventHandle)
        {
            Win32Enumerations.WaitResult Result = Win32OtherFunctions.WaitForSingleObject(EventHandle, Win32Constants.INFINITE, true);
            if (Result == Win32Enumerations.WaitResult.WAIT_IO_COMPLETION)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region Other Methods
        /// <summary>
        /// Crea un evento.
        /// </summary>
        /// <returns>Handle nativo all'evento, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr CreateEvent()
        {
            IntPtr EventHandle = Win32OtherFunctions.CreateEvent(IntPtr.Zero, null, 0, (uint)Win32Enumerations.SyncObjectAccessRights.SYNC_OBJECT_MODIFY_STATE | (uint)Win32Enumerations.GenericObjectAccessRights.SYNCHRONIZE);
            if (EventHandle != IntPtr.Zero)
            {
                return EventHandle;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un evento", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Imposta un evento come segnalato.
        /// </summary>
        /// <param name="EventHandle">Handle nativo all'evento.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SetEventAsSignaled(IntPtr EventHandle)
        {
            if (Win32OtherFunctions.SetEvent(EventHandle))
            {
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile impostare un evento come segnalato", EventAction.OtherActions, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Chiude un handle nativo a un servizio o a Gestione Controllo Servizi.
        /// </summary>
        /// <param name="Handle">Handle nativo da chiudere.</param>
        public static void CloseServiceHandle(IntPtr Handle)
        {
            Win32ServiceFunctions.CloseServiceHandle(Handle);
        }

        /// <summary>
        /// Libera la memoria occupata da una struttura <see cref="Win32Structures.SERVICE_NOTIFY_2"/> non più utile.
        /// </summary>
        /// <param name="Buffer">Puntatore a memoria da liberare.</param>
        /// <param name="SCMEvent">Indica se la chiamata a questo metodo è avvenuta dopo la gestione di un evento di Gestore Controllo Servizi.</param>
        /// <param name="ServiceDeleted">Indica se il servizio è stato eliminato.</param>
        public static void FreeNotificationBufferMemory(IntPtr Buffer, bool ServiceDeleted, bool SCMEvent)
        {
            if (!ServiceDeleted && !SCMEvent)
            {
                Marshal.DestroyStructure(Buffer, typeof(Win32Structures.SERVICE_STATUS_PROCESS));
            }
            Marshal.FreeHGlobal(Buffer);
        }
        #endregion
        #endregion
        #region Computer Info And Power State Change Methods
        #region Power State Change Methods
        #region Lock Computer / Logoff User Methods
        /// <summary>
        /// Inizio il blocco del display del computer.
        /// </summary>
        /// <returns>true se l'operazione è iniziata, false altrimenti.</returns>
        public static bool LockComputer()
        {
            if (Win32ShutdownFunctions.LockWorkstation())
            {
                LogEntry Entry = BuildLogEntryForInformation("Blocco del computer iniziato", EventAction.LockMachine);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile bloccare il computer", EventAction.LockMachine, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Disconnette l'utente.
        /// </summary>
        /// <returns>true se la disconnessione è cominciata, false altrimenti.</returns>
        public static bool LogOffUser()
        {
            Win32Enumerations.ShutdownReasons Reason = Win32Enumerations.ShutdownReasons.SHTDN_REASON_MAJOR_APPLICATION | Win32Enumerations.ShutdownReasons.SHTDN_REASON_MINOR_OTHER | Win32Enumerations.ShutdownReasons.SHTDN_REASON_FLAG_PLANNED;
            Win32Enumerations.LogOffFlags Flags = Win32Enumerations.LogOffFlags.EWX_LOGOFF;
            if (Settings.ForceLogOffIfHung)
            {
                Flags |= Win32Enumerations.LogOffFlags.EWX_FORCEIFHUNG;
            }
            if (Win32ShutdownFunctions.ExitWindows(Flags, Reason))
            {
                LogEntry Entry = BuildLogEntryForInformation("Disconnessione dell'utente iniziata", EventAction.LogOffUser);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile disconnettere l'utente", EventAction.LogOffUser, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Suspend / Hibernate System Methods
        /// <summary>
        /// Causa il passaggio del sistema allo stato in standby.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SuspendSystem()
        {
            if (Win32ShutdownFunctions.SetSuspendState(false, false, false))
            {
                LogEntry Entry = BuildLogEntryForInformation("Sospensione del sistema iniziata", EventAction.SleepMachine);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile sospendere il sistema", EventAction.SleepMachine, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }

        /// <summary>
        /// Iberna il sistema.
        /// </summary>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool HibernateSystem()
        {
            if (Win32ShutdownFunctions.SetSuspendState(true, false, false))
            {
                LogEntry Entry = BuildLogEntryForInformation("Ibernazione del sistema iniziata", EventAction.HibernateMachine);
                Logger.WriteEntry(Entry);
                return true;
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile ibernare il sistema", EventAction.HibernateMachine, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Shutdown / Restart System Methods
        /// <summary>
        /// Inizia l'arresto del sistema.
        /// </summary>
        /// <param name="Hybrid">Indica se eseguire lo spegnimento ibrido.</param>
        /// <returns>true se lo spegnimento è iniziato, false altrimenti.</returns>
        public static bool ShutdownSystem(bool Hybrid)
        {
            Win32Enumerations.ShutdownReasons Reason = Win32Enumerations.ShutdownReasons.SHTDN_REASON_MAJOR_APPLICATION | Win32Enumerations.ShutdownReasons.SHTDN_REASON_MINOR_OTHER | Win32Enumerations.ShutdownReasons.SHTDN_REASON_FLAG_PLANNED;
            Win32Enumerations.ShutdownFlags Flags = 0;
            if (Hybrid)
            {
                Flags = Win32Enumerations.ShutdownFlags.SHUTDOWN_HYBRID;
            }
            if (Settings.ForceOtherSessionsLogOffOnShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_OTHERS;
            }
            if (Settings.ForceCurrentSessionLogOffOnShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_SELF;
            }
            if (Settings.InstallUpdatesBeforeShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_INSTALL_UPDATES;
            }
            if (Settings.ManualPowerDown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_NOREBOOT;
            }
            else
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_POWEROFF;
            }
            uint Result = Win32ShutdownFunctions.InitiateShutdown(null, "Shutdown initiated by Process Manager", 0, Flags, Reason);
            if (Result != Win32Constants.ERROR_SUCCESS)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile iniziare lo spegnimento del sistema", EventAction.ShutdownMachine, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForInformation("Spegnimento del sistema iniziato", EventAction.ShutdownMachine);
                Logger.WriteEntry(Entry);
                return true;
            }
        }

        /// <summary>
        /// Inizia il riavvio del sistema.
        /// </summary>
        /// <param name="RestartToBootOptions">Indica se mostrare le opzioni di boot dopo il riavvio.</param>
        /// <returns>true se il riavvio è iniziato, false altrimenti.</returns>
        public static bool RestartSystem(bool RestartToBootOptions)
        {
            Win32Enumerations.ShutdownReasons Reason = Win32Enumerations.ShutdownReasons.SHTDN_REASON_MAJOR_APPLICATION | Win32Enumerations.ShutdownReasons.SHTDN_REASON_MINOR_OTHER | Win32Enumerations.ShutdownReasons.SHTDN_REASON_FLAG_PLANNED;
            Win32Enumerations.ShutdownFlags Flags = 0;
            if (RestartToBootOptions)
            {
                Flags = Win32Enumerations.ShutdownFlags.SHUTDOWN_RESTART_BOOT_OPTIONS;
            }
            if (Settings.ForceOtherSessionsLogOffOnShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_OTHERS;
            }
            if (Settings.ForceCurrentSessionLogOffOnShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_FORCE_SELF;
            }
            if (Settings.InstallUpdatesBeforeShutdown)
            {
                Flags |= Win32Enumerations.ShutdownFlags.SHUTDOWN_INSTALL_UPDATES;
            }
            uint Result = Win32ShutdownFunctions.InitiateShutdown(null, "Restart initiated by Process Manager", 0, Flags, Reason);
            if (Result != Win32Constants.ERROR_SUCCESS)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile iniziare il riavvio del sistema", EventAction.RestartMachine, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return false;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForInformation("Riavvio del sistema iniziato", EventAction.RestartMachine);
                Logger.WriteEntry(Entry);
                return true;
            }
        }
        #endregion
        #endregion
        #region Computer Info Getter Methods
        #region Hardware Info
        /// <summary>
        /// Recupera informazioni sui processori nel sistema.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetSystemProcessorInfo()
        {
            Dictionary<string, object> Info = new();
            Win32ComputerInfoFunctions.GetNativeSystemInfo(out Win32Structures.SYSTEM_INFO SystemInfo);
            Info.Add("ActiveProcessors", GetActiveProcessorsCount(SystemInfo.ActiveProcessorMask));
            Info.Add("Architecture", Enum.GetName(typeof(Win32Enumerations.ProcessorArchitecture), SystemInfo.ProcessorArchitecture));
            Info.Add("Level", SystemInfo.ProcessorLevel);
            Info.Add("Revision", SystemInfo.ProcessorRevision);
            Info.Add("Features", GetProcessorFeatures());
            Info.Add("NumberOfCores", GetNumberOfCoresAndPackages());
            Info.Add("NumberOfPackages", GetNumberOfCoresAndPackages(true));
            return Info;
        }
        #region Processor Info Getter Methods
        /// <summary>
        /// Recupera le caratteristiche del processore.
        /// </summary>
        /// <returns>Un array che contiene le caratteristiche supportate dal processore.</returns>
        private static List<string> GetProcessorFeatures()
        {
            List<string> Features = new();
            foreach (uint feature in Enum.GetValues(typeof(Win32Enumerations.ProcessorFeature)))
            {
                if (Win32ComputerInfoFunctions.IsProcessorFeaturePresent((Win32Enumerations.ProcessorFeature)feature))
                {
                    Features.Add(UtilityMethods.GetEnumDescription((Win32Enumerations.ProcessorFeature)feature) ?? ((Win32Enumerations.ProcessorFeature)feature).ToString());
                }
            }
            return Features;
        }

        /// <summary>
        /// Recupera il numero di core o di packages presenti nel sistema.
        /// </summary>
        /// <param name="PackagesCount">Indica se deve essere recuperato il numero di package presenti nel sistema.</param>
        /// <returns>Il numero di core o di packages.</returns>
        private static uint GetNumberOfCoresAndPackages(bool PackagesCount = false)
        {
            uint Length = 0;
            Win32Enumerations.ProcessorRelationshipType RelationshipType;
            if (!PackagesCount)
            {
                RelationshipType = Win32Enumerations.ProcessorRelationshipType.RelationProcessorCore;
            }
            else
            {
                RelationshipType = Win32Enumerations.ProcessorRelationshipType.RelationProcessorePackage;
            }
            if (!Win32ComputerInfoFunctions.GetLogicalProcessorInformation(RelationshipType, IntPtr.Zero, ref Length))
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                if (ErrorCode != Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di core o di package presenti nel sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return 0;
                }
                else
                {
                    IntPtr Buffer = Marshal.AllocHGlobal((int)Length);
                    if (Win32ComputerInfoFunctions.GetLogicalProcessorInformation(RelationshipType, Buffer, ref Length))
                    {
                        IntPtr SecondBuffer = Buffer + 4;
                        int StructureSize = Marshal.ReadInt32(SecondBuffer);
                        Marshal.FreeHGlobal(Buffer);
                        return Length / (uint)StructureSize;
                    }
                    else
                    {
                        Marshal.FreeHGlobal(Buffer);
                        ErrorCode = Marshal.GetLastWin32Error();
                        Win32Exception ex = new(ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il numero di core o di package presenti nel sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return 0;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Recupera il numero di processori attivi.
        /// </summary>
        /// <param name="ActiveProcessorMask">Campo di bit con le informazioni.</param>
        /// <returns>Numero di processori attivi.</returns>
        private static byte GetActiveProcessorsCount(UIntPtr ActiveProcessorMask)
        {
            byte ActiveProcessorsCount = 0;
            BitArray Bits = new(BitConverter.GetBytes(ActiveProcessorMask.ToUInt64()));
            foreach (bool bit in Bits)
            {
                if (bit)
                {
                    ActiveProcessorsCount += 1;
                }
            }
            return ActiveProcessorsCount;
        }
        #endregion
        /// <summary>
        /// Recupera il profilo hardware attuale.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, string> GetHardwareProfileInfo()
        {
            Dictionary<string, string> Info = new();
            if (!Win32ComputerInfoFunctions.GetCurrentHwProfile(out Win32Structures.HW_PROFILE_INFO HwProfileInfo))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sull'attuale profilo hardware del computer", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("HardwareProfileGuid", Properties.Resources.UnavailableText);
                Info.Add("HardwareProfileName", Properties.Resources.UnavailableText);
            }
            else
            {
                Info.Add("HardwareProfileGuid", HwProfileInfo.HwProfileGuid);
                Info.Add("HardwareProfileName", HwProfileInfo.HwProfileName);
            }
            return Info;
        }

        /// <summary>
        /// Recupera informazioni sulla memoria fisica.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetPhysicalMemoryInfo()
        {
            Dictionary<string, object> Info = new();
            if (!Win32ComputerInfoFunctions.GetPhysicallyInstalledSystemMemory(out ulong TotalMemoryInKilobytes))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la quantità di memoria installata nel sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("PhysicalMemorySize", Properties.Resources.UnavailableText);
            }
            else
            {
                Info.Add("PhysicalMemorySize", TotalMemoryInKilobytes);
            }
            Win32Structures.MEMORYSTATUSEX MemoryInfo = new()
            {
                Length = (uint)Marshal.SizeOf(typeof(Win32Structures.MEMORYSTATUSEX))
            };
            IntPtr MemoryInfoBuffer = Marshal.AllocHGlobal((int)MemoryInfo.Length);
            Marshal.StructureToPtr(MemoryInfo, MemoryInfoBuffer, false);
            if (!Win32ComputerInfoFunctions.GlobalMemoryStatus(MemoryInfoBuffer))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sull'utilizzo della memoria del sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("MemoryLoadPercentage", Properties.Resources.UnavailableText);
                Info.Add("OSAvailableMemory", Properties.Resources.UnavailableText);
                Info.Add("CurrentlyAvailableMemory", Properties.Resources.UnavailableText);
            }
            else
            {
                MemoryInfo = (Win32Structures.MEMORYSTATUSEX)Marshal.PtrToStructure(MemoryInfoBuffer, typeof(Win32Structures.MEMORYSTATUSEX));
                Info.Add("MemoryLoadPercentage", MemoryInfo.MemoryLoad);
                Info.Add("OSAvailableMemory", MemoryInfo.TotalPhys);
                Info.Add("CurrentlyAvailableMemory", MemoryInfo.AvailPhys);
            }
            Marshal.FreeHGlobal(MemoryInfoBuffer);
            uint StructureSize = (uint)Marshal.SizeOf(typeof(Win32Structures.PERFORMANCE_INFORMATION));
            if (!Win32ComputerInfoFunctions.GetPerformanceInfo(out Win32Structures.PERFORMANCE_INFORMATION PerformanceInformation, StructureSize))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sulla performance del sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("PageSize", Properties.Resources.UnavailableText);
                Info.Add("TotalPagesCommitted", Properties.Resources.UnavailableText);
                Info.Add("PageCommitLimit", Properties.Resources.UnavailableText);
                Info.Add("PageCommitPeak", Properties.Resources.UnavailableText);
            }
            else
            {
                Info.Add("PageSize", PerformanceInformation.PageSize.ToUInt64());
                Info.Add("TotalPagesCommitted", PerformanceInformation.CommitTotal.ToUInt64());
                Info.Add("PageCommitLimit", PerformanceInformation.CommitLimit.ToUInt64());
                Info.Add("PageCommitPeak", PerformanceInformation.CommitPeak.ToUInt64());
            }
            return Info;
        }

        /// <summary>
        /// Recupera la quantità di memoria utilizzata.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetMemoryUsage()
        {
            Dictionary<string, object> Info = new();
            Win32Structures.MEMORYSTATUSEX MemoryInfo = new()
            {
                Length = (uint)Marshal.SizeOf(typeof(Win32Structures.MEMORYSTATUSEX))
            };
            IntPtr MemoryInfoBuffer = Marshal.AllocHGlobal((int)MemoryInfo.Length);
            Marshal.StructureToPtr(MemoryInfo, MemoryInfoBuffer, false);
            if (!Win32ComputerInfoFunctions.GlobalMemoryStatus(MemoryInfoBuffer))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni sull'utilizzo della memoria del sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("MemoryLoadPercentage", Properties.Resources.UnavailableText);
                Info.Add("OSAvailableMemory", Properties.Resources.UnavailableText);
                Info.Add("CurrentlyAvailableMemory", Properties.Resources.UnavailableText);
            }
            else
            {
                MemoryInfo = (Win32Structures.MEMORYSTATUSEX)Marshal.PtrToStructure(MemoryInfoBuffer, typeof(Win32Structures.MEMORYSTATUSEX));
                Info.Add("MemoryLoadPercentage", MemoryInfo.MemoryLoad);
                Info.Add("OSAvailableMemory", MemoryInfo.TotalPhys);
                Info.Add("CurrentlyAvailableMemory", MemoryInfo.AvailPhys);
            }
            Marshal.FreeHGlobal(MemoryInfoBuffer);
            return Info;
        }

        /// <summary>
        /// Recupera il nome e il tipo di firmware del comouter.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, string> GetOtherComputerInfo()
        {
            uint Size = 0;
            Dictionary<string, string> Info = new();
            if (!Win32ComputerInfoFunctions.GetComputerName(Win32Enumerations.ComputerNameFormat.PhysicalNetBIOS, null, ref Size))
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                if (ErrorCode == Win32Constants.ERROR_MORE_DATA)
                {
                    StringBuilder NameBuilder = new((int)Size);
                    if (Win32ComputerInfoFunctions.GetComputerName(Win32Enumerations.ComputerNameFormat.PhysicalNetBIOS, NameBuilder, ref Size))
                    {
                        Info.Add("ComputerName", NameBuilder.ToString());
                    }
                    else
                    {
                        Win32Exception ex = new(ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recupera il nome del computer", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        Info.Add("ComputerName", Properties.Resources.UnavailableText);
                    }
                }
                else
                {
                    Win32Exception ex = new(ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recupera il nome del computer", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    Info.Add("ComputerName", Properties.Resources.UnavailableText);
                }
            }
            else
            {
                Info.Add("ComputerName", Properties.Resources.NoneText);
            }
            if (Win32ComputerInfoFunctions.GetFirmwareType(out Win32Enumerations.FirmwareType FirmwareType))
            {
                Info.Add("FirmwareType", Enum.GetName(typeof(Win32Enumerations.FirmwareType), FirmwareType));
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recupera il tipo di firmware del computer", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("FirmwareType", Properties.Resources.UnavailableText);
            }
            return Info;
        }
        #endregion
        #region Operating System Info
        /// <summary>
        /// Recupera informazioni sul sistema operativo.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetOSInfo()
        {
            Dictionary<string, object> Info = new();
            Info.Add("SystemUpTimeTicks", Win32ComputerInfoFunctions.GetTickCount());
            #region System Directory Paths
            uint PathLength = Win32ComputerInfoFunctions.GetSystemDirectory(null, 0);
            StringBuilder PathBuilder;
            if (PathLength != 0)
            {
                PathBuilder = new((int)PathLength);
                _ = Win32ComputerInfoFunctions.GetSystemDirectory(PathBuilder, PathLength);
                Info.Add("SystemDirectoryPath", PathBuilder.ToString());
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la directory di sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("SystemDirectoryPath", Properties.Resources.UnavailableText);
            }
            PathLength = Win32ComputerInfoFunctions.GetSystemWindowsDirectory(null, 0);
            if (PathLength != 0)
            {
                PathBuilder = new StringBuilder((int)PathLength);
                _ = Win32ComputerInfoFunctions.GetSystemWindowsDirectory(PathBuilder, PathLength);
                Info.Add("SharedSystemDirectoryPath", PathBuilder.ToString());
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la directory condivisa di sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("SharedSystemDirectoryPath", Properties.Resources.UnavailableText);
            }
            PathLength = Win32ComputerInfoFunctions.GetSystemWow64Directory(null, 0);
            if (PathLength != 0)
            {
                PathBuilder = new((int)PathLength);
                _ = Win32ComputerInfoFunctions.GetSystemWow64Directory(PathBuilder, PathLength);
                Info.Add("WOW64SystemDirectoryPath", PathBuilder.ToString());
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la directory di sistema usata da WOW64", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("WOW64SystemDirectoryPath", Properties.Resources.UnavailableText);
            }
            #endregion
            if (Win32ComputerInfoFunctions.GetSystemRegistryQuota(out uint QuotaAllowed, out uint QuotaUsed))
            {
                Info.Add("SystemRegistryMaximumSize", QuotaAllowed);
                Info.Add("SystemRegistryCurrentSize", QuotaUsed);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare la dimensione massima e quella corrente del registro di sistema", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("SystemRegistryMaximumSize", Properties.Resources.UnavailableText);
                Info.Add("SystemRegistryCurrentSize", Properties.Resources.UnavailableText);
            }
            Win32ComputerInfoFunctions.RtlGetNtVersionNumbers(out uint MajorVersion, out uint MinorVersion, out uint BuildNumber);
            Info.Add("SystemMajorVersionNumber", MajorVersion);
            Info.Add("SystemMinorVersionNumber", MinorVersion);
            Info.Add("SystemBuildNumber", ExtractWindowsBuildNumber(BuildNumber));
            if (!Win32ComputerInfoFunctions.GetProductInfo(MajorVersion, MinorVersion, 0, 0, out Win32Enumerations.ProductName ReturnedProductType))
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il nome del prodotto che identifica il sistema operativo", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("ProductName", Properties.Resources.UnavailableText);
            }
            else
            {
                Info.Add("ProductName", UtilityMethods.GetEnumDescription(ReturnedProductType));
            }
            if (Win32ComputerInfoFunctions.IsNativeVhdBoot(out bool NativeVhdBoot))
            {
                Info.Add("BootFromVHD", NativeVhdBoot);
            }
            else
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile identificare se il sistema è stato avviato un VHD", EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Info.Add("BootFromVHD", Properties.Resources.UnavailableText);
            }
            return Info;
        }

        /// <summary>
        /// Estrae il numero di build del sistema operativo dal valore fornito dalla Windows API.
        /// </summary>
        /// <param name="FullNumber">Valore fornito.</param>
        /// <returns>Il numero di build del sistema operativo.</returns>
        private static uint ExtractWindowsBuildNumber(uint FullNumber)
        {
            byte[] FullNumberBytes = BitConverter.GetBytes(FullNumber);
            BitArray Bits = new(FullNumberBytes);
            byte[] BuildNumberBytes = new byte[2];
            BitArray BuildNumberBits = new(16);
            for (int i = 0; i < 16; i++)
            {
                BuildNumberBits.Set(i, Bits[i]);
            }
            BuildNumberBits.CopyTo(BuildNumberBytes, 0);
            return BitConverter.ToUInt16(BuildNumberBytes, 0);
        }
        #region System Parameters Info
        #region Accessibility
        /// <summary>
        /// Recupera informazioni sui parametri di sistema relativi all'accessibilità.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetSystemAccessibilityParameters()
        {
            Dictionary<string, object> Parameters = new();
            #region Accessibility Timeout
            Win32Structures.ACCESSTIMEOUT? TimeoutInfo = GetAccessibilityTimeoutParameters();
            if (TimeoutInfo.HasValue)
            {
                bool TimeoutEnabled = TimeoutInfo.Value.Flags.HasFlag(Win32Enumerations.AccessibilityTimeoutSettings.ATF_TIMEOUTON);
                bool FeedbackEnabled = TimeoutInfo.Value.Flags.HasFlag(Win32Enumerations.AccessibilityTimeoutSettings.ATF_ONOFFFEEDBACK);
                AccessibilityTimeout TimeoutData = new(TimeoutEnabled, FeedbackEnabled, TimeoutInfo.Value.TimeoutMsec);
                Parameters.Add("TimeoutInfo", TimeoutData);
            }
            else
            {
                Parameters.Add("TimeoutInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Audio Description
            Win32Structures.AUDIODESCRIPTION? AudioDescriptionInfo = GetAccessibilityAudioDescriptionParameters();
            if (AudioDescriptionInfo.HasValue)
            {
                AudioDescriptionInfo AudioDescriptionFeatureInfo = new(AudioDescriptionInfo.Value.Enabled, GetLocaleName(AudioDescriptionInfo.Value.Locale));
                Parameters.Add("AudioDescriptionInfo", AudioDescriptionFeatureInfo);
            }
            else
            {
                Parameters.Add("AudioDescriptionInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Client Area Animation / Overlapped Content
            bool? ClientAreaAnimationEnabled = IsClientAreaAnimationEnabled();
            if (ClientAreaAnimationEnabled.HasValue)
            {
                Parameters.Add("ClientAreaAnimationEnabled", ClientAreaAnimationEnabled.Value);
            }
            else
            {
                Parameters.Add("ClientAreaAnimationEnabled", Properties.Resources.UnavailableText);
            }
            bool? OverlappedContentEnabled = IsOverlappedContentEnabled();
            if (OverlappedContentEnabled.HasValue)
            {
                Parameters.Add("OverlappedContentEnabled", OverlappedContentEnabled.Value);
            }
            else
            {
                Parameters.Add("OverlappedContentEnabled", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Filter Keys
            Win32Structures.FILTERKEYS? FilterKeysInfo = GetFilterKeysFeatureInfo();
            if (FilterKeysInfo.HasValue)
            {
                bool FeatureAvailable = FilterKeysInfo.Value.Flags.HasFlag(Win32Enumerations.FilterKeysSettings.FKF_AVAILABLE);
                bool ClickSoundEnabled = FilterKeysInfo.Value.Flags.HasFlag(Win32Enumerations.FilterKeysSettings.FKF_CLICKON);
                bool FeatureEnabled = FilterKeysInfo.Value.Flags.HasFlag(Win32Enumerations.FilterKeysSettings.FKF_FILTERKEYSON);
                bool HotkeyActive = FilterKeysInfo.Value.Flags.HasFlag(Win32Enumerations.FilterKeysSettings.FKF_HOTKEYACTIVE);
                bool HotkeySoundEnabled = FilterKeysInfo.Value.Flags.HasFlag(Win32Enumerations.FilterKeysSettings.FKF_HOTKEYSOUND);
                FilterKeysFeatureInfo FilterKeysFeatureInfo = new(FeatureAvailable, ClickSoundEnabled, FeatureEnabled, HotkeyActive, HotkeySoundEnabled, FilterKeysInfo.Value.WaitMsec, FilterKeysInfo.Value.DelayMsec, FilterKeysInfo.Value.RepeatMsec, FilterKeysInfo.Value.BounceMsec);
                Parameters.Add("FilterKeysInfo", FilterKeysFeatureInfo);
            }
            else
            {
                Parameters.Add("FilterKeysFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Focus Border
            uint?[] FocusBorderData = GetFocusBorderHeightAndWidth();
            int RectangleHeight;
            int RectangleWidth;
            if (FocusBorderData[0] is not null)
            {
                RectangleHeight = (int)FocusBorderData[0].Value;
            }
            else
            {
                RectangleHeight = -1;
            }
            if (FocusBorderData[1] is not null)
            {
                RectangleWidth = (int)FocusBorderData[1].Value;
            }
            else
            {
                RectangleWidth = -1;
            }
            FocusBorderData FocusBorderInfo = new(RectangleHeight, RectangleWidth);
            Parameters.Add("FocusBorderData", FocusBorderInfo);
            #endregion
            #region High Contrast
            Win32Structures.HIGHCONTRAST? HighContrastFeatureInfo = GetHighContrastFeatureInfo();
            if (HighContrastFeatureInfo.HasValue)
            {
                bool HighContrastEnabled = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_HIGHCONTRASTON);
                bool HighContrastAvailable = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_AVAILABLE);
                bool HotkeyActive = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_HOTKEYACTIVE);
                bool ConfirmationDialogEnabled = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_CONFIRMHOTKEY);
                bool SoundEnabled = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_HOTKEYSOUND);
                bool HotkeyAvailable = HighContrastFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.HighContrastSettings.HCF_HOTKEYAVAILABLE);
                HighContrastFeatureInfo HighContrastFeatureData = new(HighContrastEnabled, HighContrastAvailable, HotkeyActive, ConfirmationDialogEnabled, SoundEnabled, HotkeyAvailable, HighContrastFeatureInfo.Value.DefaultScheme);
                Parameters.Add("HighContrastInfo", HighContrastFeatureData);
            }
            else
            {
                Parameters.Add("HighContrastInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Popup Message Duration / Mouse Click Lock
            uint? PopupMessageDuration = GetPopupMessageDuration();
            if (PopupMessageDuration.HasValue)
            {
                Parameters.Add("PopupMessageDuration", PopupMessageDuration.Value);
            }
            else
            {
                Parameters.Add("PopupMessageDuration", Properties.Resources.UnavailableText);
            }
            MouseClickLockFeatureInfo? MouseClickLockFeatureInfo = GetMouseClickLockInfo();
            if (MouseClickLockFeatureInfo.HasValue)
            {
                Parameters.Add("MouseClickLockFeatureInfo", MouseClickLockFeatureInfo);
            }
            else
            {
                Parameters.Add("MouseClickLockFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Mouse Keys
            Win32Structures.MOUSEKEYS? MouseKeysInfo = GetMouseKeysFeatureInfo();
            if (MouseKeysInfo.HasValue)
            {
                bool FeatureAvailable = MouseKeysInfo.Value.Flags.HasFlag(Win32Enumerations.MouseKeysSettings.MKF_AVAILABLE);
                bool HotkeyActive = MouseKeysInfo.Value.Flags.HasFlag(Win32Enumerations.MouseKeysSettings.MKF_HOTKEYACTIVE);
                bool HotkeySound = MouseKeysInfo.Value.Flags.HasFlag(Win32Enumerations.MouseKeysSettings.MKF_HOTKEYSOUND);
                bool FeatureEnabled = MouseKeysInfo.Value.Flags.HasFlag(Win32Enumerations.MouseKeysSettings.MKF_MOUSEKEYSON);
                MouseKeysInfo MouseKeysFeatureInfo = new(FeatureAvailable, HotkeyActive, HotkeySound, FeatureEnabled, MouseKeysInfo.Value.MaxSpeed, MouseKeysInfo.Value.TimeToMaxSpeed, MouseKeysInfo.Value.CtrlSpeed);
                Parameters.Add("MouseKeysFeatureInfo", MouseKeysFeatureInfo);
            }
            else
            {
                Parameters.Add("MouseKeysFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Mouse Sonar / Mouse Vanish
            bool?[] MouseSonarVanishFeatureStatus = GetOtherMouseFeaturesStatus();
            if (MouseSonarVanishFeatureStatus is not null)
            {
                Parameters.Add("MouseSonarEnabled", MouseSonarVanishFeatureStatus[0]);
                Parameters.Add("MouseVanishEnabled", MouseSonarVanishFeatureStatus[1]);
            }
            else
            {
                Parameters.Add("MouseSonarEnabled", Properties.Resources.UnavailableText);
                Parameters.Add("MouseVanishEnabled", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Screen Reader / Show Sound
            bool? ScreenReaderUtilityRunning = IsScreenReaderUtilityRunning();
            if (ScreenReaderUtilityRunning.HasValue)
            {
                Parameters.Add("ScreenReaderUtilityRunning", ScreenReaderUtilityRunning.Value);
            }
            else
            {
                Parameters.Add("ScreenReaderUtilityRunning", Properties.Resources.UnavailableText);
            }
            bool? ShowSoundAccessibilityFeatureEnabled = IsShowSoundAccessibilityFeatureEnabled();
            if (ShowSoundAccessibilityFeatureEnabled.HasValue)
            {
                Parameters.Add("ShowSoundFeatureEnabled", ShowSoundAccessibilityFeatureEnabled.Value);
            }
            else
            {
                Parameters.Add("ShowSoundFeatureEnabled", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Sound Sentry
            Win32Structures.SOUNDSENTRY? SoundSentryFeatureInfo = GetSoundSentryFeatureInfo();
            if (SoundSentryFeatureInfo.HasValue)
            {
                bool FeatureEnabled = SoundSentryFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.SoundSentrySettings.SSF_SOUNDSENTRYON);
                bool FeatureAvailable = SoundSentryFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.SoundSentrySettings.SSF_AVAILABLE);
                SoundSentryWindowsEffect WindowsEffect = (SoundSentryWindowsEffect)SoundSentryFeatureInfo.Value.WindowsEffect;
                SoundSentryInfo SoundSentryInfo = new(FeatureEnabled, FeatureAvailable, WindowsEffect);
                Parameters.Add("SoundSentryFeatureInfo", SoundSentryInfo);
            }
            else
            {
                Parameters.Add("SoundSentryFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Sticky Keys
            Win32Structures.STICKYKEYS? StickyKeysFeatureInfo = GetStickyKeysFeatureInfo();
            if (StickyKeysFeatureInfo.HasValue)
            {
                bool FeatureEnabled = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_STICKYKEYSON);
                bool FeatureAvailable = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_AVAILABLE);
                bool HotkeySound = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_HOTKEYSOUND);
                bool HotkeyActive = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_HOTKEYACTIVE);
                bool Tristate = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_TRISTATE);
                bool TwoKeysOff = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_TWOKEYSOFF);
                bool AudibleFeedback = StickyKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.StickyKeysSettings.SKF_AUDIBLEFEEDBACK);
                StickyKeysFeatureInfo StickyKeysInfo = new(FeatureAvailable, FeatureEnabled, AudibleFeedback, HotkeyActive, HotkeySound, Tristate, TwoKeysOff);
                Parameters.Add("StickyKeysFeatureInfo", StickyKeysInfo);
            }
            else
            {
                Parameters.Add("StickyKeysFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            #region Toggle Keys
            Win32Structures.TOGGLEKEYS? ToggleKeysFeatureInfo = GetToggleKeysFeatureInfo();
            if (ToggleKeysFeatureInfo.HasValue)
            {
                bool FeatureAvailable = ToggleKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.ToggleKeysSettings.TKF_AVAILABLE);
                bool FeatureEnabled = ToggleKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.ToggleKeysSettings.TKF_TOGGLEKEYSON);
                bool HotkeyActive = ToggleKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.ToggleKeysSettings.TKF_HOTKEYACTIVE);
                bool HotkeySound = ToggleKeysFeatureInfo.Value.Flags.HasFlag(Win32Enumerations.ToggleKeysSettings.TKF_HOTKEYSOUND);
                ToggleKeysFeatureInfo ToggleKeysInfo = new(FeatureAvailable, FeatureEnabled, HotkeyActive, HotkeySound);
                Parameters.Add("ToggleKeysFeatureInfo", ToggleKeysInfo);
            }
            else
            {
                Parameters.Add("ToggleKeysFeatureInfo", Properties.Resources.UnavailableText);
            }
            #endregion
            return Parameters;
        }
        #endregion
        #region Desktop
        /// <summary>
        /// Recupera i parametri di sistema relativi al desktop.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetSystemDesktopParameters()
        {
            Dictionary<string, object> Parameters = new();
            bool? ClearTypeEnabled = IsClearTypeEnabled();
            if (ClearTypeEnabled.HasValue)
            {
                Parameters.Add("ClearTypeEnabled", ClearTypeEnabled.Value);
            }
            else
            {
                Parameters.Add("ClearTypeEnabled", Properties.Resources.UnavailableText);
            }
            string DesktopWallpaperPath = GetDesktopWallpaperPath();
            if (DesktopWallpaperPath is not null)
            {
                Parameters.Add("DesktopWallpaperPath", DesktopWallpaperPath);
            }
            else
            {
                Parameters.Add("DesktopWallpaperPath", Properties.Resources.UnavailableText);
            }
            bool? DropShadowEnabled = IsDropShadowEnabled();
            if (DropShadowEnabled.HasValue)
            {
                Parameters.Add("DropShadowEnabled", DropShadowEnabled.Value);
            }
            else
            {
                Parameters.Add("DropShadowEnabled", Properties.Resources.UnavailableText);
            }
            bool? FlatMenuAppearanceEnabled = IsFlatMenuAppearanceEnabled();
            if (FlatMenuAppearanceEnabled.HasValue)
            {
                Parameters.Add("FlatMenuEnabled", FlatMenuAppearanceEnabled.Value);
            }
            else
            {
                Parameters.Add("FlatMenuEnabled", Properties.Resources.UnavailableText);
            }
            (bool? IsEnabled, uint? Contrast, Win32Enumerations.FontSmoothingOrientation? Orientation, Win32Enumerations.FontSmoothingType? Type) = GetFontSmoothingSettings();
            FontSmoothingSettings SmoothingSettings = new(IsEnabled, Contrast, Orientation, Type);
            Parameters.Add("FontSmoothingSettings", SmoothingSettings);
            Win32Structures.RECT? WorkArea = GetWorkAreaSize();
            if (WorkArea.HasValue)
            {
                Parameters.Add("WorkAreaSize", WorkArea);
            }
            else
            {
                Parameters.Add("WorkAreaSize", Properties.Resources.UnavailableText);
            }
            return Parameters;
        }
        #endregion
        #region Icon
        /// <summary>
        /// Recupera i parametri di sistema relativi alle icone.
        /// </summary>
        /// <returns>Una struttura <see cref="IconMetrics"/> con le informazioni.</returns>
        public static IconMetrics? GetSystemIconParameters()
        {
            Win32Structures.ICONMETRICS? IconMetrics = GetIconMetrics();
            if (IconMetrics.HasValue)
            {
                byte[] FontPitchAndFamily = GetFontPitchAndFamily(IconMetrics.Value.Font.PitchAndFamily);
                FontData FontData = new(IconMetrics.Value.Font.Height, IconMetrics.Value.Font.Width, IconMetrics.Value.Font.Escapement, IconMetrics.Value.Font.Orientation, IconMetrics.Value.Font.Weight, IconMetrics.Value.Font.Italic, IconMetrics.Value.Font.Underline, IconMetrics.Value.Font.StrikeOut, IconMetrics.Value.Font.Charset, IconMetrics.Value.Font.OutputPrecision, IconMetrics.Value.Font.ClippingPrecision, IconMetrics.Value.Font.Quality, (Win32Enumerations.FontPitch)FontPitchAndFamily[0], (Win32Enumerations.FontFamily)FontPitchAndFamily[1], IconMetrics.Value.Font.FaceName);
                IconMetrics IconMetricsData = new(IconMetrics.Value.HorizontalSpacing, IconMetrics.Value.VerticalSpacing, IconMetrics.Value.TitleWrap != 0, FontData);
                return IconMetricsData;
            } 
            else
            {
                return null;
            }
        }
        #endregion
        #region Input
        /// <summary>
        /// Recupera i parametri di sistema relativi all'input.
        /// </summary>
        /// <returns>Un dizionario con le informazioni.</returns>
        public static Dictionary<string, object> GetSystemInputParameters()
        {
            Dictionary<string, object> Parameters = new();

            return Parameters;
        }
        #endregion
        #endregion
        /// <summary>
        /// Converte l'identificatore di una località nel suo nome.
        /// </summary>
        /// <param name="LocaleID">Identificatore località.</param>
        /// <returns>Il nome della località.</returns>
        private static string GetLocaleName(uint LocaleID)
        {
            StringBuilder Name = new((int)Win32Constants.LOCALE_NAME_MAX_LENGTH);
            if (Win32OtherFunctions.LCIDToLocaleName(LocaleID, Name, (int)Win32Constants.LOCALE_NAME_MAX_LENGTH, Win32Constants.LOCALE_ALLOW_NEUTRAL_NAMES) != 0)
            {
                return Name.ToString();
            }
            else
            {
                return Properties.Resources.UnavailableText;
            }
        }

        /// <summary>
        /// Recupera i valori dell'inclinazione e della famiglia del font usato per le icone.
        /// </summary>
        /// <param name="PitchAndFamily">Valore che contiene l'informazione.</param>
        /// <returns>Un'array di byte con le informazioni, il primo valore è l'inclinazione, il secondo è la famiglia.</returns>
        private static byte[] GetFontPitchAndFamily(byte PitchAndFamily)
        {
            byte FontPitch;
            byte FontFamily;
            byte[] PitchAndFamilyBytes = BitConverter.GetBytes(PitchAndFamily);
            BitArray PitchAndFamilyBits = new(PitchAndFamilyBytes);
            BitArray PitchBits = new(2);
            BitArray FamilyBits = new(4);
            for (int i = 0; i < PitchAndFamilyBits.Length; i++)
            {
                if (i <= 1)
                {
                    PitchBits[i] = PitchAndFamilyBits[i];
                }
                else if (i >= 4)
                {
                    FamilyBits[i - 4] = PitchAndFamilyBits[i];
                }
            }
            byte[] PitchByte = new byte[1];
            byte[] FamilyByte = new byte[1];
            PitchBits.CopyTo(PitchByte, 0);
            FamilyBits.CopyTo(FamilyByte, 0);
            FontPitch = PitchByte[0];
            FontFamily = FamilyByte[0];
            byte[] Values = { FontPitch, FontFamily };
            return Values;
        }

        #region System Parameters Info Getter Methods
        #region Accessibility
        /// <summary>
        /// Recupera le informazioni sul timeout delle funzionabilità di accessibilità.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.ACCESSTIMEOUT"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.ACCESSTIMEOUT? GetAccessibilityTimeoutParameters()
        {
            Win32Structures.ACCESSTIMEOUT Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.ACCESSTIMEOUT))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETACCESSTIMEOUT, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETACCESSTIMEOUT);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.ACCESSTIMEOUT)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.ACCESSTIMEOUT));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera le informazioni sulle descrizioni audio.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.AUDIODESCRIPTION"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.AUDIODESCRIPTION? GetAccessibilityAudioDescriptionParameters()
        {
            Win32Structures.AUDIODESCRIPTION Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.AUDIODESCRIPTION))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETAUDIODESCRIPTION, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETAUDIODESCRIPTION);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.AUDIODESCRIPTION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.AUDIODESCRIPTION));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Determina se le animazioni dell'area client sono attivate.
        /// </summary>
        /// <returns>true se le animazioni sono attive, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsClientAreaAnimationEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETCLIENTAREAANIMATION, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETCLIENTAREAANIMATION);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                bool Info = Convert.ToBoolean(Marshal.ReadInt32(Buffer));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Determina se il contenuto sovrapposto è attivo.
        /// </summary>
        /// <returns>true se il contenuto sovrapposto è attivo, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsOverlappedContentEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETDISABLEOVERLAPPEDCONTENT, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETDISABLEOVERLAPPEDCONTENT);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                bool Info = Convert.ToBoolean(Marshal.ReadInt32(Buffer));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità di accessibilità Filtro Tasti.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.FILTERKEYS"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.FILTERKEYS? GetFilterKeysFeatureInfo()
        {
            Win32Structures.FILTERKEYS Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.FILTERKEYS))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETFILTERKEYS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETFILTERKEYS);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.FILTERKEYS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.FILTERKEYS));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera le dimensioni del rettangolo di focus.
        /// </summary>
        /// <returns>Un array con l'altezza e la larghezza del rettangolo, nullo in caso di errore.</returns>
        private static uint?[] GetFocusBorderHeightAndWidth()
        {
            uint?[] Data = new uint?[2];
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETFOCUSBORDERHEIGHT, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETFOCUSBORDERHEIGHT);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Data[0] = null;
            }
            else
            {
                Data[0] = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETFOCUSBORDERWIDTH, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETFOCUSBORDERWIDTH);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Data[1] = null;
            }
            else
            {
                Data[1] = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            return Data;
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità Alto Contrasto.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.HIGHCONTRAST"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.HIGHCONTRAST? GetHighContrastFeatureInfo()
        {
            Win32Structures.HIGHCONTRAST Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.HIGHCONTRAST))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETHIGHCONTRAST, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETHIGHCONTRAST);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.HIGHCONTRAST)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.HIGHCONTRAST));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera la durata dei messaggi popup.
        /// </summary>
        /// <returns>Durata dei messaggi popup, in secondi, nullo in caso di errore.</returns>
        private static uint? GetPopupMessageDuration()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMESSAGEDURATION, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMESSAGEDURATION);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità di blocco del tasto del mouse.
        /// </summary>
        /// <returns>Una struttura <see cref="MouseClickLockFeatureInfo"/> con le informazioni, nullo in caso di errore.</returns>
        private static MouseClickLockFeatureInfo? GetMouseClickLockInfo()
        {
            bool? FeatureEnabled;
            uint? DelayMSec;
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSECLICKLOCK, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSECLICKLOCK);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                FeatureEnabled = null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                FeatureEnabled = Convert.ToBoolean(Info);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSECLICKLOCKTIME, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSECLICKLOCKTIME);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                DelayMSec = null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                DelayMSec = Info;
            }
            if (!FeatureEnabled.HasValue && !DelayMSec.HasValue)
            {
                return null;
            }
            else
            {
                MouseClickLockFeatureInfo FeatureInfo = new(FeatureEnabled, DelayMSec);
                return FeatureInfo;
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità MouseKeys.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.MOUSEKEYS"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.MOUSEKEYS? GetMouseKeysFeatureInfo()
        {
            Win32Structures.MOUSEKEYS Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.MOUSEKEYS))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSEKEYS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSEKEYS);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.MOUSEKEYS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.MOUSEKEYS));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera informazioni sulle funzionalità MouseSonar e MouseVanish.
        /// </summary>
        /// <returns>Un array con lo stato delle funzionalità, nullo in caso di errore.</returns>
        private static bool?[] GetOtherMouseFeaturesStatus()
        {
            bool? MouseSonarEnabled;
            bool? MouseVanishEnabled;
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSESONAR, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSESONAR);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                MouseSonarEnabled = null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                MouseSonarEnabled = Convert.ToBoolean(Info);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSEVANISH, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETMOUSEVANISH);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                MouseVanishEnabled = null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                MouseVanishEnabled = Convert.ToBoolean(Info);
            }
            if (!MouseSonarEnabled.HasValue && !MouseVanishEnabled.HasValue)
            {
                return null;
            }
            else
            {
                bool?[] MouseFeaturesStatus = new bool?[2];
                MouseFeaturesStatus[0] = MouseSonarEnabled;
                MouseFeaturesStatus[1] = MouseVanishEnabled;
                return MouseFeaturesStatus;
            }
        }

        /// <summary>
        /// Determina se un'applicazione di lettura schermo è in esecuzione.
        /// </summary>
        /// <returns>true se un'applicaziona di lettura schermo è in esecuzione, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsScreenReaderUtilityRunning()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETSCREENREADER, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETSCREENREADER);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Convert.ToBoolean(Info);
            }
        }

        /// <summary>
        /// Determina se la funzionalità Show Sounds è abilitata.
        /// </summary>
        /// <returns>true se la funzionalità è abilitata, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsShowSoundAccessibilityFeatureEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETSHOWSOUNDS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETSHOWSOUNDS);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Convert.ToBoolean(Info);
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità SoundSentry.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.SOUNDSENTRY"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.SOUNDSENTRY? GetSoundSentryFeatureInfo()
        {
            Win32Structures.SOUNDSENTRY Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.SOUNDSENTRY))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETSOUNDSENTRY, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETSOUNDSENTRY);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.SOUNDSENTRY)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.SOUNDSENTRY));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità Tasti Permanenti.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.STICKYKEYS"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.STICKYKEYS? GetStickyKeysFeatureInfo()
        {
            Win32Structures.STICKYKEYS Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.STICKYKEYS))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETSTICKYKEYS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETSTICKYKEYS);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.STICKYKEYS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.STICKYKEYS));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }

        /// <summary>
        /// Recupera informazioni sulla funzionalità ToggleKeys.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.TOGGLEKEYS"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.TOGGLEKEYS? GetToggleKeysFeatureInfo()
        {
            Win32Structures.TOGGLEKEYS Info = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.TOGGLEKEYS))
            };
            IntPtr Buffer = Marshal.AllocHGlobal((int)Info.Size);
            Marshal.StructureToPtr(Info, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemAccessibilityParameters.SPI_GETTOGGLEKEYS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemAccessibilityParameters), Win32Enumerations.SystemAccessibilityParameters.SPI_GETTOGGLEKEYS);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di uno dei parametri di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                Info = (Win32Structures.TOGGLEKEYS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.TOGGLEKEYS));
                Marshal.FreeHGlobal(Buffer);
                return Info;
            }
        }
        #endregion
        #region Desktop
        /// <summary>
        /// Determina se ClearType è abilitato.
        /// </summary>
        /// <returns>true se ClearType è abilitato, false altrimenti.</returns>
        private static bool? IsClearTypeEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETCLEARTYPE, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETCLEARTYPE);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Convert.ToBoolean(Info);
            }
        }

        /// <summary>
        /// Recupera il percorso dell'immagine bitmap usata come sfondo del desktop.
        /// </summary>
        /// <returns>Il percorso dello sfondo del desktop, nullo in caso di errore.</returns>
        private static string GetDesktopWallpaperPath()
        {
            IntPtr Buffer = Marshal.AllocHGlobal((int)Win32Constants.MAX_PATH);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETDESKWALLPAPER, Win32Constants.MAX_PATH, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETDESKWALLPAPER);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                string Path = Marshal.PtrToStringUni(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Path;
            }
        }

        /// <summary>
        /// Determina se le ombre discendenti sono attive.
        /// </summary>
        /// <returns>true se le ombre discendenti sono attive, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsDropShadowEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETDROPSHADOW, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETDROPSHADOW);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Convert.ToBoolean(Info);
            }
        }

        /// <summary>
        /// Indica se i menù utente hanno l'aspetto di base.
        /// </summary>
        /// <returns>true se i menù hanno l'aspetto di base abilitato, false altrimenti, nullo in caso di errore.</returns>
        private static bool? IsFlatMenuAppearanceEnabled()
        {
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETFLATMENU, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETFLATMENU);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                return Convert.ToBoolean(Info);
            }
        }

        /// <summary>
        /// Recupera informazioni sull'antialias dei font.
        /// </summary>
        /// <returns>Una tupla con informazioni sull'antialias dei font, i componenti possono essere nulli se si è verificato un errore durante il recupero.</returns>
        private static (bool? IsEnabled, uint? Contrast, Win32Enumerations.FontSmoothingOrientation? Orientation, Win32Enumerations.FontSmoothingType? Type) GetFontSmoothingSettings()
        {
            bool? Enabled;
            uint? Contrast;
            Win32Enumerations.FontSmoothingOrientation? Orientation;
            Win32Enumerations.FontSmoothingType? Type;
            #region Parameters Get Operations
            IntPtr Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHING, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHING);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Enabled = null;
            }
            else
            {
                uint Info = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
                Enabled = Convert.ToBoolean(Info);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGCONTRAST, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGCONTRAST);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Contrast = null;
            }
            else
            {
                Contrast = (uint)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGORIENTATION, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGORIENTATION);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Orientation = null;
            }
            else
            {
                Orientation = (Win32Enumerations.FontSmoothingOrientation)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            Buffer = Marshal.AllocHGlobal(4);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGTYPE, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETFONTSMOOTHINGTYPE);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                Type = null;
            }
            else
            {
                Type = (Win32Enumerations.FontSmoothingType)Marshal.ReadInt32(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            #endregion
            return (Enabled, Contrast, Orientation, Type);
        }

        /// <summary>
        /// Recupera la dimensione dell'area di lavoro del monitor primario.
        /// </summary>
        /// <returns>Una struttura <see cref="Win32Structures.RECT"/> con le informazioni, nullo in caso di errore.</returns>
        private static Win32Structures.RECT? GetWorkAreaSize()
        {
            Win32Structures.RECT WorkArea;
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.RECT)));
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemDesktopParameters.SPI_GETWORKAREA, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemDesktopParameters.SPI_GETWORKAREA);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                WorkArea = (Win32Structures.RECT)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.RECT));
                Marshal.FreeHGlobal(Buffer);
                return WorkArea;
            }
        }
        #endregion
        #region Icon
        /// <summary>
        /// Recupera le metriche delle icone.
        /// </summary>
        private static Win32Structures.ICONMETRICS? GetIconMetrics()
        {
            Win32Structures.ICONMETRICS IconMetrics = new()
            {
                Size = (uint)Marshal.SizeOf(typeof(Win32Structures.ICONMETRICS))
            };
            IntPtr Buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32Structures.ICONMETRICS)));
            Marshal.StructureToPtr(IconMetrics, Buffer, false);
            if (!Win32ComputerInfoFunctions.SystemParametersInfo((uint)Win32Enumerations.SystemIconParameters.SPI_GETICONMETRICS, 0, Buffer, 0))
            {
                Marshal.FreeHGlobal(Buffer);
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                string ParameterName = Enum.GetName(typeof(Win32Enumerations.SystemDesktopParameters), Win32Enumerations.SystemIconParameters.SPI_GETICONMETRICS);
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare il valore di un parametro di sistema, parametro: " + ParameterName, EventAction.ComputerInfoRead, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return null;
            }
            else
            {
                IconMetrics = (Win32Structures.ICONMETRICS)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.ICONMETRICS));
                Marshal.DestroyStructure(Buffer, typeof(Win32Structures.LOGFONT));
                Marshal.FreeHGlobal(Buffer);
                return IconMetrics;
            }
        }
        #endregion
        #region Input

        #endregion
        #endregion
        #endregion
        #endregion
        #endregion
        #region Process Limiter Management Methods
        /// <summary>
        /// Crea un job che verrà utilizzato dal limitatore CPU.
        /// </summary>
        /// <param name="UsageLimit">Limite di utilizzo in percentuale.</param>
        /// <returns>Handle nativo al job, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr CreateProcessLimiterJobObject(byte UsageLimit)
        {
            IntPtr JobHandle = Win32OtherFunctions.CreateJobObject(IntPtr.Zero, null);
            if (JobHandle == IntPtr.Zero)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un job per il limitatore CPU", EventAction.ProcessLimiterInitialization, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
                return IntPtr.Zero;
            }
            else
            {
                Win32Structures.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION RateControl = new()
                {
                    ControlFlags = Win32Enumerations.JobObjectCPURateControl.CpuRateControlEnabled | Win32Enumerations.JobObjectCPURateControl.CpuRateControlHardCap,
                    CpuRate = (uint)UsageLimit * 100
                };
                uint StructureSize = (uint)Marshal.SizeOf(RateControl);
                IntPtr RateControlStructureBuffer = Marshal.AllocHGlobal((int)StructureSize);
                Marshal.StructureToPtr(RateControl, RateControlStructureBuffer, false);
                if (!Win32OtherFunctions.SetInformationJobObject(JobHandle, Win32Enumerations.JobInformationClass.JobObjectCpuRateControlInformation, RateControlStructureBuffer, StructureSize))
                {
                    Marshal.FreeHGlobal(RateControlStructureBuffer);
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un job per il limitatore CPU", EventAction.ProcessLimiterInitialization, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return IntPtr.Zero;
                }
                Marshal.FreeHGlobal(RateControlStructureBuffer);
                return JobHandle;
            }
        }

        /// <summary>
        /// Aggiunge un processo a un job associato al limitatore CPU.
        /// </summary>
        /// <param name="JobHandle">Handle nativo al job a cui aggiungere il processo.</param>
        /// <param name="ProcessHandle">Handle al processo da aggiugere.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool AddProcessToJob(IntPtr JobHandle, SafeProcessHandle ProcessHandle)
        {
            if (Settings.SafeMode)
            {
                if (!Win32ProcessFunctions.IsProcessCritical(ProcessHandle.DangerousGetHandle(), out bool IsCritical))
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile determinare se un processo è un processo di sistema", EventAction.ProcessLimiterPathAdd, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    if (IsCritical)
                    {
                        return false;
                    }
                }
            }
            IntPtr CurrentProcessHandle = Win32OtherFunctions.GetCurrentProcess();
            if (!Win32OtherFunctions.CompareObjectHandles(CurrentProcessHandle, ProcessHandle.DangerousGetHandle()))
            {
                if (!Win32OtherFunctions.AssignProcessToJobObject(JobHandle, ProcessHandle.DangerousGetHandle()))
                {
                    Win32Exception ex = new(Marshal.GetLastWin32Error());
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile assegnare un processo a un job associato al limitatore CPU", EventAction.ProcessLimiterPathAdd, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return false;
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForInformation("Processo aggiunto a un job del limitatore processi", EventAction.ProcessLimiterJobAdd, ProcessHandle);
                    Logger.WriteEntry(Entry);
                    return true;
                }
            }
            else
            {
                LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile assegnare un processo a un job associato al limitatore CPU, azioni sul processo corrente non sono permesse", EventAction.ProcessLimiterPathAdd);
                Logger.WriteEntry(Entry);
                return false;
            }

        }

        /// <summary>
        /// Recupera informazioni di contabilità di un job del limitatore processi.
        /// </summary>
        /// <param name="JobHandle">Handle nativo al job.</param>
        /// <param name="LimitedProcesses">Processi limitati in esecuzione associati al job.</param>
        /// <returns>Istanza di <see cref="JobInfo"/> con le informazioni.</returns>
        public static JobInfo GetJobData(IntPtr JobHandle, List<ProcessInfo> LimitedProcesses = null)
        {
            int BufferSize = Marshal.SizeOf(typeof(Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION));
            IntPtr Buffer = Marshal.AllocHGlobal(BufferSize);
            if (Win32OtherFunctions.QueryInformationJobObject(JobHandle, Win32Enumerations.JobInformationClass.JobObjectBasicAndIoAccountingInformation, Buffer, (uint)BufferSize, out uint ReturnLength))
            {
                Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION Data = (Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION));
                Marshal.FreeHGlobal(Buffer);
                if (LimitedProcesses is not null)
                {
                    return new(new(Data.BasicInfo.TotalUserTime), new(Data.BasicInfo.TotalKernelTime), Data.BasicInfo.TotalPageFaultCount, Data.BasicInfo.TotalProcesses, Data.BasicInfo.ActiveProcesses, Data.BasicInfo.TotalTerminatedProcesses, Data.IoInfo.ReadOperationsCount, Data.IoInfo.WriteOperationCount, Data.IoInfo.OtherOperationCount, Data.IoInfo.ReadTransferCount, Data.IoInfo.WriteTransferCount, Data.IoInfo.OtherTransferCount, LimitedProcesses, true);
                }
                else
                {
                    return new(new(Data.BasicInfo.TotalUserTime), new(Data.BasicInfo.TotalKernelTime), Data.BasicInfo.TotalPageFaultCount, Data.BasicInfo.TotalProcesses, Data.BasicInfo.ActiveProcesses, Data.BasicInfo.TotalTerminatedProcesses, Data.IoInfo.ReadOperationsCount, Data.IoInfo.WriteOperationCount, Data.IoInfo.OtherOperationCount, Data.IoInfo.ReadTransferCount, Data.IoInfo.WriteTransferCount, Data.IoInfo.OtherTransferCount, null, false);
                }
            }
            else
            {
                uint ErrorCode = (uint)Marshal.GetLastWin32Error();
                if (ErrorCode is Win32Constants.ERROR_INSUFFICIENT_BUFFER)
                {
                    Buffer = Marshal.ReAllocHGlobal(Buffer, (IntPtr)ReturnLength);
                    if (Win32OtherFunctions.QueryInformationJobObject(JobHandle, Win32Enumerations.JobInformationClass.JobObjectBasicAndIoAccountingInformation, Buffer, (uint)BufferSize, out _))
                    {
                        Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION Data = (Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION)Marshal.PtrToStructure(Buffer, typeof(Win32Structures.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION));
                        Marshal.FreeHGlobal(Buffer);
                        if (LimitedProcesses is not null)
                        {
                            return new(new(Data.BasicInfo.TotalUserTime), new(Data.BasicInfo.TotalKernelTime), Data.BasicInfo.TotalPageFaultCount, Data.BasicInfo.TotalProcesses, Data.BasicInfo.ActiveProcesses, Data.BasicInfo.TotalTerminatedProcesses, Data.IoInfo.ReadOperationsCount, Data.IoInfo.WriteOperationCount, Data.IoInfo.OtherOperationCount, Data.IoInfo.ReadTransferCount, Data.IoInfo.WriteTransferCount, Data.IoInfo.OtherTransferCount, LimitedProcesses, true);
                        }
                        else
                        {
                            return new(new(Data.BasicInfo.TotalUserTime), new(Data.BasicInfo.TotalKernelTime), Data.BasicInfo.TotalPageFaultCount, Data.BasicInfo.TotalProcesses, Data.BasicInfo.ActiveProcesses, Data.BasicInfo.TotalTerminatedProcesses, Data.IoInfo.ReadOperationsCount, Data.IoInfo.WriteOperationCount, Data.IoInfo.OtherOperationCount, Data.IoInfo.ReadTransferCount, Data.IoInfo.WriteTransferCount, Data.IoInfo.OtherTransferCount, null, false);
                        }
                    }
                    else
                    {
                        Win32Exception ex = new((int)ErrorCode);
                        LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un job", EventAction.ProcessLimiterJobQuery, null, ex.NativeErrorCode, ex.Message);
                        Logger.WriteEntry(Entry);
                        return null;
                    }
                }
                else
                {
                    Win32Exception ex = new((int)ErrorCode);
                    LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile recuperare informazioni su un job", EventAction.ProcessLimiterJobQuery, null, ex.NativeErrorCode, ex.Message);
                    Logger.WriteEntry(Entry);
                    return null;
                }
            }
        }
        #endregion
        #region Process Automatic Settings Methods
        #region Memory Monitoring
        /// <summary>
        /// Crea un oggetto per il monitoraggio della memoria di sistema.
        /// </summary>
        /// <returns>Handle nativo all'oggetto, <see cref="IntPtr.Zero"/> in caso di errore.</returns>
        public static IntPtr CreateMemoryNotificationObject()
        {
            IntPtr MemoryNotificationHandle = Win32MemoryFunctions.CreateMemoryResourceNotification(Win32Enumerations.MemoryResourceNotificationType.LowMemoryResourceNotification);
            if (MemoryNotificationHandle == IntPtr.Zero)
            {
                Win32Exception ex = new(Marshal.GetLastWin32Error());
                LogEntry Entry = BuildLogEntryForWin32Error("Non è stato possibile creare un oggetto di monitoraggio dello stato della memoria di sistema", EventAction.SystemMemoryMonitoring, null, ex.NativeErrorCode, ex.Message);
                Logger.WriteEntry(Entry);
            }
            return MemoryNotificationHandle;
        }

        /// <summary>
        /// Inizia il monitoraggio dello stato della memoria.
        /// </summary>
        /// <param name="EventHandle">Handle a un evento che indica che il monitoraggio deve essere terminato.</param>
        /// <param name="MemoryResourceNotificationHandle">Handle nativo all'oggetto di notifica stato memoria.</param>
        /// <param name="VM">Istanza di <see cref="ProcessInfoVM"/> necessaria per la pulitura della memoria.</param>
        public static void StartMemoryWatch(ProcessInfoVM VM, IntPtr MemoryResourceNotificationHandle, SafeWaitHandle EventHandle)
        {
            IntPtr[] Handles = new IntPtr[2];
            Handles[0] = MemoryResourceNotificationHandle;
            Handles[1] = EventHandle.DangerousGetHandle();
            uint Result = Win32OtherFunctions.WaitForMultipleObjects(2, Handles, false, Win32Constants.INFINITE);
            if (Result == (uint)Win32Enumerations.WaitResult.WAIT_FAILED)
            {
                WatchdogManager.StopMemoryMonitoring();
            }
            else if (Result == (uint)Win32Enumerations.WaitResult.WAIT_OBJECT_0)
            {
                if (Settings.ShowNotificationForLowMemoryCondition)
                {
                    new ToastContentBuilder().AddText(Properties.Resources.LowMemoryConditionNotificationText2, hintMaxLines: 1).AddText(Properties.Resources.LowMemoryConditionNotificationText + " " + VM.MemoryUsagePercentage + "% (" + VM.MemoryUsage + ")").Show();
                }
                if (Settings.CleanSystemMemoryIfLow)
                {
                    _ = Task.Run(() => CleanSystemMemory(VM, MemoryResourceNotificationHandle, EventHandle));
                }
                else
                {
                    bool ContinueMonitoring = true;
                    if (Win32MemoryFunctions.QueryMemoryResourceNotification(MemoryResourceNotificationHandle, out bool ResourceState))
                    {
                        while (ResourceState)
                        {
                            Thread.Sleep(15000);
                            if (!Win32MemoryFunctions.QueryMemoryResourceNotification(MemoryResourceNotificationHandle, out ResourceState))
                            {
                                ContinueMonitoring = false;
                                break;
                            }
                            else
                            {
                                if (!ResourceState)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    WatchdogManager.StopMemoryMonitoring();
                    if (ContinueMonitoring)
                    {
                        WatchdogManager.InitializeMemoryMonitoring();
                    }
                }
            }
        }

        /// <summary>
        /// Pulisce la memoria di sistema per quanto possibile.
        /// </summary>
        /// <param name="MainVM">Istanza di <see cref="ProcessInfoVM"/>.</param>
        /// <param name="ResourceNotificationHandle">Handle nativo all'oggetto di notifica stato memoria.</param>
        /// <param name="EventHandle">Handle all'evento che indica il termine del monitoraggio della memoria.</param>
        public static void CleanSystemMemory(ProcessInfoVM MainVM, IntPtr ResourceNotificationHandle, SafeWaitHandle EventHandle)
        {
            LogEntry Entry = BuildLogEntryForInformation("Iniziata pulizia della memoria di sistema", EventAction.SystemMemoryCleaning, null);
            Logger.WriteEntry(Entry);
            bool ResourceState;
            if (MainVM.EmptyAllProcessesWorkingSet())
            {
                Entry = BuildLogEntryForInformation("Pulizia dei working set dei processi eseguita", EventAction.SystemMemoryCleaning, null);
                Logger.WriteEntry(Entry);
                if (Win32MemoryFunctions.QueryMemoryResourceNotification(ResourceNotificationHandle, out ResourceState))
                {
                    if (ResourceState)
                    {
                        GCHandle Value = GCHandle.Alloc(Win32Enumerations.MemoryListCommand.MemoryPurgeLowPriorityStandbyList, GCHandleType.Pinned);
                        uint Result = Win32OtherFunctions.NtSetSystemInformation(Win32Enumerations.SystemInformationClass.SystemMemoryListInformation, Value.AddrOfPinnedObject(), 4);
                        Value.Free();
                        if (Result == Win32Constants.STATUS_SUCCESS)
                        {
                            Entry = BuildLogEntryForInformation("Pulizia della lista standby a bassa priorità eseguita", EventAction.SystemMemoryCleaning, null);
                            Logger.WriteEntry(Entry);
                            if (Win32MemoryFunctions.QueryMemoryResourceNotification(ResourceNotificationHandle, out ResourceState))
                            {
                                if (ResourceState)
                                {
                                    Entry = BuildLogEntryForInformation("Pulizia della lista standby eseguita", EventAction.SystemMemoryCleaning, null);
                                    Logger.WriteEntry(Entry);
                                    Value = GCHandle.Alloc(Win32Enumerations.MemoryListCommand.MemoryPurgeStandbyList, GCHandleType.Pinned);
                                    _ = Win32OtherFunctions.NtSetSystemInformation(Win32Enumerations.SystemInformationClass.SystemMemoryListInformation, Value.AddrOfPinnedObject(), 4);
                                    Value.Free();
                                }
                            }
                            else
                            {
                                Entry = BuildLogEntryForWarning("Non è stato possibile pulire la lista standby", EventAction.SystemMemoryCleaning);
                                Logger.WriteEntry(Entry);
                            }
                        }
                        else
                        {
                            Entry = BuildLogEntryForWarning("Non è stato possibile pulire la lista standby a bassa priorità", EventAction.SystemMemoryCleaning);
                            Logger.WriteEntry(Entry);
                        }
                    }
                }
            }
            else
            {
                Entry = BuildLogEntryForWarning("Non è stato possibile liberare i working set dei processi", EventAction.SystemMemoryCleaning);
                Logger.WriteEntry(Entry);
            }
            bool ContinueMonitoring = true;
            if (Win32MemoryFunctions.QueryMemoryResourceNotification(ResourceNotificationHandle, out ResourceState))
            {
                while (ResourceState)
                {
                    Thread.Sleep(15000);
                    if (!Win32MemoryFunctions.QueryMemoryResourceNotification(ResourceNotificationHandle, out ResourceState))
                    {
                        ContinueMonitoring = false;
                        break;
                    }
                    else
                    {
                        if (!ResourceState)
                        {
                            break;
                        }
                    }
                }
            }
            WatchdogManager.StopMemoryMonitoring();
            if (ContinueMonitoring)
            {
                WatchdogManager.InitializeMemoryMonitoring();
            }
        }
        #endregion
        #region System Energy Usage
        /// <summary>
        /// Modifica l'utilizzo energetico del sistema.
        /// </summary>
        /// <param name="KeepDisplayOn">Indica se mantenere il display acceso.</param>
        /// <param name="KeepSystemInWorkingState">Indica se impedire al sistema di entrare in sospensione.</param>
        /// <param name="ShuttingDown">Indica che il programma è in chiusura.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool ChangeSystemWakeState(bool KeepDisplayOn, bool KeepSystemInWorkingState, bool ShuttingDown = false)
        {
            if (!ShuttingDown)
            {
                if (KeepDisplayOn && KeepSystemInWorkingState)
                {
                    uint FormerState = Win32OtherFunctions.SetThreadExecutionState(Win32Enumerations.ExecutionState.ES_CONTINUOUS | Win32Enumerations.ExecutionState.ES_DISPLAY_REQUIRED | Win32Enumerations.ExecutionState.ES_SYSTEM_REQUIRED);
                    if (FormerState is not 0)
                    {
                        LogEntry Entry = BuildLogEntryForInformation("Utilizzo energetico del sistema modificato", EventAction.SystemEnergyStateManipulation);
                        Logger.WriteEntry(Entry);
                        return true;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare le impostazioni energetiche del sistema", EventAction.SystemEnergyStateManipulation);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else if (!KeepDisplayOn && KeepSystemInWorkingState)
                {
                    uint FormerState = Win32OtherFunctions.SetThreadExecutionState(Win32Enumerations.ExecutionState.ES_CONTINUOUS | Win32Enumerations.ExecutionState.ES_SYSTEM_REQUIRED);
                    if (FormerState is not 0)
                    {
                        LogEntry Entry = BuildLogEntryForInformation("Utilizzo energetico del sistema modificato", EventAction.SystemEnergyStateManipulation);
                        Logger.WriteEntry(Entry);
                        return true;
                    }
                    else
                    {
                        LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare le impostazioni energetiche del sistema", EventAction.SystemEnergyStateManipulation);
                        Logger.WriteEntry(Entry);
                        return false;
                    }
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare le impostazioni energetiche del sistema, informazioni non valide", EventAction.SystemEnergyStateManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
            else
            {
                uint FormerState = Win32OtherFunctions.SetThreadExecutionState(Win32Enumerations.ExecutionState.ES_CONTINUOUS);
                if (FormerState is not 0)
                {
                    LogEntry Entry = BuildLogEntryForInformation("Utilizzo energetico del sistema modificato", EventAction.SystemEnergyStateManipulation);
                    Logger.WriteEntry(Entry);
                    return true;
                }
                else
                {
                    LogEntry Entry = BuildLogEntryForWarning("Non è stato possibile modificare le impostazioni energetiche del sistema", EventAction.SystemEnergyStateManipulation);
                    Logger.WriteEntry(Entry);
                    return false;
                }
            }
        }
        #endregion
        #endregion
        #region Pagefile Manipulation Methods
        /// <summary>
        /// Recupera informazioni sui file di paging.
        /// </summary>
        /// <returns>Un array di istanze di <see cref="PageFileInfo"/> con le informazioni sui file di paging presenti nel sistema.</returns>
        public static PageFileInfo[] GetPagefilesInfo()
        {
            List<PageFileInfo> Pagefiles = new();
            uint PageSize = GetMemoryPageSize();
            _ = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemPagefileInformation, IntPtr.Zero, 0, out uint ReturnLength);
            uint Size = ReturnLength;
            IntPtr PagefileInfoBuffer = Marshal.AllocHGlobal((int)Size);
            uint Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemPagefileInformation, PagefileInfoBuffer, Size, out _);
            while (Result is Win32Constants.STATUS_INFO_LENGTH_MISMATCH)
            {
                Size *= 2;
                PagefileInfoBuffer = Marshal.ReAllocHGlobal(PagefileInfoBuffer, (IntPtr)Size);
                Result = Win32OtherFunctions.NtQuerySystemInformation(Win32Enumerations.SystemInformationClass.SystemPagefileInformation, PagefileInfoBuffer, Size, out _);
            }
            if (Result is Win32Constants.STATUS_SUCCESS)
            {
                IntPtr SecondBuffer = PagefileInfoBuffer;
                Win32Structures.SYSTEM_PAGEFILE_INFORMATION Info = (Win32Structures.SYSTEM_PAGEFILE_INFORMATION)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.SYSTEM_PAGEFILE_INFORMATION));
                Pagefiles.Add(new(Info.PageFileName.Buffer.Remove(0, 4), Info.TotalSize, Info.TotalInUse, Info.PeakUsage, PageSize));
                while (Info.NextEntryOffset is not 0)
                {
                    SecondBuffer += (int)Info.NextEntryOffset;
                    Info = (Win32Structures.SYSTEM_PAGEFILE_INFORMATION)Marshal.PtrToStructure(SecondBuffer, typeof(Win32Structures.SYSTEM_PAGEFILE_INFORMATION));
                    string Filename = Info.PageFileName.Buffer.Remove(0, 4);
                    Pagefiles.Add(new(Filename, Info.TotalSize, Info.TotalInUse, Info.PeakUsage, PageSize));
                }
                Marshal.FreeHGlobal(PagefileInfoBuffer);
                return Pagefiles.ToArray();
            }
            else
            {
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile recuperare informazioni sui file di paging presenti nel sistema", EventAction.PageFileEnumeration, null, Result);
                Logger.WriteEntry(Entry);
                return null;
            }
        }

        /// <summary>
        /// Esegue la sottoscrizione agli eventi del registro di sistema relativi alla modifica della chiave con le informazioni sui file di paging.
        /// </summary>
        /// <param name="KeyHandle">Handle nativo alla chiave.</param>
        /// <param name="Event">Handle a un evento.</param>
        /// <returns>true se l'operazione è riuscita, false altrimenti.</returns>
        public static bool SubscribeToPagefileAddRemoveEvents(SafeRegistryHandle KeyHandle, SafeWaitHandle Event)
        {
            uint Result = Win32OtherFunctions.RegNotifyChangeKeyValue(KeyHandle.DangerousGetHandle(), false, Win32Enumerations.RegistryNotificationFilters.REG_NOTIFY_CHANGE_LAST_SET, Event.DangerousGetHandle(), true);
            if (Result is Win32Constants.ERROR_SUCCESS)
            {
                return true;
            }
            else
            {
                LogEntry Entry = BuildLogEntryForNTSTATUSError("Non è stato possibile sottoscrivere gli eventi relativi alla creazione e all'eliminazione dei file di paging presenti nel sistema", EventAction.PageFileMonitoring, null, Result);
                Logger.WriteEntry(Entry);
                return false;
            }
        }
        #endregion
        #region Utility Methods
        /// <summary>
        /// Converte una struttura <see cref="FILETIME"/> in un oggetto <see cref="DateTime"/>.
        /// </summary>
        /// <param name="TimeStructure">Struttua <see cref="FILETIME"/> con i dati necessari.</param>
        /// <returns>Oggetto <see cref="DateTime"/> creato dai dati contenuti nella struttura <see cref="FILETIME"/>.</returns>
        private static DateTime FileTimeToDateTime(Win32Structures.FILETIME TimeStructure)
        {
            byte[] TimeBytes = new byte[8];
            byte[] LowBytes = BitConverter.GetBytes(TimeStructure.LowDateTime);
            byte[] HighBytes = BitConverter.GetBytes(TimeStructure.HighDateTime);
            Array.ConstrainedCopy(LowBytes, 0, TimeBytes, 0, LowBytes.Length);
            Array.ConstrainedCopy(HighBytes, 0, TimeBytes, 4, HighBytes.Length);
            return DateTime.FromFileTime(BitConverter.ToInt64(TimeBytes, 0));
        }

        /// <summary>
        /// Converte una struttura <see cref="FILETIME"/> in un valore a 64 bit.
        /// </summary>
        /// <param name="TimeStructure">Struttura <see cref="FILETIME"/> con i dati necessari.</param>
        /// <returns>Un valore a 64 bit.</returns>
        private static long FileTimeToInt64(Win32Structures.FILETIME TimeStructure)
        {
            byte[] TimeBytes = new byte[8];
            byte[] LowBytes = BitConverter.GetBytes(TimeStructure.LowDateTime);
            byte[] HighBytes = BitConverter.GetBytes(TimeStructure.HighDateTime);
            Array.ConstrainedCopy(LowBytes, 0, TimeBytes, 0, LowBytes.Length);
            Array.ConstrainedCopy(HighBytes, 0, TimeBytes, 4, HighBytes.Length);
            return BitConverter.ToInt64(TimeBytes, 0);
        }

        /// <summary>
        /// Converte una struttura <see cref="Win32Structures.LUID"/> in un valore a 64 bit.
        /// </summary>
        /// <param name="LUID">Struttura <see cref="Win32Structures.LUID"/> da convertire.</param>
        /// <returns>Un valore a 64 bit.</returns>
        private static long LUIDToInt64(Win32Structures.LUID LUID)
        {
            byte[] LUIDBytes = new byte[8];
            byte[] LowBytes = BitConverter.GetBytes(LUID.LowPart);
            byte[] HighBytes = BitConverter.GetBytes(LUID.HighPart);
            Array.ConstrainedCopy(LowBytes, 0, LUIDBytes, 0, LowBytes.Length);
            Array.ConstrainedCopy(HighBytes, 0, LUIDBytes, 4, HighBytes.Length);
            return BitConverter.ToInt64(LUIDBytes, 0);
        }
        #region Logging Methods
        /// <summary>
        /// Recupera dati utilizzati per il logging.
        /// </summary>
        /// <param name="Handle">Handle al processo.</param>
        /// <param name="OnlyPID">Indica se deve essere recuperato solo l'ID del processo.</param>
        /// <param name="OnlyProcessName">Indica se deve essere recuperato solo il nome del processo.</param>
        /// <param name="PID">ID del processo.</param>
        /// <param name="ProcessName">Nome del processo.</param>
        /// <param name="DebugLoggingCall">Indica se metodo è stato chiamato durante il log di debug.</param>
        /// <returns>Un array con i dati.</returns>
        /// <remarks>L'array è composto dai seguenti elementi, nell'ordine indicato:<br/><br/>
        /// 1) Nome del processo<br/>
        /// 2) ID del processo</remarks>
        private static object[] GetDataForLogging(SafeProcessHandle Handle, bool OnlyPID, bool OnlyProcessName, uint PID = 0, string ProcessName = null)
        {
            object[] Data = new object[2];
            if (OnlyPID)
            {
                PID = GetProcessPID(Handle);
            }
            else if (OnlyProcessName)
            {
                ProcessName = GetProcessName(Handle, PID);
            }
            else
            {
                PID = GetProcessPID(Handle);
                ProcessName = GetProcessName(Handle, PID);
            }
            Data[0] = ProcessName;
            Data[1] = PID;
            return Data;
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log un errore derivato dalla Windows API.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione.</param>
        /// <param name="Handle">Handle al processo su cui l'azione era in corso.</param>
        /// <param name="ErrorCode">Codice di errore.</param>
        /// <param name="Message">Messaggio relativo al codice di errore.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        private static LogEntry BuildLogEntryForWin32Error(string EntryText, EventAction Action, SafeProcessHandle Handle, int? ErrorCode, string Message, uint? PID = null, string ProcessName = null)
        {
            LogEntry Entry;
            if (Handle != null && !Handle.IsInvalid)
            {
                object[] LoggingData;
                if (PID.HasValue && ProcessName == null)
                {
                    LoggingData = GetDataForLogging(Handle, false, true, PID.Value);
                }
                else if (!PID.HasValue && ProcessName != null)
                {
                    LoggingData = GetDataForLogging(Handle, true, false, ProcessName: ProcessName);
                }
                else
                {
                    LoggingData = GetDataForLogging(Handle, false, false);
                }
                if ((string)LoggingData[0] != Properties.Resources.UnavailableText)
                {
                    if (ErrorCode.HasValue)
                    {
                        Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1] + " codice di errore: " + ErrorCode + " (" + Message + ")", EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                    else
                    {
                        Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1], EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }

                }
                else
                {
                    if (ErrorCode.HasValue)
                    {
                        Entry = new(EntryText + ", PID: " + (uint)LoggingData[1] + " codice di errore: " + ErrorCode + " (" + Message + ")", EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                    else
                    {
                        Entry = new(EntryText + ", PID: " + (uint)LoggingData[1], EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                }
            }
            else
            {
                if (ErrorCode.HasValue)
                {
                    Entry = new(EntryText + ", codice di errore: " + ErrorCode + " (" + Message + ")", EventSource.WindowsAPI, EventSeverity.Error, Action);
                }
                else
                {
                    Entry = new(EntryText, EventSource.WindowsAPI, EventSeverity.Error, Action);
                }
            }
            return Entry;
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log un errore derivato dalla Windows Native API.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione.</param>
        /// <param name="Handle">Handle al processo su cui l'azione era in corso.</param>
        /// <param name="NTSTATUS">Codice di errore.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        private static LogEntry BuildLogEntryForNTSTATUSError(string EntryText, EventAction Action, SafeProcessHandle Handle, uint? NTSTATUS, uint? PID = null, string ProcessName = null)
        {
            LogEntry Entry;
            if (Handle != null && !Handle.IsInvalid)
            {
                object[] LoggingData;
                if (PID.HasValue && ProcessName == null)
                {
                    LoggingData = GetDataForLogging(Handle, false, true, PID.Value);
                }
                else if (!PID.HasValue && ProcessName != null)
                {
                    LoggingData = GetDataForLogging(Handle, true, false, ProcessName: ProcessName);
                }
                else
                {
                    LoggingData = GetDataForLogging(Handle, false, false);
                }
                if ((string)LoggingData[0] != Properties.Resources.UnavailableText)
                {
                    if (NTSTATUS.HasValue)
                    {
                        Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1] + " NTSTATUS: " + NTSTATUS, EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                    else
                    {
                        Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1], EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }

                }
                else
                {
                    if (NTSTATUS.HasValue)
                    {
                        Entry = new(EntryText + ", PID: " + (uint)LoggingData[1] + " NTSTATUS: " + NTSTATUS, EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                    else
                    {
                        Entry = new(EntryText + ", PID: " + (uint)LoggingData[1], EventSource.WindowsAPI, EventSeverity.Error, Action);
                    }
                }
            }
            else
            {
                if (NTSTATUS.HasValue)
                {
                    Entry = new(EntryText + ", NTSTATUS: " + NTSTATUS, EventSource.WindowsAPI, EventSeverity.Error, Action);
                }
                else
                {
                    Entry = new(EntryText, EventSource.WindowsAPI, EventSeverity.Error, Action);
                }
            }
            return Entry;
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log la riuscita di un'operazione.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione efettuata.</param>
        /// <param name="Handle">Handle al processo su cui l'azione è stata effettuata.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        /// <remarks>Se <paramref name="Action"/> ha valore <see cref="EventAction.ProcessTermination"/> oppure <see cref="EventAction.ProcessTreeTermination"/>, <paramref name="Handle"/> non è valido.</remarks>
        public static LogEntry BuildLogEntryForInformation(string EntryText, EventAction Action, SafeProcessHandle Handle, uint? PID = null, string ProcessName = null)
        {
            LogEntry Entry;
            if (Handle != null && !Handle.IsInvalid)
            {
                if (Action != EventAction.ProcessTermination || Action != EventAction.ProcessTreeTermination)
                {
                    object[] LoggingData;
                    if (PID.HasValue && ProcessName == null)
                    {
                        LoggingData = GetDataForLogging(Handle, false, true, PID.Value);
                    }
                    else if (!PID.HasValue && ProcessName != null)
                    {
                        LoggingData = GetDataForLogging(Handle, true, false, ProcessName: ProcessName);
                    }
                    else
                    {
                        LoggingData = GetDataForLogging(Handle, false, false);
                    }
                    if ((string)LoggingData[0] != Properties.Resources.UnavailableText)
                    {
                        Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1], EventSource.Process, EventSeverity.Information, Action);
                    }
                    else
                    {
                        Entry = new(EntryText + ", PID: " + (uint)LoggingData[1], EventSource.Process, EventSeverity.Information, Action);
                    }
                }
                else
                {
                    Entry = new(EntryText, EventSource.Process, EventSeverity.Information, Action);
                }
                
            }
            else
            {
                Entry = new(EntryText, EventSource.Process, EventSeverity.Information, Action);
            }
            return Entry;
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log la riuscita di un'operazione.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione efettuata.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        public static LogEntry BuildLogEntryForInformation(string EntryText, EventAction Action)
        {
            return new(EntryText, EventSource.Application, EventSeverity.Information, Action);
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log il fallimento di un'operazione per una condizione nota.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione efettuata.</param>
        /// <param name="Handle">Handle al processo su cui l'azione è stata effettuata.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        public static LogEntry BuildLogEntryForWarning(string EntryText, EventAction Action, SafeProcessHandle Handle, uint? PID = null, string ProcessName = null)
        {
            LogEntry Entry;
            if (Handle != null && !Handle.IsInvalid)
            {
                object[] LoggingData;
                if (PID.HasValue && ProcessName == null)
                {
                    LoggingData = GetDataForLogging(Handle, false, true, PID.Value);
                }
                else if (!PID.HasValue && ProcessName != null)
                {
                    LoggingData = GetDataForLogging(Handle, true, false, ProcessName: ProcessName);
                }
                else
                {
                    LoggingData = GetDataForLogging(Handle, false, false);
                }
                if ((string)LoggingData[0] != Properties.Resources.UnavailableText)
                {
                    Entry = new(EntryText + ", nome processo: " + (string)LoggingData[0] + ", PID: " + (uint)LoggingData[1], EventSource.Process, EventSeverity.Warning, Action);
                }
                else
                {
                    Entry = new(EntryText + ", PID: " + (uint)LoggingData[1], EventSource.Process, EventSeverity.Warning, Action);
                }

            }
            else
            {
                Entry = new(EntryText, EventSource.Process, EventSeverity.Warning, Action);
            }
            return Entry;
        }

        /// <summary>
        /// Costruisce una struttura <see cref="LogEntry"/> per mettere a log il fallimento di un'operazione per una condizione nota.
        /// </summary>
        /// <param name="EntryText">Testo della voce.</param>
        /// <param name="Action">Azione efettuata.</param>
        /// <returns><see cref="LogEntry"/> con le informazioni sull'evento.</returns>
        public static LogEntry BuildLogEntryForWarning(string EntryText, EventAction Action)
        {
            return new(EntryText, EventSource.Application, EventSeverity.Warning, Action);
        }
        #endregion
        #endregion
    }
}