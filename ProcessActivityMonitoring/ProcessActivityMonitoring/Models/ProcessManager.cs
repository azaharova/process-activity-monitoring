using ProcessActivityMonitoring.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Runtime.InteropServices;
using ComType = System.Runtime.InteropServices.ComTypes;
using System.Collections;

namespace ProcessActivityMonitoring.Models
{
    public class ProcessManager : NotifyPropertyChangedHelper
    {
        #region Fields

        ArrayList ProcessDataList = new ArrayList();
        HashSet<uint> IdList = new HashSet<uint>();
        ObservableCollection<ProcessData> processes;

        #endregion

        #region Properties

        public ObservableCollection<ProcessData> Processes
        {
            get { return processes; }
            set
            {
                processes = value;
                NotifyPropertyChanged("Processes");
            }
        }

        #endregion

        #region .ctors

        public ProcessManager()
        {
            Processes = new ObservableCollection<ProcessData>();
            refresh();
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        #endregion

        #region Methods

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            refresh();
        }

        private void refresh()
        {
            ProcessEntry32 processInfo = new ProcessEntry32();
            ProcessTimes processTimes = new ProcessTimes();
            IntPtr processList, processHandle = ProcessCPU.PROCESS_HANDLE_ERROR;
            ProcessData currentProcessData;
            int index, total = 0;
            bool noError;
            processList = ProcessCPU.CreateToolhelp32Snapshot(ProcessCPU.TH32CS_SNAPPROCESS, 0);
            if (processList == ProcessCPU.PROCESS_LIST_ERROR)
                return;
            processInfo.Size = ProcessCPU.PROCESS_ENTRY_32_SIZE;
            noError = ProcessCPU.Process32First(processList, ref processInfo);
            IdList.Clear();
            while (noError)
                try
                {
                    processHandle = ProcessCPU.OpenProcess(ProcessCPU.PROCESS_ALL_ACCESS, false, processInfo.ID);
                    ProcessCPU.GetProcessTimes(
                        processHandle,
                        out processTimes.RawCreationTime,
                        out processTimes.RawExitTime,
                        out processTimes.RawKernelTime,
                        out processTimes.RawUserTime);

                    processTimes.ConvertTime();
                    currentProcessData = processExists(processInfo.ID);
                    IdList.Add(processInfo.ID);
                    if (currentProcessData == null)
                    {
                        index = ProcessDataList.Add(new ProcessData(
                            processInfo.ID,
                            processInfo.ExeFilename,
                            processTimes.UserTime.Ticks,
                            processTimes.KernelTime.Ticks));
                        if (processInfo.ID != 0)
                            AddProcess((ProcessData)ProcessDataList[index]);
                    }
                    else
                        total += currentProcessData.UpdateCpuUsage(
                                    processTimes.UserTime.Ticks,
                                    processTimes.KernelTime.Ticks);
                }
                finally
                {
                    if (processHandle != ProcessCPU.PROCESS_HANDLE_ERROR)
                        ProcessCPU.CloseHandle(processHandle);
                    noError = ProcessCPU.Process32Next(processList, ref processInfo);
                }

            ProcessCPU.CloseHandle(processList);
            index = 0;
            while (index < ProcessDataList.Count)
            {
                ProcessData TempProcess = (ProcessData)ProcessDataList[index];
                if (IdList.Contains(TempProcess.Id))
                    index++;
                else
                {
                    Processes.Remove(TempProcess);
                    ProcessDataList.RemoveAt(index);
                }
            }
            NotifyPropertyChanged("Processes");
        }

        private ProcessData processExists(uint ID)
        {
            foreach (ProcessData TempProcess in ProcessDataList)
                if (TempProcess.Id == ID)
                    return TempProcess;

            return null;
        }

