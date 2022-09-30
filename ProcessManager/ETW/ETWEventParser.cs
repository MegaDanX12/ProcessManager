using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;

namespace ProcessManager.ETW
{
    /// <summary>
    /// Elabora gli eventi in arrivo dai provider ETW.
    /// </summary>
    public static class ETWEventParser
    {
        /// <summary>
        /// Istanza di <see cref="KernelTraceEventParser"/> in grado di elaborare gli eventi kernel per la sessione principale.
        /// </summary>
        private static KernelTraceEventParser EventParser;

        /// <summary>
        /// Istanza del viewmodel principale.
        /// </summary>
        private static ProcessInfoVM MainViewmodel;

        /// <summary>
        /// Istanza del viewmodel per le proprietà di un processo.
        /// </summary>
        private static ProcessPropertiesVM ProcessPropertiesViewmodel;

        /// <summary>
        /// Esegue la sottoscrizione agli eventi relativi alla sessione ETW principale.
        /// </summary>
        /// <param name="Parser">Istanza di <see cref="KernelTraceEventParser"/> in grado di elaborare gli eventi kernel per la sessione principale.</param>
        public static void InitializeMainEventsParser(KernelTraceEventParser Parser, ProcessInfoVM VM)
        {
            MainViewmodel = VM;
            EventParser = Parser;
            EventParser.ProcessStart += MainEventsParser_ProcessStart;
            EventParser.ProcessStop += MainEventsParser_ProcessStop;
            EventParser.ThreadStart += MainEventsParser_ThreadStart;
            EventParser.ThreadStop += MainEventsParser_ThreadStop;
        }
        #region Main Events Parsing Methods
        private static void MainEventsParser_ThreadStop(ThreadTraceData obj)
        {
            MainViewmodel.UpdateProcessData("UpdateThreadsNew", (uint)obj.ProcessID);
        }

        private static void MainEventsParser_ThreadStart(ThreadTraceData obj)
        {
            MainViewmodel.UpdateProcessData("UpdateThreadsEnd", (uint)obj.ProcessID);
        }

        private static void MainEventsParser_ProcessStop(ProcessTraceData obj)
        {
            MainViewmodel.UpdateProcessData("RemoveProcess", (uint)obj.ProcessID);
        }

        private static void MainEventsParser_ProcessStart(ProcessTraceData obj)
        {
            string ProcessName = obj.ProcessName + ".exe";
            MainViewmodel.UpdateProcessData("AddProcess", (uint)obj.ProcessID, ProcessName, obj.ImageFileName, obj.CommandLine);
        }
        #endregion
        /// <summary>
        /// Esegue la sottoscrizione agli eventi relativi ai dati sulle proprietà di un processo.
        /// </summary>
        /// <param name="Parser">Istanza di <see cref="KernelTraceEventParser"/> in grado di elaborare gli eventi kernel per la sessione relativa alle proprietà di un processo.</param>
        public static void InitializeProcessPropertiesEventsParser(ProcessPropertiesVM VM)
        {
            ProcessPropertiesViewmodel = VM;
            EventParser.ThreadStart += ProcessPropertiesEventsParser_ThreadStart;
            EventParser.ThreadStop += ProcessPropertiesEventsParser_ThreadStop;
            EventParser.ImageLoad += ProcessPropertiesEventsParser_ImageLoad;
            EventParser.ImageUnload += ProcessPropertiesEventsParser_ImageUnload;
            EventParser.ObjectCreateHandle += ProcessPropertiesEventsParser_ObjectCreateHandle;
            EventParser.ObjectCloseHandle += ProcessPropertiesEventsParser_ObjectCloseHandle;
            EventParser.ObjectDuplicateHandle += ProcessPropertiesEventsParser_ObjectDuplicateHandle;
            EventParser.VirtualMemAlloc += ProcessPropertiesEventsParser_VirtualMemAlloc;
            EventParser.VirtualMemFree += ProcessPropertiesEventsParser_VirtualMemFree;
        }
        #region Process Properties Events Parsing Methods
        private static void ProcessPropertiesEventsParser_VirtualMemFree(VirtualAllocTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && obj.ProcessID == PID)
            {
                ProcessPropertiesViewmodel.UpdateMemoryRegionsInfo("RemoveRegion", BaseAddress: new IntPtr((long)obj.BaseAddr));
            }
        }

