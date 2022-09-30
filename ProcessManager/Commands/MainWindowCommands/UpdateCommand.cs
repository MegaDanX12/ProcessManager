using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using ProcessManager.InfoClasses.ServicesInfo;
using System.Globalization;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class UpdateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            AnonymousPipeServerStream InPipe = new(PipeDirection.In, HandleInheritability.Inheritable);
            AnonymousPipeServerStream OutPipe = new(PipeDirection.Out, HandleInheritability.Inheritable);
            string AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ProcessStartInfo StartInfo;
            if (Settings.RestartAfterUpdate)
            {
                StartInfo = new("Updater.exe", AppDomain.CurrentDomain.BaseDirectory + " " + AppVersion + " " + InPipe.GetClientHandleAsString() + " " + OutPipe.GetClientHandleAsString() + CurrentProcess.Id.ToString("D0", CultureInfo.CurrentCulture) + " restart");
                CurrentProcess.Dispose();
                _ = Process.Start(StartInfo);
            }
            else
            {
                StartInfo = new("Updater.exe", AppDomain.CurrentDomain.BaseDirectory + " " + AppVersion + " " + InPipe.GetClientHandleAsString() + " " + OutPipe.GetClientHandleAsString() + CurrentProcess.Id.ToString("D0", CultureInfo.CurrentCulture));
                CurrentProcess.Dispose();
                _ = Process.Start(StartInfo);
            }
            Thread MessageProcessingThread = new(new ParameterizedThreadStart(ProcessMessages));
            Tuple<AnonymousPipeServerStream, AnonymousPipeServerStream> ThreadParameters = new(InPipe, OutPipe);
            MessageProcessingThread.Start(ThreadParameters);
        }

        /// <summary>
        /// Elabora i messaggi in arrivo dall'applicazione di aggiornamento.
        /// </summary>
        /// <param name="state">Pipe di comunicazione.</param>
        private void ProcessMessages(object state)
        {
            string Response;
            Tuple<AnonymousPipeServerStream, AnonymousPipeServerStream> Pipes = state as Tuple<AnonymousPipeServerStream, AnonymousPipeServerStream>;
            using (StreamReader PipeReader = new(Pipes.Item1))
            {
                using (StreamWriter PipeWriter = new(Pipes.Item2))
                {
                    Response = PipeReader.ReadLine();
                    if (Response is "NewVersion")
                    {
                        if (Settings.UpdateDownloadAfterConfirmation)
                        {
                            MessageBoxResult Result = MessageBox.Show(Properties.Resources.UpdateDownloadConfirmationMessage, Properties.Resources.UpdateDownloadConfirmationMessageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (Result == MessageBoxResult.Yes)
                            {
                                PipeWriter.WriteLine("Download");
                                Response = PipeReader.ReadLine();
                                if (Response is "InstallReady")
                                {
                                    if (Settings.UpdateDownloadCompletedNotifications)
                                    {
                                        new ToastContentBuilder().AddText(Properties.Resources.UpdateDownloadedNotificationText, hintMaxLines: 1).Show();
                                    }
                                    if (Settings.UpdateDownloadOnly)
                                    {
                                        PipeWriter.WriteLine("DoNotInstall");
                                    }
                                    else
                                    {
                                        if (Settings.UpdateInstallAfterConfirmation)
                                        {
                                            Result = MessageBox.Show(Properties.Resources.UpdateInstallConfirmationMessage, Properties.Resources.UpdateInstallConfirmationMessageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                            if (Result == MessageBoxResult.Yes)
                                            {

                                                PipeWriter.WriteLine("Confirmed");
                                                if (Settings.UpdateInstallStartedNotifications)
                                                {
                                                    new ToastContentBuilder().AddText(Properties.Resources.UpdateReadyToInstallNotificationText, hintMaxLines: 1).Show();
                                                }
                                                Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
                                            }
                                            else
                                            {
                                                PipeWriter.WriteLine("Stop");
                                            }
                                        }
                                        else
                                        {
                                            PipeWriter.WriteLine("Confirmed");
                                            if (Settings.UpdateInstallStartedNotifications)
                                            {
                                                new ToastContentBuilder().AddText(Properties.Resources.UpdateReadyToInstallNotificationText, hintMaxLines: 1).Show();
                                            }
                                            Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
                                        }
                                    }
                                }
                                else if (Response is "GenericError")
                                {
                                    _ = MessageBox.Show(Properties.Resources.UpdateGenericErrorMessage, Properties.Resources.UpdateGenericErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                PipeWriter.WriteLine("Terminate");
                            }
                        }
                        else
                        {
                            PipeWriter.WriteLine("Download");
                            Response = PipeReader.ReadLine();
                            if (Response is "InstallReady")
                            {
                                if (Settings.UpdateDownloadCompletedNotifications)
                                {
                                    new ToastContentBuilder().AddText(Properties.Resources.UpdateDownloadedNotificationText, hintMaxLines: 1).Show();
                                }
                                if (Settings.UpdateDownloadOnly)
                                {
                                    PipeWriter.WriteLine("DoNotInstall");
                                }
                                else
                                {
                                    if (Settings.UpdateInstallAfterConfirmation)
                                    {
                                        MessageBoxResult Result = MessageBox.Show(Properties.Resources.UpdateInstallConfirmationMessage, Properties.Resources.UpdateInstallConfirmationMessageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                        if (Result == MessageBoxResult.Yes)
                                        {

                                            PipeWriter.WriteLine("Confirmed");
                                            if (Settings.UpdateInstallStartedNotifications)
                                            {
                                                new ToastContentBuilder().AddText(Properties.Resources.UpdateReadyToInstallNotificationText, hintMaxLines: 1).Show();
                                            }
                                            Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
                                        }
                                        else
                                        {
                                            PipeWriter.WriteLine("Stop");
                                        }
                                    }
                                    else
                                    {
                                        PipeWriter.WriteLine("Confirmed");
                                        if (Settings.UpdateInstallStartedNotifications)
                                        {
                                            new ToastContentBuilder().AddText(Properties.Resources.UpdateReadyToInstallNotificationText, hintMaxLines: 1).Show();
                                        }
                                        Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
                                    }
                                }
                            }
                            else if (Response is "GenericError")
                            {
                                _ = MessageBox.Show(Properties.Resources.UpdateGenericErrorMessage, Properties.Resources.UpdateGenericErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        if (Response is "ConnectionError" or "ConnectionFailed")
                        {
                            _ = MessageBox.Show(Properties.Resources.UpdateConnectionErrorMessage, Properties.Resources.UpdateConnectionErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (Response is "InvalidVersion")
                        {
                            _ = MessageBox.Show(Properties.Resources.UpdateGenericErrorMessage, Properties.Resources.UpdateGenericErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}