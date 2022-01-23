using DbgHelp.MinidumpFiles;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
//using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Win32.SafeHandles;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MinidumpViewer.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

        }



        private const int LVG_SYSTEM_INFO = 0;
        private const int LVG_CPU_INFO = 1;

        private void LoadDumpFile_Click(object sender, RoutedEventArgs e)
        {
            HandleIDList.Text = "";
            ModuleNameText.Text = "";
            ExceptionBox.Text = "";
            CommentABox.Text = "";
            CommentWBox.Text = "";
            ThreadInfoText.Text = "";
            ThreadNameText.Text = "";
            ThreadsText.Text = "";
            Memory32Text.Text = "";
            Memory64Text.Text = "";
            MemoryInfoText.Text = "";
            SystemInfoText.Text = "";
            SystemMemText.Text = "";
            MiscInfoText.Text = "";
            UnloadedText.Text = "";
            LoadFile();
        }


        public async void LoadFile()
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".dmp");
                Globals.storageFile = await picker.PickSingleFileAsync();
                Output.Text = $"Loading File: {Globals.storageFile.Name}\n\n";
                Stream stream = await Globals.storageFile.OpenStreamForReadAsync();

                var fileStream = Globals.storageFile.CreateSafeFileHandle(FileAccess.Read);

                FileStream fsStream = new FileStream(fileStream, FileAccess.Read);
                 Globals._miniDumpFile = MiniDumpFile.OpenExisting(fsStream);

                DumpCommentData();
                DumpExceptionData();
                DumpHandleData();
                DumpMemoryData();
                DumpModuleData();
                DumpThreadData();
                DumpSystemData();
                DumpMiscData();
                Globals._miniDumpFile.Dispose();
            }
            catch (Exception ex)
            {
                ExceptionHelper.Exceptions.ThrownExceptionErrorExtended(ex);
            }
        }




        private string FormattedTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.ToString();

            //if (timeSpan.TotalMilliseconds < 1000)
            //    return String.Format("{0}ms", timeSpan.TotalMilliseconds);
            //else if (timeSpan.TotalSeconds < 60)
            //    return String.Format("{0}.{1}s", timeSpan.Seconds, timeSpan.Milliseconds);
            //else if (timeSpan.TotalMinutes < 60)
            //    return String.Format("{0}:{1}min", timeSpan.Minutes, timeSpan.Seconds);
            //else
            //    return timeSpan.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        public void DumpHandleData()
        {
            MiniDumpHandleDescriptor[] handleData = Globals._miniDumpFile.ReadHandleData();
            uint handleAttributes = 0;
            uint handleGrantedAccess = 0;
            uint handleHandleCount = 0;
            ulong handleHandleId = 0;
            bool handleIsHandleDescriptor2 = false;
            MiniDumpHandleObjectInformation handleObjectInfo = null;
            uint handleObjectInfoRva = 0;
            string handleObjectName = "";
            uint handlePointerCount = 0;
            string handleTypeName = "";

            //List<string> list = new List<string>();
            foreach (var handle in handleData)
            {
                try
                {
                    handleAttributes = handle.Attributes;
                    handleGrantedAccess = handle.GrantedAccess;
                    handleHandleCount = handle.HandleCount;
                    handleHandleId = handle.HandleId;
                    //Globals.minidumpHandleIDs += $"{Formatters.FormatAsHex(handleHandleId)}\n";
                    handleIsHandleDescriptor2 = handle.IsHandleDescriptor2;
                    handleObjectInfo = handle.ObjectInfo;
                    handleObjectInfoRva = handle.ObjectInfoRva;
                    handleObjectName = handle.ObjectName;

                    //list.Add($"ID: {Formatters.FormatAsHex(handleHandleId)}\nType: {handleTypeName}\nObject: {handleObjectName}\n\n");
                    handlePointerCount = handle.PointerCount;
                    handleTypeName = handle.TypeName;
                    Globals.minidumpHandleObjects += $"\nID: {Formatters.FormatAsHex(handleHandleId)}\n";
                    if (handleTypeName == string.Empty)
                    {

                    }
                    else
                    {
                        Globals.minidumpHandleObjects += $"Type: {handleTypeName}\n";
                    }
                    if (handleObjectName == string.Empty)
                    {

                    }
                    else
                    {
                        Globals.minidumpHandleObjects += $"Object: {handleObjectName}\n";
                    }

                    // Globals.minidumpHandleTypeName += $"{handleTypeName}\n";
                    HandleIDList.Text = Globals.minidumpHandleObjects;
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DumpModuleData()
        {
            MiniDumpModule[] moduleData = Globals._miniDumpFile.ReadModuleList();

            string ModuleData = "";
            foreach (var module in moduleData)
            {
                try
                {

                    ModuleData += $"{module.PathAndFileName}\n" +
                        $"File Size: {module.SizeOfImageFormatted}\n" +
                        $"File Version: {module.FileVersion}\n" +
                        $"Address: {module.BaseOfImageFormatted}\n\n";
                }
                catch (Exception ex)
                {

                }
            }
            ModuleNameText.Text = ModuleData;

            MiniDumpUnloadedModulesStream unloadedModulesStream = Globals._miniDumpFile.ReadUnloadedModuleList();


            Globals.unloadedModuleData = "[Unloaded Modules]\n";
            if (unloadedModulesStream.NumberOfEntries == 0)
            {
                Globals.unloadedModuleData = "No data found for stream";
                //return;

            }
            else
            {

                foreach (MiniDumpUnloadedModule unloadedModule in unloadedModulesStream.Entries)
                {

                    Globals.unloadedModuleData += $"{unloadedModule.ModuleName}\n" +
                        $"Size: {Formatters.FormatAsSizeString(unloadedModule.SizeOfImage)}\n" +
                        $"Time/Date Stamp: {unloadedModule.TimeDateStamp}\n" +
                        $"Base Of Address: { Formatters.FormatAsMemoryAddress(unloadedModule.BaseOfImage)}";
                }
            }
            UnloadedText.Text = Globals.unloadedModuleData;

        }

        /// <summary>
        /// 
        /// </summary>
        public void DumpExceptionData()
        {
            MiniDumpExceptionStream exceptionStream = Globals._miniDumpFile.ReadExceptionStream();


            if (exceptionStream == null)
            {
                Globals.exceptionStreamExceptionFlag = "No data found for stream";
            }
            else
            {

                Globals.exceptionStreamExceptionCode = $"{Formatters.FormatAsHex(exceptionStream.ExceptionRecord.ExceptionCode)}";
                if (exceptionStream.ExceptionRecord.ExceptionFlags == MiniDumpException.EXCEPTION_NONCONTINUABLE)
                {
                    Globals.exceptionStreamExceptionFlag = " EXCEPTION_NONCONTINUABLE";
                }
                else
                {
                    Globals.exceptionStreamExceptionFlag = String.Format($"{Formatters.FormatAsHex(exceptionStream.ExceptionRecord.ExceptionFlags)}");
                }
                Globals.exceptionStreamRecordRaw = String.Format($"{Formatters.FormatAsHex(exceptionStream.ExceptionRecord.ExceptionRecordRaw)}");
                Globals.exceptionStreamExceptionAddress = String.Format($"{Formatters.FormatAsHex(exceptionStream.ExceptionRecord.ExceptionAddress)}");
                Globals.exceptionStreamNumberParameters = String.Format($"{exceptionStream.ExceptionRecord.NumberParameters}");

                // TreeNode informationNode = exceptionNode.Nodes.Add("ExceptionInformation");

                for (int i = 0; i < exceptionStream.ExceptionRecord.ExceptionInformation.Length; i++)
                {
                    Globals.exceptionStreamExceptionInfo += String.Format($"    [{i}]: 0x{Formatters.FormatAsHex(exceptionStream.ExceptionRecord.ExceptionInformation[i])}\n");
                }

            }
            ExceptionBox.Text = $"Exception Code: {Globals.exceptionStreamExceptionCode}\n" +
                $"Exception Flags: {Globals.exceptionStreamExceptionFlag}\n" +
                $"Exception Record: {Globals.exceptionStreamRecordRaw}\n" +
                $"Exception Address: {Globals.exceptionStreamExceptionAddress}\n" +
                $"Number Parameters: {Globals.exceptionStreamNumberParameters}\n" +
                $"Exception Information:\n{Globals.exceptionStreamExceptionInfo}";
        }

        /// <summary>
        /// 
        /// </summary>
        public void DumpThreadData()
        {
            MiniDumpThread[] threadData = Globals._miniDumpFile.ReadThreadList();
            string threadDataArray = "";
            foreach (var thread in threadData)
            {
                try
                {



                    threadDataArray += $"ThreadID: 0x{thread.ThreadId.ToString("x8")} ({thread.ThreadId})\n" +
                    $"Suspend Count: {thread.SuspendCount}\n" +
                    $"Priority Class: {thread.PriorityClass}\n" +
                    $"Priority: {thread.Priority}\n" +
                    $"Teb: {thread.Teb.ToString("x8")}\n" +
                    $"Thread Stack: {thread.Stack.StartOfMemoryRangeFormatted} ({thread.Stack.Memory.DataSizePretty})\n" +
                    $"Context Data Size: {thread.ThreadContext.DataSize} bytes\n\n";
                }
                catch (Exception ex)
                {

                }
                ThreadsText.Text = threadDataArray;
            }

            MiniDumpThreadInfo[] threadInfoData = Globals._miniDumpFile.ReadThreadInfoList();
            string threadIdInfo = "";
            string threadDumpFlags = "";
            string threadDumpError = "";
            string threadExitStatus = "";
            string threadCreateTime = "";
            string threadExitTime = "";
            string threadKernelTime = "";
            string threadUserTime = "";
            string threadStartAddress = "";
            string threadAffinity = "";

            string threadInfoArray = "";
            if (threadInfoData.Length == 0)
            {
                threadIdInfo = "No data found for the stream";
            }
            else
            {
                foreach (var thread in threadInfoData)
                {
                    threadIdInfo = "0x" + thread.ThreadId.ToString("x8") + " (" + thread.ThreadId + ")";
                    threadDumpFlags = thread.DumpFlags.ToString();
                    threadDumpError = "0x" + thread.DumpError.ToString("x8");
                    threadExitStatus = (thread.ExitStatus == MiniDumpThreadInfo.STILL_ACTIVE) ? "STILL_ACTIVE" : thread.ExitStatus.ToString();
                    threadCreateTime = thread.CreateTime.ToString();

                    if (thread.ExitTime == DateTime.MinValue)
                    {
                        threadExitTime = "";
                    }
                    else
                    {
                        threadExitTime = thread.ExitTime.ToString();
                    }
                    threadKernelTime = FormattedTimeSpan(thread.KernelTime);
                    threadUserTime = FormattedTimeSpan(thread.UserTime);
                    threadStartAddress = "0x" + thread.StartAddress.ToString("x8");
                    threadAffinity = thread.Affinity.ToString();

                    threadInfoArray += $"ThreadID: 0x{thread.ThreadId.ToString("x8")} ({thread.ThreadId})\n" +
                        $"Dump Flags: {thread.DumpFlags.ToString()}\n" +
                        $"Dump Error: 0x{thread.DumpError.ToString("x8")}\n" +
                        $"Exit Status: {threadExitStatus}\n" +
                        $"Creation Time: {thread.CreateTime}\n" +
                        $"Exit Time: {threadExitTime}\n" +
                        $"Kernel Time: {FormattedTimeSpan(thread.KernelTime)}\n" +
                        $"User Time: {FormattedTimeSpan(thread.UserTime)}\n" +
                        $"Start Address: 0x{thread.StartAddress.ToString("x8")}\n" +
                        $"Affinity: {thread.Affinity}\n\n";
                }
                ThreadInfoText.Text = threadInfoArray;
            }


            MiniDumpThreadNamesStream threadNamesStream = Globals._miniDumpFile.ReadThreadNamesStream();

            if (threadNamesStream.Entries.Count == 0)
            {
                ThreadNameText.Text = "No data found for stream";
            }
            else
            {
                foreach (var thread in threadNamesStream.Entries)
                {
                    ThreadNameText.Text += $"{thread.Name}\n";
                }

            }

        }


        /// <summary>
        /// 
        /// </summary>
        public void DumpMemoryData()
        {
            MiniDumpMemoryDescriptor[] memoryData = Globals._miniDumpFile.ReadMemoryList();
            string MemoryData = "Memory Start | Memory End | Data Size\n\n";
            if (memoryData.Length == 0)
            {
                Memory32Text.Text = "No data found for stream";
            }
            else
            {
                foreach (var mem in memoryData)
                {
                    MemoryData += $" {mem.StartOfMemoryRangeFormatted}     {mem.EndOfMemoryRangeFormatted}     {mem.Memory.DataSizePretty}\n";
                }
                Memory32Text.Text = MemoryData;
            }

            MiniDumpMemory64Stream memory64Data = Globals._miniDumpFile.ReadMemory64List();

            string mem64data = "Memory Start | Memory End | Data Size\n\n";
            if (memory64Data.MemoryRanges.Length == 0)
            {
                Memory64Text.Text = "No data found for stream";
            }
            else
            {
                foreach (MiniDumpMemoryDescriptor64 mem64 in memory64Data.MemoryRanges)
                {
                    mem64data += $" {mem64.StartOfMemoryRangeFormatted}     {mem64.EndOfMemoryRangeFormatted}     {mem64.DataSizePretty}\n";
                }
                Memory64Text.Text = mem64data;
            }

            MiniDumpMemoryInfoStream memoryInfo = Globals._miniDumpFile.ReadMemoryInfoList();



            string MemoryInfoData = "";
            foreach (MiniDumpMemoryInfo memInfo in memoryInfo.Entries)
            {
                if (memInfo.State == MemoryState.MEM_FREE)
                {


                    MemoryInfoData += $"Base Address: {memInfo.BaseAddress}\n" +
                        $"Region Size: {memInfo.RegionSizePretty}\n" +
                        $"State: {memInfo.State}\n";
                }
                else
                {

                    // Some regions don't have any Protection information
                    //memoryInfoProtect = ((int)memInfo.Protect == 0) ? string.Empty : memInfo.Protect.ToString();
                    // memoryInfoType = memInfo.Type.ToString();

                    MemoryInfoData += $"Base Address: {Formatters.FormatAsMemoryAddress(memInfo.BaseAddress)}\n" +
                        $"Allocation Base: {Formatters.FormatAsMemoryAddress(memInfo.AllocationBase)}\n" +
                        $"Allocation Protection: {memInfo.AllocationProtect}\n" +
                        $"Region Size: {memInfo.RegionSizePretty}\n" +
                        $"State: {memInfo.State}\n" +
                        $"Protection: {memInfo.Protect.ToString()}\n" +
                        $"Type: {memInfo.Type}\n\n";
                }
            }
            MemoryInfoText.Text = MemoryInfoData;

        }
       
        /// <summary>
        /// 
        /// </summary>
        public void DumpSystemData()
        {
            MiniDumpSystemInfoStream systemInfo = Globals._miniDumpFile.ReadSystemInfo();
            string systemInfoProcessorArchitecture = systemInfo.ProcessorArchitecture.ToString();
            string systemInfoProcessorLevel = "";
            string systemInfoProcessorRevision = "";
            string systemInfoNumberOfProcessors = "";
            string systemInfoOperatingSystem = "";
            string systemInfoProductType = "";
            string systemInfoMajorVersion = "";
            string systemInfoMinorVersion = "";
            string systemInfoBuildNumber = "";
            string systemInfoPlatformId = "";
            string systemInfoCSDVersion = "";
            string systemInfoSuiteMask = "";
            string systemInfoHasSuiteBackOffice = "";
            string systemInfoHasSuiteBlade = "";
            string systemInfoHasSuiteComputeServer = "";
            string systemInfoHasSuiteDataCenter = "";
            string systemInfoHasSuiteEnterprise = "";
            string systemInfoHasSuiteEmbeddedNt = "";
            string systemInfoHasSuitePersonal = "";
            string systemInfoHasSuiteSingleUserTerminalServices = "";
            string systemInfoHasSuiteSmallBusiness = "";
            string systemInfoHasSuiteSmallBusinessRestricted = "";
            string systemInfoHasSuiteStorageServer = "";
            string systemInfoHasSuiteTerminal = "";

            string SystemInfoArray = "";

            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM)
            {
                SystemInfoArray += "Processor Architecture: ARM\n";
            }
            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL)
            {
                SystemInfoArray += "Processor Architecture: I386\n";
            }
            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64)
            {
                SystemInfoArray += "Processor Architecture: AMD64\n";
            }
            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_IA64)
            {
                SystemInfoArray += "Processor Architecture: IA-64\n";
            }
            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_UNKNOWN)
            {
                SystemInfoArray += "Processor Architecture: Unknown\n";
            }


            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL)
            {
                switch (systemInfo.ProcessorLevel)
                {
                    case 3: systemInfoProcessorLevel = $"Intel 80386"; break;
                    case 4: systemInfoProcessorLevel = $"Intel 80486"; break;
                    case 5: systemInfoProcessorLevel = $"Intel Pentium"; break;
                    case 6: systemInfoProcessorLevel = $"Intel Pentium Pro or Pentium II"; break;
                    default: systemInfoProcessorLevel = systemInfo.ProcessorLevel.ToString(); break;
                }
            }
            else
            {
                systemInfoProcessorLevel = systemInfo.ProcessorLevel.ToString();
            }

            systemInfoProcessorRevision = String.Format($"0x{0:x8} {systemInfo.ProcessorRevision}");
            systemInfoNumberOfProcessors = systemInfo.NumberOfProcessors.ToString();
            systemInfoOperatingSystem = systemInfo.OperatingSystemDescription;
            systemInfoProductType = systemInfo.ProductType.ToString();
            systemInfoMajorVersion = systemInfo.MajorVersion.ToString();
            systemInfoMinorVersion = systemInfo.MinorVersion.ToString();
            systemInfoBuildNumber = systemInfo.BuildNumber.ToString();
            systemInfoPlatformId = systemInfo.PlatformId.ToString();
            systemInfoCSDVersion = systemInfo.CSDVersion;
            systemInfoSuiteMask = systemInfo.SuiteMask.ToString();
            systemInfoHasSuiteBackOffice = systemInfo.HasSuiteBackOffice.ToString();
            systemInfoHasSuiteBlade = systemInfo.HasSuiteBlade.ToString();
            systemInfoHasSuiteComputeServer = systemInfo.HasSuiteComputeServer.ToString();
            systemInfoHasSuiteDataCenter = systemInfo.HasSuiteDataCenter.ToString();
            systemInfoHasSuiteEnterprise = systemInfo.HasSuiteEnterprise.ToString();
            systemInfoHasSuiteEmbeddedNt = systemInfo.HasSuiteEmbeddedNt.ToString();
            systemInfoHasSuitePersonal = systemInfo.HasSuitePersonal.ToString();
            systemInfoHasSuiteSingleUserTerminalServices = systemInfo.HasSuiteSingleUserTerminalServices.ToString();
            systemInfoHasSuiteSmallBusiness = systemInfo.HasSuiteSmallBusiness.ToString();
            systemInfoHasSuiteSmallBusinessRestricted = systemInfo.HasSuiteSmallBusinessRestricted.ToString();
            systemInfoHasSuiteStorageServer = systemInfo.HasSuiteStorageServer.ToString();
            systemInfoHasSuiteTerminal = systemInfo.HasSuiteTerminal.ToString();

            string cpufeat = "";
            if (systemInfo.ProcessorArchitecture == MiniDumpProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL)
            {
                string systemInfoVendorId = systemInfo.CpuInfoX86.VendorId;
                string systemInfoVersionInformation = String.Format($"0x{0:x8} {systemInfo.CpuInfoX86.VersionInformation}");
                string systemInfoFeatureInformation = String.Format($"0x{0:x8} ({1}) {systemInfo.CpuInfoX86.FeatureInformation}, {Convert.ToString(systemInfo.CpuInfoX86.FeatureInformation)}");

                if (systemInfo.CpuInfoX86.VendorId == "AuthenticAMD")
                {
                    cpufeat = String.Format($"0x{systemInfo.CpuInfoX86.AMDExtendedCpuFeatures}");
                }
                else
                {

                    foreach (var bit in systemInfo.CpuInfoOther.ProcessorFeatures)
                    {
                        cpufeat += $"0x{bit}\n";
                    }

                }
            }

            SystemInfoText.Text = $"Number of Processors: {systemInfo.NumberOfProcessors}\n" +
                $"Operating System: {systemInfo.OperatingSystemDescription}\n" +
                $"Product Type: {systemInfo.ProductType}\n" +
                $"Versioning: {systemInfo.MajorVersion}.{systemInfo.MinorVersion}.{systemInfo.BuildNumber}\n" +
                $"Platform ID: {systemInfo.PlatformId}\n" +
                $"CSD Version: {systemInfo.CSDVersion}\n" +
                $"Processor Features: {cpufeat}";

            MiniDumpSystemMemoryInfo systemMemoryInfo = Globals._miniDumpFile.ReadSystemMemoryInfo();


            string SystemBasicInfo = "[Basic Info]\n";
            if (systemMemoryInfo == null)
            {
                SystemMemText.Text += "Stream not found";
                //return;
            }
            else
            {
                SystemMemText.Text += $"[System Memory Info]\nRevision: {systemMemoryInfo.Revision}\n";
                SystemMemText.Text += $"Flags: {Formatters.FormatAsHex(systemMemoryInfo.Flags)}\n\n";

                // Unofficial field descriptions
                // http://masm32.com/board/index.php?topic=3402.0

                SystemBasicInfo += $"Timer Resolution: {systemMemoryInfo.SystemBasicInformation.TimerResolution} \n";
                SystemBasicInfo += $"Page Size: {systemMemoryInfo.SystemBasicInformation.PageSize} \n";
                SystemBasicInfo += $"Number Of Physical Pages: {systemMemoryInfo.SystemBasicInformation.NumberOfPhysicalPages.ToString("N0")} \n";
                SystemBasicInfo += $"Lowest Physical Page Number: {systemMemoryInfo.SystemBasicInformation.LowestPhysicalPageNumber.ToString("N0")} \n";
                SystemBasicInfo += $"Highest Physical Page Number: {systemMemoryInfo.SystemBasicInformation.HighestPhysicalPageNumber.ToString("N0")} \n";
                SystemBasicInfo += $"Allocation Granularity: {systemMemoryInfo.SystemBasicInformation.AllocationGranularity} \n";
                SystemBasicInfo += $"Minimum User Mode Address: {Formatters.FormatAsMemoryAddress(systemMemoryInfo.SystemBasicInformation.MinimumUserModeAddress)} \n";
                SystemBasicInfo += $"Maximum User Mode Address: {Formatters.FormatAsMemoryAddress(systemMemoryInfo.SystemBasicInformation.MaximumUserModeAddress)} \n";
                SystemBasicInfo += $"Active Processors Affinity Mask: {Formatters.FormatAsHex(systemMemoryInfo.SystemBasicInformation.ActiveProcessorsAffinityMask)} \n";
                SystemBasicInfo += $"Number Of Processors: {systemMemoryInfo.SystemBasicInformation.NumberOfProcessors} \n\n";
            }

            string FileCacheInfo = "[File Cache Info]\n";

            FileCacheInfo += String.Format($"Current Size: {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.CurrentSize)} ({systemMemoryInfo.SystemFileCacheInformation.CurrentSize} bytes)\n");
            FileCacheInfo += String.Format($"Peak Size: {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.PeakSize)} ({systemMemoryInfo.SystemFileCacheInformation.PeakSize} bytes)\n");
            FileCacheInfo += $"Page Fault Count: {systemMemoryInfo.SystemFileCacheInformation.PageFaultCount.ToString("N0")} \n";
            FileCacheInfo += String.Format($"Min Working Set: {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.MinimumWorkingSet)} ({systemMemoryInfo.SystemFileCacheInformation.MinimumWorkingSet} bytes)\n");
            FileCacheInfo += String.Format($"Max Working Set: {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.MaximumWorkingSet)} ({systemMemoryInfo.SystemFileCacheInformation.MaximumWorkingSet} bytes)\n");
            FileCacheInfo += String.Format($"Current Size: {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.CurrentSizeIncludingTransitionInPages)} ({systemMemoryInfo.SystemFileCacheInformation.CurrentSizeIncludingTransitionInPages} bytes)\n");

            FileCacheInfo += String.Format($"Peak Size (TransitionsInPages): {Formatters.FormatAsSizeString(systemMemoryInfo.SystemFileCacheInformation.PeakSizeIncludingTransitionInPages)} ({systemMemoryInfo.SystemFileCacheInformation.PeakSizeIncludingTransitionInPages} bytes)\n");
            FileCacheInfo += $"Transition Repurpose Count: {systemMemoryInfo.SystemFileCacheInformation.TransitionRePurposeCount.ToString("N0")} \n";
            FileCacheInfo += $"Flags: {Formatters.FormatAsHex(systemMemoryInfo.SystemFileCacheInformation.Flags)} \n\n";

            string SysBasicPerformanceInfo = "[System Performance Info]\n";
            SysBasicPerformanceInfo += $"Available Pages: {systemMemoryInfo.SystemBasicPerformanceInformation.AvailablePages.ToString("N0")}\n";
            SysBasicPerformanceInfo += $"Committed Pages: {systemMemoryInfo.SystemBasicPerformanceInformation.CommittedPages.ToString("N0")}\n";
            SysBasicPerformanceInfo += $"Commit Limit: {systemMemoryInfo.SystemBasicPerformanceInformation.CommitLimit.ToString("N0")}\n";
            SysBasicPerformanceInfo += $"Peak Commitment: {systemMemoryInfo.SystemBasicPerformanceInformation.PeakCommitment.ToString("N0")}\n\n";



            string SysPerformanceInfo = "[Extended Performance Info]\n";
            SysPerformanceInfo += $"Idle ProcessTime: {systemMemoryInfo.SystemPerformanceInformation.IdleProcessTime}\n";
            SysPerformanceInfo += $"IORead Transfer Count: {systemMemoryInfo.SystemPerformanceInformation.IoReadTransferCount.ToString("N0")}\n";
            SysPerformanceInfo += $"IOWrite Transfer Count: {systemMemoryInfo.SystemPerformanceInformation.IoWriteTransferCount.ToString("N0")}\n";
            SysPerformanceInfo += $"IOOther Transfer Count: {systemMemoryInfo.SystemPerformanceInformation.IoOtherTransferCount.ToString("N0")}\n";
            SysPerformanceInfo += $"IORead OperationC ount: {systemMemoryInfo.SystemPerformanceInformation.IoReadOperationCount.ToString("N0")}\n";
            SysPerformanceInfo += $"IOWrite Operation Count: {systemMemoryInfo.SystemPerformanceInformation.IoWriteOperationCount.ToString("N0")}\n";
            SysPerformanceInfo += $"IOOther Operation Count: {systemMemoryInfo.SystemPerformanceInformation.IoOtherOperationCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Available Pages: {systemMemoryInfo.SystemPerformanceInformation.AvailablePages.ToString("N0")}\n";
            SysPerformanceInfo += $"Committed Pages: {systemMemoryInfo.SystemPerformanceInformation.CommittedPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Commit Limit: {systemMemoryInfo.SystemPerformanceInformation.CommitLimit.ToString("N0")}\n";
            SysPerformanceInfo += $"Peak Commitment: {systemMemoryInfo.SystemPerformanceInformation.PeakCommitment.ToString("N0")}\n";
            SysPerformanceInfo += $"Page Fault Count: {systemMemoryInfo.SystemPerformanceInformation.PageFaultCount.ToString("N0")}\n";
            SysPerformanceInfo += $"CopyOnWrite Count: {systemMemoryInfo.SystemPerformanceInformation.CopyOnWriteCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Transition Count: {systemMemoryInfo.SystemPerformanceInformation.TransitionCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Cache Transition Count: {systemMemoryInfo.SystemPerformanceInformation.CacheTransitionCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Demand Zero Count: {systemMemoryInfo.SystemPerformanceInformation.DemandZeroCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Page Read Count: {systemMemoryInfo.SystemPerformanceInformation.PageReadCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Page Read IO Count: {systemMemoryInfo.SystemPerformanceInformation.PageReadIoCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Cache Read Count: {systemMemoryInfo.SystemPerformanceInformation.CacheReadCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Cache IO Count: {systemMemoryInfo.SystemPerformanceInformation.CacheIoCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Dirty Pages Write Count: {systemMemoryInfo.SystemPerformanceInformation.DirtyPagesWriteCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Dirty Write IO Count: {systemMemoryInfo.SystemPerformanceInformation.DirtyWriteIoCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Mapped Pages Write Count: {systemMemoryInfo.SystemPerformanceInformation.MappedPagesWriteCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Mapped Write IO Count: {systemMemoryInfo.SystemPerformanceInformation.MappedWriteIoCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Paged Pool Pages: {systemMemoryInfo.SystemPerformanceInformation.PagedPoolPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Non-Paged Pool Pages: {systemMemoryInfo.SystemPerformanceInformation.NonPagedPoolPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Paged Pool Allocs: {systemMemoryInfo.SystemPerformanceInformation.PagedPoolAllocs.ToString("N0")}\n";
            SysPerformanceInfo += $"Paged Pool Frees: {systemMemoryInfo.SystemPerformanceInformation.PagedPoolFrees.ToString("N0")}\n";
            SysPerformanceInfo += $"Non-Paged Pool Allocs: {systemMemoryInfo.SystemPerformanceInformation.NonPagedPoolAllocs.ToString("N0")}\n";
            SysPerformanceInfo += $"Non-Paged Pool Frees: {systemMemoryInfo.SystemPerformanceInformation.NonPagedPoolFrees.ToString("N0")}\n";
            SysPerformanceInfo += $"Free System Ptes: {systemMemoryInfo.SystemPerformanceInformation.FreeSystemPtes.ToString("N0")}\n";
            SysPerformanceInfo += $"Resident System Code Page: {systemMemoryInfo.SystemPerformanceInformation.ResidentSystemCodePage.ToString("N0")}\n";
            SysPerformanceInfo += $"Total System Driver Pages: {systemMemoryInfo.SystemPerformanceInformation.TotalSystemDriverPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Total System Code Pages: {systemMemoryInfo.SystemPerformanceInformation.TotalSystemCodePages.ToString("N0")}\n";
            SysPerformanceInfo += $"Non-Paged Pool Lookaside Hits: {systemMemoryInfo.SystemPerformanceInformation.NonPagedPoolLookasideHits.ToString("N0")}\n";
            SysPerformanceInfo += $"Paged Pool Lookaside Hits: {systemMemoryInfo.SystemPerformanceInformation.PagedPoolLookasideHits.ToString("N0")}\n";
            SysPerformanceInfo += $"Available Paged Pool Pages: {systemMemoryInfo.SystemPerformanceInformation.AvailablePagedPoolPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Resident System Cache Page: {systemMemoryInfo.SystemPerformanceInformation.ResidentSystemCachePage.ToString("N0")}\n";
            SysPerformanceInfo += $"Resident Paged Pool Page: {systemMemoryInfo.SystemPerformanceInformation.ResidentPagedPoolPage.ToString("N0")}\n";
            SysPerformanceInfo += $"Resident System Driver Page: {systemMemoryInfo.SystemPerformanceInformation.ResidentSystemDriverPage.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Read No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcFastReadNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Read Wait: {systemMemoryInfo.SystemPerformanceInformation.CcFastReadWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Read Resource Miss: {systemMemoryInfo.SystemPerformanceInformation.CcFastReadResourceMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Read Not Possible: {systemMemoryInfo.SystemPerformanceInformation.CcFastReadNotPossible.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Mdl Read No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcFastMdlReadNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Mdl Read Wait: {systemMemoryInfo.SystemPerformanceInformation.CcFastMdlReadWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Mdl Read Resource Miss: {systemMemoryInfo.SystemPerformanceInformation.CcFastMdlReadResourceMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Fast Mdl Read Not Possible: {systemMemoryInfo.SystemPerformanceInformation.CcFastMdlReadNotPossible.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Map Data No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcMapDataNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Map Data Wait: {systemMemoryInfo.SystemPerformanceInformation.CcMapDataWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Map Data No Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcMapDataNoWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Map Data Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcMapDataWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Pin Mapped Data Count: {systemMemoryInfo.SystemPerformanceInformation.CcPinMappedDataCount.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Pin Read No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcPinReadNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Pin Read Wait: {systemMemoryInfo.SystemPerformanceInformation.CcPinReadWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Pin Read No Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcPinReadNoWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Pin Read Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcPinReadWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Copy Read No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcCopyReadNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Copy Read Wait: {systemMemoryInfo.SystemPerformanceInformation.CcCopyReadWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Copy Read No Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcCopyReadNoWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Copy Read Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcCopyReadWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Mdl Read No Wait: {systemMemoryInfo.SystemPerformanceInformation.CcMdlReadNoWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Mdl Read Wait: {systemMemoryInfo.SystemPerformanceInformation.CcMdlReadWait.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Mdl Read No Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcMdlReadNoWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Mdl Read Wait Miss: {systemMemoryInfo.SystemPerformanceInformation.CcMdlReadWaitMiss.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Read Ahead IOs: {systemMemoryInfo.SystemPerformanceInformation.CcReadAheadIos.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Lazy Write IOs: {systemMemoryInfo.SystemPerformanceInformation.CcLazyWriteIos.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Lazy Write Pages: {systemMemoryInfo.SystemPerformanceInformation.CcLazyWritePages.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Data Flushes: {systemMemoryInfo.SystemPerformanceInformation.CcDataFlushes.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Data Pages: {systemMemoryInfo.SystemPerformanceInformation.CcDataPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Context Switches: {systemMemoryInfo.SystemPerformanceInformation.ContextSwitches.ToString("N0")}\n";
            SysPerformanceInfo += $"First Level Tb Fills: {systemMemoryInfo.SystemPerformanceInformation.FirstLevelTbFills.ToString("N0")}\n";
            SysPerformanceInfo += $"Second Level Tb Fills: {systemMemoryInfo.SystemPerformanceInformation.SecondLevelTbFills.ToString("N0")}\n";
            SysPerformanceInfo += $"System Calls: {systemMemoryInfo.SystemPerformanceInformation.SystemCalls.ToString("N0")}\n";

            SysPerformanceInfo += $"Cc Total Dirty Pages: {systemMemoryInfo.SystemPerformanceInformation.CcTotalDirtyPages.ToString("N0")}\n";
            SysPerformanceInfo += $"Cc Dirty Page Threshold: {systemMemoryInfo.SystemPerformanceInformation.CcDirtyPageThreshold.ToString("N0")}\n";

            SysPerformanceInfo += $"Resident Available Pages: {systemMemoryInfo.SystemPerformanceInformation.ResidentAvailablePages.ToString("N0")}\n";
            SysPerformanceInfo += $"Shared Committed Pages: {systemMemoryInfo.SystemPerformanceInformation.SharedCommittedPages.ToString("N0")}\n";

            SystemMemText.Text += $"{SystemBasicInfo}{FileCacheInfo}{SysBasicPerformanceInfo}{SysPerformanceInfo}";
        }
       
        /// <summary>
        /// 
        /// </summary>
        public void DumpMiscData()
        {
            MiniDumpMiscInfo miscInfo = Globals._miniDumpFile.ReadMiscInfo();
            //miscInfo.SizeOfInfo
            Globals.miscData1 += "[Misc Info]\n";
            if (miscInfo.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC1_PROCESS_ID))
            {
                Globals.miscData1 += $"Process ID: {miscInfo.ProcessId.ToString()}\n";
            }
            else
            {
                // string miscInfoMINIDUMP_MISC1_PROCESS_ID = "Not available";
            }

            if (miscInfo.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC1_PROCESS_TIMES))
            {
                Globals.miscData1 += $"Process Creation Time: {miscInfo.ProcessCreateTime.ToString()}\n";
                Globals.miscData1 += $"Process User Time: {miscInfo.ProcessUserTime.ToString()}\n";
                Globals.miscData1 += $"Process Kernel Time: {miscInfo.ProcessKernelTime.ToString()}\n\n";
            }
            else
            {
                // string miscInfoMINIDUMP_MISC1_PROCESS_TIMES = "Not available";
            }

            //
            //
            // Check what other level of information is available
            if (miscInfo.MiscInfoLevel == MiniDumpMiscInfoLevel.MiscInfo4)
            {
                //
                //AddMiscInfo2Data((MiniDumpMiscInfo2)miscInfo);
                MiniDumpMiscInfo2 miscInfo2 = (MiniDumpMiscInfo2)miscInfo;
                if (miscInfo.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC1_PROCESSOR_POWER_INFO))
                {
                    Globals.miscData2 += $"CPU Max Mhz: {miscInfo2.ProcessorMaxMhz.ToString()}\n";
                    Globals.miscData2 += $"CPU Current Mhz: {miscInfo2.ProcessorCurrentMhz.ToString()}\n";
                    Globals.miscData2 += $"CPU Mhz Limit: {miscInfo2.ProcessorMhzLimit.ToString()}\n";
                    Globals.miscData2 += $"CPU Max Idle State: {miscInfo2.ProcessorMaxIdleState.ToString()}\n";
                    Globals.miscData2 += $"CPU Current Idle State: {miscInfo2.ProcessorCurrentIdleState.ToString()}\n\n";

                }
                else
                {
                    //Globals.miscData2 += "Not available";
                }



                //
                //AddMiscInfo3Data((MiniDumpMiscInfo3)miscInfo);
                MiniDumpMiscInfo3 miscInfo3 = (MiniDumpMiscInfo3)miscInfo;

                if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROCESS_INTEGRITY))
                {
                    Globals.miscData3 += $"Process Integrity Level: {miscInfo3.ProcessIntegrityLevel.ToString()}\n";

                }
                else
                {
                    //string miscInfoMINIDUMP_MISC3_PROCESS_INTEGRITY = "Not available";

                }

                // MINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS isn't actually documented, so I'm assuming it covers ProcessExecuteFlags
                if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS))
                {
                    Globals.miscData3 += $"Process Execute Flags: {miscInfo3.ProcessExecuteFlags.ToString()}\n";
                }
                else
                {
                    //string miscInfoMINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS = "Not available";
                }

                // MINIDUMP_MISC3_PROTECTED_PROCESS isn't actually documented, so I'm assuming it covers ProtectedProcess
                if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROTECTED_PROCESS))
                {
                    Globals.miscData3 += $"Protected Process: {miscInfo3.ProtectedProcess.ToString()}\n\n";
                }
                else
                {
                    //string miscInfoMINIDUMP_MISC3_PROTECTED_PROCESS = "Not available";
                }

                // MINIDUMP_MISC3_TIMEZONE isn't actually documented, so I'm assuming it covers TimeZoneId & TimeZoneId
                if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_TIMEZONE))
                {

                    Globals.miscData3 += $"Time Zone: {miscInfo3.TimeZone.StandardName}\n";
                    Globals.miscData3 += $"Time/Date: {miscInfo3.TimeZone.StandardDate.ToString()}\n";

                }
                else
                {
                    // string miscInfoMINIDUMP_MISC3_TIMEZONE = "Not available";
                }
                // Globals.miscData3 += "";

                //
                //
                //AddMiscInfo4Data((MiniDumpMiscInfo4)miscInfo);
                MiniDumpMiscInfo4 miscInfo4 = (MiniDumpMiscInfo4)miscInfo;
                // MINIDUMP_MISC4_BUILDSTRING isn't actually documented, so I'm assuming it covers BuildString & DbgBldStr
                if (miscInfo4.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC4_BUILDSTRING))
                {
                    Globals.miscData4 += $"Build String: {miscInfo4.BuildString}\n";
                    Globals.miscData4 += $"Debug String: {miscInfo4.DbgBldStr}\n";
                }
                else
                {
                    //string miscInfoMINIDUMP_MISC4_BUILDSTRING = "Not available";
                }

            }
            else if (miscInfo.MiscInfoLevel == MiniDumpMiscInfoLevel.MiscInfo3)
            {
                //
                //
                // Check what other level of information is available
                if (miscInfo.MiscInfoLevel == MiniDumpMiscInfoLevel.MiscInfo4)
                {
                    //
                    //AddMiscInfo2Data((MiniDumpMiscInfo2)miscInfo);
                    MiniDumpMiscInfo2 miscInfo2 = (MiniDumpMiscInfo2)miscInfo;
                    if (miscInfo.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC1_PROCESSOR_POWER_INFO))
                    {
                        Globals.miscData2 += $"CPU Max Mhz: {miscInfo2.ProcessorMaxMhz.ToString()}\n";
                        Globals.miscData2 += $"CPU Current Mhz: {miscInfo2.ProcessorCurrentMhz.ToString()}\n";
                        Globals.miscData2 += $"CPU Mhz Limit: {miscInfo2.ProcessorMhzLimit.ToString()}\n";
                        Globals.miscData2 += $"CPU Max Idle State: {miscInfo2.ProcessorMaxIdleState.ToString()}\n";
                        Globals.miscData2 += $"CPU Current Idle State: {miscInfo2.ProcessorCurrentIdleState.ToString()}\n\n";

                    }
                    else
                    {
                        //Globals.miscData2 += "Not available";
                    }



                    //
                    //AddMiscInfo3Data((MiniDumpMiscInfo3)miscInfo);
                    MiniDumpMiscInfo3 miscInfo3 = (MiniDumpMiscInfo3)miscInfo;

                    if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROCESS_INTEGRITY))
                    {
                        Globals.miscData3 += $"Process Integrity Level: {miscInfo3.ProcessIntegrityLevel.ToString()}\n";

                    }
                    else
                    {
                        //string miscInfoMINIDUMP_MISC3_PROCESS_INTEGRITY = "Not available";

                    }

                    // MINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS isn't actually documented, so I'm assuming it covers ProcessExecuteFlags
                    if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS))
                    {
                        Globals.miscData3 += $"Process Execute Flags: {miscInfo3.ProcessExecuteFlags.ToString()}\n";
                    }
                    else
                    {
                        //string miscInfoMINIDUMP_MISC3_PROCESS_EXECUTE_FLAGS = "Not available";
                    }

                    // MINIDUMP_MISC3_PROTECTED_PROCESS isn't actually documented, so I'm assuming it covers ProtectedProcess
                    if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_PROTECTED_PROCESS))
                    {
                        Globals.miscData3 += $"Protected Process: {miscInfo3.ProtectedProcess.ToString()}\n\n";
                    }
                    else
                    {
                        //string miscInfoMINIDUMP_MISC3_PROTECTED_PROCESS = "Not available";
                    }

                    // MINIDUMP_MISC3_TIMEZONE isn't actually documented, so I'm assuming it covers TimeZoneId & TimeZoneId
                    if (miscInfo3.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC3_TIMEZONE))
                    {

                        Globals.miscData3 += $"Time Zone: {miscInfo3.TimeZone.StandardName}\n";
                        Globals.miscData3 += $"Time/Date: {miscInfo3.TimeZone.StandardDate.ToString()}\n";

                    }
                    else
                    {
                        // string miscInfoMINIDUMP_MISC3_TIMEZONE = "Not available";
                    }
                }
                else if (miscInfo.MiscInfoLevel == MiniDumpMiscInfoLevel.MiscInfo2)
                {
                    //
                    //AddMiscInfo2Data((MiniDumpMiscInfo2)miscInfo);
                    MiniDumpMiscInfo2 miscInfo2 = (MiniDumpMiscInfo2)miscInfo;
                    if (miscInfo.Flags1.HasFlag(MiscInfoFlags.MINIDUMP_MISC1_PROCESSOR_POWER_INFO))
                    {
                        Globals.miscData2 += $"CPU Max Mhz: {miscInfo2.ProcessorMaxMhz.ToString()}\n";
                        Globals.miscData2 += $"CPU Current Mhz: {miscInfo2.ProcessorCurrentMhz.ToString()}\n";
                        Globals.miscData2 += $"CPU Mhz Limit: {miscInfo2.ProcessorMhzLimit.ToString()}\n";
                        Globals.miscData2 += $"CPU Max Idle State: {miscInfo2.ProcessorMaxIdleState.ToString()}\n";
                        Globals.miscData2 += $"CPU Current Idle State: {miscInfo2.ProcessorCurrentIdleState.ToString()}\n\n";

                    }
                    else
                    {
                        //Globals.miscData2 += "Not available";
                    }
                }
            }

            MiscInfoText.Text = $"{Globals.miscData1}{Globals.miscData2}{Globals.miscData3}{Globals.miscData4}";
        }

        /// <summary>
        /// 
        /// </summary>
        public void DumpCommentData()
        {
            MiniDumpCommentStreamW commentWStream = Globals._miniDumpFile.ReadCommentStreamW();

            if (commentWStream.Comment == null)
            {

                CommentWBox.Text = "No data found for stream";
            }
            else
            {
                Globals.minidumpCommentW = commentWStream.Comment;
                CommentWBox.Text = Globals.minidumpCommentW;
            }



            MiniDumpCommentStreamA commentAStream = Globals._miniDumpFile.ReadCommentStreamA();

            if (commentAStream.Comment == null)
            {
                Globals.minidumpCommentA = "No data found for stream";
                CommentABox.Text = Globals.minidumpCommentA;

            }
            else
            {

                CommentWBox.Text = commentAStream.Comment;
            }


            Output.Text = $"File Path: {Globals.storageFile.Path}\n" +
                           $"Date Created: {Globals.storageFile.DateCreated}\n" +
                           $"Content Type: {Globals.storageFile.ContentType}\n" +
                           $"Attributes: {Globals.storageFile.Attributes}";





        }

        /// <summary>
        /// 
        /// </summary>
        public void DumpOtherData()
        {

        }








    }
}
