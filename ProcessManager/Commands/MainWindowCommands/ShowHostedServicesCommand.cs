using ProcessManager.InfoClasses.ServicesInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using ProcessManager.WMI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class ShowHostedServicesCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public ShowHostedServicesCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (VM.ServicesData.FeatureAvailable)
            {
                ProcessInfo Info = Parameter as ProcessInfo;
                string ParentProcessName = WMIProcessInfoMethods.GetParentProcessName(Info.CreationTime, Info.PID);
                if (ParentProcessName.Contains("services.exe"))
                {
                    List<Service> Services = null;
                    if (Info.Name == "svchost.exe")
                    {
                        if (Info.CommandLine.Contains("-k"))
                        {
                            int FirstSpaceIndex = Info.CommandLine.IndexOf(" ", StringComparison.CurrentCulture);
                            string TrimmedCommandLine = Info.CommandLine.Substring(FirstSpaceIndex + 4);
                            FirstSpaceIndex = TrimmedCommandLine.IndexOf(" ", StringComparison.CurrentCulture);
                            if (FirstSpaceIndex == -1)
                            {
                                Services = VM.ServicesData.GetProcessHostedServices(Info.PID, TrimmedCommandLine);
                            }
                            else
                            {
                                TrimmedCommandLine = TrimmedCommandLine.Remove(FirstSpaceIndex);
                                Services = VM.ServicesData.GetProcessHostedServices(Info.PID, TrimmedCommandLine);
                            }
                        }
                    }
                    else
                    {
                        Services = VM.ServicesData.GetProcessHostedServices(Info.PID);
                    }
                    if (Services.Count > 0)
                    {
                        HostedServicesDataVM ServicesVM = new(Services.ToArray(), Info, VM.ServicesData);
                        ServicesInfoWindow Window = new(ServicesVM);
                        Window.Show();
                    }
                    else
                    {
                        _ = MessageBox.Show(Properties.Resources.NoServicesHostedErrorMessage, Properties.Resources.NoServicesHostedErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _ = MessageBox.Show(Properties.Resources.NoServicesHostedErrorMessage, Properties.Resources.NoServicesHostedErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _ = MessageBox.Show(Properties.Resources.ServiceMonitoringUnavailableErrorMessage, Properties.Resources.ServiceMonitoringUnavailableErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}