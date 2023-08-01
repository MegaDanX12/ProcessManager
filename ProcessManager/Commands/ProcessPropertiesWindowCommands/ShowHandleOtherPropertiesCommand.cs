using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.HandleSpecificInfo;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ShowHandleOtherPropertiesCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ShowHandleOtherPropertiesCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Tuple<SafeProcessHandle, HandleInfo> Parameters = Parameter as Tuple<SafeProcessHandle, HandleInfo>;
            Window Window;
            switch (Parameters.Item2.Type)
            {
                case "WindowStation":
                    WindowStationInfo WindowStationInfo = NativeHelpers.GetWindowStationSpecificInfo(Parameters.Item1, Parameters.Item2);
                    Window = new WindowStationInfoWindow(WindowStationInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Desktop":
                    DesktopInfo DesktopInfo = NativeHelpers.GetDesktopSpecificInfo(Parameters.Item1, Parameters.Item2);
                    Window = new DesktopInfoWindow(DesktopInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Timer":
                    TimerInfo TimerInfo = NativeHelpers.GetTimerSpecificInfo(Parameters.Item1, Parameters.Item2, false);
                    Window = new TimerInfoWindow(TimerInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Semaphore":
                    SemaphoreInfo SemaphoreInfo = NativeHelpers.GetSemaphoreSpecificInfo(Parameters.Item1, Parameters.Item2, IntPtr.Zero);
                    Window = new SemaphoreInfoWindow(VM, SemaphoreInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Section":
                    SectionInfo SectionInfo = NativeHelpers.GetSectionSpecificInfo(Parameters.Item1, Parameters.Item2);
                    Window = new SectionInfoWindow(SectionInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Mutant":
                    MutantInfo MutantInfo = NativeHelpers.GetMutantSpecificInfo(Parameters.Item1, Parameters.Item2, false);
                    Window = new MutantInfoWindow(MutantInfo);
                    _ = Window.ShowDialog();
                    break;
                case "Event":
                    EventInfo EventInfo = NativeHelpers.GetEventSpecificInfo(Parameters.Item1, Parameters.Item2, false);
                    Window = new EventInfoWindow(EventInfo);
                    _ = Window.ShowDialog();
                    break;
                case "File":
                    FileInfo FileInfo = NativeHelpers.GetFileSpecificInfo(Parameters.Item1, Parameters.Item2);
                    Window = new FileInfoWindow(FileInfo);
                    _ = Window.ShowDialog();
                    break;
                default:
                    _ = MessageBox.Show(Properties.Resources.NoSpecificInformationAvailableMessage, Properties.Resources.NoSpecificInformationAvailableTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}