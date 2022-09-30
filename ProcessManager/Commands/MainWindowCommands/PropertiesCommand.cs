using Microsoft.Win32.SafeHandles;
using ProcessManager.InfoClasses.ProcessStatistics;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProcessManager.Commands.MainWindowCommands
{
    public class PropertiesCommand : ICommand
    {
        private readonly ProcessInfoVM VM;

        public PropertiesCommand(ProcessInfoVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            WaitWindow WaitWindow = new();
            WaitWindow.Show();
            Thread WindowThread = new(() =>
            {
                ProcessGeneralInfo GeneralInfo = VM.GetProcessGeneralInformation(Parameter as ProcessInfo);
                ProcessStatistics Statistics = VM.GetProcessStatistics(Parameter as ProcessInfo);
                ObservableCollection<ThreadInfo> ThreadsInfo = VM.GetProcessThreadsInfo(Parameter as ProcessInfo);
                TokenInfo TokenInformation = VM.GetProcessTokenInfo(Parameter as ProcessInfo);
                List<MemoryRegionInfo> MemoryRegionsInfo = VM.GetProcessMemoryInfo(Parameter as ProcessInfo);
                List<ModuleInfo> ModulesInfo = VM.GetProcessModulesInfo(Parameter as ProcessInfo);
                HandleInfo[] HandlesInfo = VM.GetProcessHandlesInfo(Parameter as ProcessInfo);
                NetPerformanceInfo NetPerformanceInfo = VM.GetProcessNetPerformanceInfo(Parameter as ProcessInfo);
                ObservableCollection<HandleCountInfo> HandleCounters = new();
                List<string> HandleTypes = new();
                List<uint> HandleCounters2 = new();
                foreach (HandleInfo Info in HandlesInfo)
                {
                    if (!HandleTypes.Contains(Info.Type))
                    {
                        HandleTypes.Add(Info.Type);
                    }
                }
                foreach (string handletype in HandleTypes)
                {
                    HandleCounters2.Add((uint)HandlesInfo.Count(info => info.Type == handletype));
                }
                for (int i = 0; i < HandleTypes.Count; i++)
                {
                    HandleCounters.Add(new(HandleTypes[i], HandleCounters2[i]));
                }
                ProcessPropertiesVM PropertiesVM = new(GeneralInfo, Statistics, ThreadsInfo, TokenInformation, ModulesInfo, MemoryRegionsInfo, HandlesInfo.ToList(), NetPerformanceInfo, NativeHelpers.GetProcessHandle(((ProcessInfo)Parameter).PID), HandleCounters, Parameter as ProcessInfo);
                SafeProcessHandle ProcessHandle = NativeHelpers.GetProcessHandle(((ProcessInfo)Parameter).PID);
                WaitWindow.Dispatcher.Invoke(() => WaitWindow.Close());
                if (NetPerformanceInfo != null)
                {
                    ProcessPropertiesWindow Window = new(ProcessHandle, GeneralInfo.EnabledSettings, PropertiesVM);
                    PropertiesVM.Dispatcher = Window.Dispatcher;
                    Window.Closed += (sender2, e2) =>
                    {
                        Window.Dispatcher.InvokeShutdown();
                        ProcessHandle.Dispose();
                    };
                    Window.Show();
                    Dispatcher.Run();
                }
                else
                {
                    ProcessPropertiesWindowNoNet Window = new(ProcessHandle, GeneralInfo.EnabledSettings, PropertiesVM);
                    PropertiesVM.Dispatcher = Window.Dispatcher;
                    Window.Closed += (sender2, e2) => 
                    {
                        Window.Dispatcher.InvokeShutdown();
                        ProcessHandle.Dispose();
                    };
                    Window.Show();
                    Dispatcher.Run();
                }
            })
            {
                Name = "PropertiesWindowThread"
            };
            WindowThread.SetApartmentState(ApartmentState.STA);
            WindowThread.Start();
        }

        public event EventHandler CanExecuteChanged;
    }
}