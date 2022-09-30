using Microsoft.Win32;
using ProcessManager.Commands.PagefilesListWindowCommands;
using ProcessManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.ViewModels
{
    public class PageFileListVM : IDisposable
    {
        /// <summary>
        /// Informazioni sui file di paging presenti nel sistema.
        /// </summary>
        public ObservableCollection<PageFileInfo> PageFiles { get; }

        /// <summary>
        /// Comando per la creazione di un nuovo file di paging.
        /// </summary>
        public ICommand CreatePagefileCommand { get; }

        /// <summary>
        /// Comando per l'eliminazione di un file di paging.
        /// </summary>
        public ICommand DeletePagefileCommand { get; }

        /// <summary>
        /// Comando per cambiare la dimensione di un file di paging.
        /// </summary>
        public ICommand ChangePagefileSizeCommand { get; }

        /// <summary>
        /// Timer per l'aggiornamento dei dati.
        /// </summary>
        private readonly Timer UpdateDataTimer;

        private readonly object LockObject = new();

        /// <summary>
        /// Evento che indica un cambiamento nella chiave monitorata del registro di sistema.
        /// </summary>
        private readonly AutoResetEvent RegistryEvent = new(false);

        /// <summary>
        /// Chiave di registro con le informazioni sui file di paging.
        /// </summary>
        private RegistryKey PagefileDataKey;

        /// <summary>
        /// Indica che il monitoraggio del registro di sistema deve essere terminato.
        /// </summary>
        private bool RegistryMonitorTermination;
        private bool disposedValue;

        public PageFileListVM()
        {
            PageFiles = new(NativeHelpers.GetPagefilesInfo());
            Thread RegistryNotificationThread = new(new ThreadStart(ProcessRegistryEvent));
            CreatePagefileCommand = new CreatePagefileCommand();
            ChangePagefileSizeCommand = new ChangePagefileSizeCommand();
            DeletePagefileCommand = new DeletePagefileCommand();
            UpdateDataTimer = new((state) => UpdateData("UpdateData"), null, 5000, Timeout.Infinite);
            RegistryNotificationThread.Start();
        }

        /// <summary>
        /// Elabora gli eventi del registro di sistema.
        /// </summary>
        private void ProcessRegistryEvent()
        {
            string[] PagefileNames;
            List<string> KnownPagefileNames = new();
            IEnumerable<string> DifferencePagefiles;
            PagefileDataKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management");
            while (!RegistryMonitorTermination)
            {
                if (NativeHelpers.SubscribeToPagefileAddRemoveEvents(PagefileDataKey.Handle, RegistryEvent.SafeWaitHandle))
                {
                    LogEntry Entry = NativeHelpers.BuildLogEntryForInformation("Iniziato monitoraggio dei dati sui file di paging", EventAction.PageFileMonitoring);
                    Logger.WriteEntry(Entry);
                    _ = RegistryEvent.WaitOne();
                    if (!RegistryMonitorTermination)
                    {
                        PagefileNames = (string[])PagefileDataKey.GetValue("PagingFiles");
                        if (PagefileNames.Length != PageFiles.Count)
                        {
                            foreach (PageFileInfo file in PageFiles)
                            {
                                KnownPagefileNames.Add(file.PageFilePath);
                            }
                            if (PagefileNames.Length < PageFiles.Count)
                            {
                                DifferencePagefiles = KnownPagefileNames.SkipWhile(name => PagefileNames.Contains(name));
                                foreach (string filename in DifferencePagefiles)
                                {
                                    UpdateData("PagefileRemoved", filename);
                                }
                            }
                            else
                            {
                                List<PageFileInfo> Pagefiles = NativeHelpers.GetPagefilesInfo().ToList();
                                _ = Pagefiles.RemoveAll(info => PageFiles.Contains(info));
                                foreach (PageFileInfo info in Pagefiles)
                                {
                                    UpdateData("PagefileAdded", NewInfo: info);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Aggiorna i dati.
        /// </summary>
        /// <param name="Operation">Operazione da eseguire.</param>
        /// <param name="Name">Nome dl file di paging da rimuovere.</param>
        /// <param name="NewInfo">Istanza di <see cref="PageFileInfo"/> che rappresenta il nuovo file di paging.</param>
        public void UpdateData(string Operation, string Name = null, PageFileInfo NewInfo = null)
        {
            lock (LockObject)
            {
                switch (Operation)
                {
                    case "UpdateData":
                        for (int i = 0; i < PageFiles.Count; i++)
                        {
                            PageFiles[i].UpdateData();
                        }
                        _ = UpdateDataTimer.Change(5000, Timeout.Infinite);
                        break;
                    case "PagefileAdded":
                        Application.Current.Dispatcher.Invoke(() => PageFiles.Add(NewInfo));
                        break;
                    case "PagefileRemoved":
                        PageFileInfo Info = PageFiles.First(info => info.PageFilePath == Name);
                        _ = Application.Current.Dispatcher.Invoke(() => PageFiles.Remove(Info));
                        break;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RegistryMonitorTermination = true;
                    _ = RegistryEvent.Set();
                    PagefileDataKey.Dispose();
                    RegistryEvent.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}