        private static void ProcessPropertiesEventsParser_VirtualMemAlloc(VirtualAllocTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && obj.ProcessID == PID)
            {
                MemoryRegionInfo Info = NativeHelpers.GetMemoryRegionInfo(ProcessPropertiesViewmodel.Handle, new IntPtr((long)obj.BaseAddr));
                ProcessPropertiesViewmodel.UpdateMemoryRegionsInfo("AddRegion", Info);
            }
        }

        private static void ProcessPropertiesEventsParser_ObjectDuplicateHandle(ObjectDuplicateHandleTraceData obj)
        {
            uint ProcessID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (ProcessID is not 0 && ProcessID == (uint)obj.TargetProcessID)
            {
                HandleInfo Info = NativeHelpers.GetHandleInfo((uint)obj.TargetHandle, (IntPtr)obj.Object, ProcessPropertiesViewmodel.Handle, obj.ObjectName);
                ProcessPropertiesViewmodel.UpdateHandlesList("AddHandle", Info);
            }
        }

        private static void ProcessPropertiesEventsParser_ObjectCloseHandle(ObjectHandleTraceData obj)
        {
            if (obj.ProcessID is not -1)
            {
                uint ProcessID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
                if (ProcessID is not 0 && ProcessID == obj.ProcessID)
                {
                    ProcessPropertiesViewmodel.UpdateHandlesList("RemoveHandle", HandleValue: ((uint)obj.Handle).ToString("X", CultureInfo.InvariantCulture));
                }
            }
            else
            {
                ProcessPropertiesViewmodel.UpdateHandlesList("RemoveHandle", HandleValue: ((uint)obj.Handle).ToString("X", CultureInfo.InvariantCulture));
            }
        }

        private static void ProcessPropertiesEventsParser_ObjectCreateHandle(ObjectHandleTraceData obj)
        {
            if (obj.ProcessID is not -1)
            {
                uint ProcessID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
                if (ProcessID is not 0 && ProcessID == (uint)obj.ProcessID)
                {
                    HandleInfo Info = NativeHelpers.GetHandleInfo((uint)obj.Handle, (IntPtr)obj.Object, ProcessPropertiesViewmodel.Handle, obj.ObjectName);
                    ProcessPropertiesViewmodel.UpdateHandlesList("AddHandle", Info);
                }
            }
        }

        private static void ProcessPropertiesEventsParser_ImageUnload(ImageLoadTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && PID == obj.ProcessID)
            {
                ProcessPropertiesViewmodel.UpdateModulesList("RemoveModule", ModuleName: obj.FileName);
            }
        }

        private static void ProcessPropertiesEventsParser_ImageLoad(ImageLoadTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && PID == obj.ProcessID)
            {
                string FullPath = NativeHelpers.FindModulePath(ProcessPropertiesViewmodel.Handle, obj.FileName, (uint)obj.ImageSize, (uint)obj.ImageChecksum, ProcessPropertiesViewmodel.Info.FullPath);
                if (FullPath is not null)
                {
                    FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(FullPath);
                    ProcessPropertiesViewmodel.UpdateModulesList("AddModule", FullPath, (uint)obj.ImageSize, (uint)obj.ImageBase, VersionInfo.FileDescription);
                }
                else
                {
                    ProcessPropertiesViewmodel.UpdateModulesList("AddModule", Properties.Resources.UnavailableText, (uint)obj.ImageSize, (uint)obj.ImageBase, Properties.Resources.UnavailableText);
                }
            }
        }

        private static void ProcessPropertiesEventsParser_ThreadStop(ThreadTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && PID == obj.ProcessID)
            {
                ProcessPropertiesViewmodel.UpdateThreadData("RemoveThread", (uint)obj.ThreadID);
            }
        }

        private static void ProcessPropertiesEventsParser_ThreadStart(ThreadTraceData obj)
        {
            uint PID = NativeHelpers.GetProcessPID(ProcessPropertiesViewmodel.Handle);
            if (PID is not 0 && PID == obj.ProcessID)
            {
                ProcessPropertiesViewmodel.UpdateThreadData("AddThread", (uint)obj.ThreadID);
            }
        }
        #endregion
        /// <summary>
        /// Annulla la sottoscrizione agli eventi relativi ai dati sulle proprietà di un processo.
        /// </summary>
        public static void ShutdownProcessPropertiesEventsParser()
        {
            ProcessPropertiesViewmodel = null;
            EventParser.ThreadStart -= ProcessPropertiesEventsParser_ThreadStart;
            EventParser.ThreadStop -= ProcessPropertiesEventsParser_ThreadStop;
            EventParser.ImageLoad -= ProcessPropertiesEventsParser_ImageLoad;
            EventParser.ImageUnload -= ProcessPropertiesEventsParser_ImageUnload;
            EventParser.ObjectCreateHandle -= ProcessPropertiesEventsParser_ObjectCreateHandle;
            EventParser.ObjectCloseHandle -= ProcessPropertiesEventsParser_ObjectCloseHandle;
            EventParser.ObjectDuplicateHandle -= ProcessPropertiesEventsParser_ObjectDuplicateHandle;
            EventParser.VirtualMemAlloc -= ProcessPropertiesEventsParser_VirtualMemAlloc;
            EventParser.VirtualMemFree -= ProcessPropertiesEventsParser_VirtualMemFree;
        }
    }
}