        private void AddProcess(ProcessData newProcess)
        {
            Processes.Add(newProcess);
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        [MarshalAs(UnmanagedType.U2)]
        public short Year;
        [MarshalAs(UnmanagedType.U2)]
        public short Month;
        [MarshalAs(UnmanagedType.U2)]
        public short DayOfWeek;
        [MarshalAs(UnmanagedType.U2)]
        public short Day;
        [MarshalAs(UnmanagedType.U2)]
        public short Hour;
        [MarshalAs(UnmanagedType.U2)]
        public short Minute;
        [MarshalAs(UnmanagedType.U2)]
        public short Second;
        [MarshalAs(UnmanagedType.U2)]
        public short Milliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessEntry32
    {
        public uint Size;
        public uint Usage;
        public uint ID;
        public IntPtr DefaultHeapID;
        public uint ModuleID;
        public uint Threads;
        public uint ParentProcessID;
        public int PriorityClassBase;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ExeFilename;
    };

    public struct ProcessTimes
    {
        public DateTime CreationTime, ExitTime, KernelTime, UserTime;
        public ComType.FILETIME RawCreationTime, RawExitTime, RawKernelTime, RawUserTime;

        public void ConvertTime()
        {
            CreationTime = FiletimeToDateTime(RawCreationTime);
            ExitTime = FiletimeToDateTime(RawExitTime);
            KernelTime = FiletimeToDateTime(RawKernelTime);
            UserTime = FiletimeToDateTime(RawUserTime);
        }

        private DateTime FiletimeToDateTime(ComType.FILETIME FileTime)
        {
            try
            {
                if (FileTime.dwLowDateTime < 0) FileTime.dwLowDateTime = 0;
                if (FileTime.dwHighDateTime < 0) FileTime.dwHighDateTime = 0;

                long RawFileTime = (((long)FileTime.dwHighDateTime) << 32) + FileTime.dwLowDateTime;
                return DateTime.FromFileTimeUtc(RawFileTime);
            }
            catch { return new DateTime(); }
        }
    };

    class ProcessCPU
    {
        // gets a process list pointer
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint Flags, uint ProcessID);

        // gets the first process in the process list
        [DllImport("KERNEL32.DLL")]
        public static extern bool Process32First(IntPtr Handle, ref ProcessEntry32 ProcessInfo);

        // gets the next process in the process list
        [DllImport("KERNEL32.DLL")]
        public static extern bool Process32Next(IntPtr Handle, ref ProcessEntry32 ProcessInfo);

        // closes handles
        [DllImport("KERNEL32.DLL")]
        public static extern bool CloseHandle(IntPtr Handle);

        // gets the process handle
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            uint DesiredAccess,
            bool InheritHandle,
            uint ProcessId);

        // gets the process creation, exit, kernel and user time 
        [DllImport("kernel32.dll")]
        public static extern bool GetProcessTimes(
            IntPtr ProcessHandle,
            out ComType.FILETIME CreationTime,
            out ComType.FILETIME ExitTime,
            out ComType.FILETIME KernelTime,
            out ComType.FILETIME UserTime);

        // some consts will need later
        public const int PROCESS_ENTRY_32_SIZE = 296;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static readonly IntPtr PROCESS_LIST_ERROR = new IntPtr(-1);
        public static readonly IntPtr PROCESS_HANDLE_ERROR = new IntPtr(-1);
    }

    public class ProcessData : NotifyPropertyChangedHelper
    {
        private long OldUserTime;
        private long OldKernelTime;
        private DateTime OldUpdate;
        private uint id;
        private string processName;
        private string cpuUsage;

        public uint Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string ProcessName
        {
            get { return processName; }
            set
            {
                processName = value;
                NotifyPropertyChanged("ProcessName");
            }
        }
        public string CpuUsage
        {
            get { return cpuUsage; }
            set
            {
                cpuUsage = value;
                NotifyPropertyChanged("CpuUsage");
            }
        }

        public int Index { get; set; }

        public ProcessData(uint Id, string Name, long OldUserTime, long OldKernelTime)
        {
            this.Id = Id;
            this.ProcessName = Name;
            this.OldUserTime = OldUserTime;
            this.OldKernelTime = OldKernelTime;
            OldUpdate = DateTime.Now;
        }

        public int UpdateCpuUsage(long NewUserTime, long NewKernelTime)
        {
            long updateDelay;
            long userTime = NewUserTime - OldUserTime;
            long kernelTime = NewKernelTime - OldKernelTime;
            int rawUsage;
            if (DateTime.Now.Ticks == OldUpdate.Ticks)
                Thread.Sleep(100);
            updateDelay = DateTime.Now.Ticks - OldUpdate.Ticks;
            rawUsage = (int)(((userTime + kernelTime) * 100) / updateDelay);
            CpuUsage = rawUsage + "%";
            OldUserTime = NewUserTime;
            OldKernelTime = NewKernelTime;
            OldUpdate = DateTime.Now;
            return rawUsage;
        }

    }
}
