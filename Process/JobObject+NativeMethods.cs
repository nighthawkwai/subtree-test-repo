using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Services.Core.Process
{
    public partial class JobObject
    {
        //All the native code for Jobobjects
        private static class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32", CharSet=CharSet.Unicode)]
            public static extern IntPtr CreateJobObject(IntPtr securityInfo, string lpName);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool SetInformationJobObject(SafeJobHandle hJob, JobObjectInfoType infoType,
                IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool AssignProcessToJobObject(SafeJobHandle job, SafeProcessHandle process);

            [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "QueryInformationJobObject")]
            public static extern bool QueryInformationJobObject(SafeJobHandle jobHandle, JobObjectInfoType infoType,
                IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength, IntPtr lpReturnLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool TerminateJobObject(SafeJobHandle jobHandle, uint exitCode);
        }

        #region Win32
        // ReSharper disable InconsistentNaming

        [StructLayout(LayoutKind.Sequential)]
        internal struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public Jobobjectlimit LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [Flags]
        internal enum Jobobjectlimit : uint
        {
            // Basic Limits
            Workingset = 0x00000001,
            ProcessTime = 0x00000002,
            JobTime = 0x00000004,
            ActiveProcess = 0x00000008,
            Affinity = 0x00000010,
            PriorityClass = 0x00000020,
            PreserveJobTime = 0x00000040,
            SchedulingClass = 0x00000080,

            // Extended Limits
            ProcessMemory = 0x00000100,
            JobMemory = 0x00000200,
            DieOnUnhandledException = 0x00000400,
            BreakawayOk = 0x00000800,
            SilentBreakawayOk = 0x00001000,
            KillOnJobClose = 0x00002000,
            SubsetAffinity = 0x00004000,

            // Notification Limits
            JobReadBytes = 0x00010000,
            JobWriteBytes = 0x00020000,
            RateControl = 0x00040000,
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            public ulong nLength;
            public IntPtr lpSecurityDescriptor;
            public ulong bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }


        internal enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11,
            NetworkRateControlInformation = 32,
            CpuRateControlInformation = 15,
        }

        [Flags]
        internal enum JOB_OBJECT_NET_RATE_CONTROL_FLAGS : uint
        {
            JOB_OBJECT_NET_RATE_CONTROL_ENABLE = 0x1,
            JOB_OBJECT_NET_RATE_CONTROL_MAX_BANDWITH = 0x2,
            JOB_OBJECT_NET_RATE_CONTROL_DSCP_TAG = 0x4,
            JOB_OBJECT_NET_RATE_CONTROL_VALID_FLAGS = 0x7
        }

        /// <summary>
        /// You can only set the control of the network traffic on one job in a hierarchy of nested jobs. The settings that you specify apply to that job and the child jobs in the hierarchy for that job.
        /// The settings do not apply to the chain of jobs from the parent job up to the top of the hierarchy. 
        /// You can change the settings on the original job in the hierarchy on which you set rate control. 
        /// However, attempts to set values for the control of the network rate for any other jobs in the hierarchy, including the parent jobs, fail.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_NET_RATE_CONTROL_INFORMATION
        {
            /// <summary>
            /// The maximum bandwidth for outgoing network traffic for the job, in bytes.
            /// </summary>
            public ulong MaxBandwidth;
            public JOB_OBJECT_NET_RATE_CONTROL_FLAGS ControlFlags;
            public byte DstpTag;
        }

        [Flags]
        internal enum JobobjectCpuRateControlFlags : uint
        {
            /// <summary>
            /// This flag enables the job's CPU rate to be controlled based on weight or hard cap. You must set this value if you also set JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED, JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP, or JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE.
            /// </summary>
            JobObjectCpuRateControlEnable = 0x1,

            /// <summary>
            /// The job's CPU rate is calculated based on its relative weight to the weight of other jobs. If this flag is set, the Weight member contains more information. If this flag is clear, the CpuRate member contains more information.
            /// </summary>
            JobObjectCpuRateControlWeightBased = 0x2,

            /// <summary>
            /// The job's CPU rate is a hard limit. After the job reaches its CPU cycle limit for the current scheduling interval, no threads associated with the job will run until the next interval.
            /// </summary>
            JobObjectCpuRateControlHardCap = 0x4,

            /// <summary>
            /// Sends messages when the CPU rate for the job exceeds the rate limits for the job during the tolerance interval.
            /// </summary>
            JobObjectCpuRateControlNotify = 0x8,

            /// <summary>
            /// The CPU rate for the job is limited by minimum and maximum rates that you specify in the MinRate and MaxRate members.
            /// </summary>
            JobObjectCpuRateControlMinMaxRate = 0x10,
        }
        /// <summary>
        /// Contains CPU rate control information for a job object. This structure is used by the SetInformationJobObject and QueryInformationJobObject functions with the JobObjectCpuRateControlInformation information class.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct JobobjectCpuRateControlInformation
        {
            /// <summary>
            /// The scheduling policy for CPU rate control.
            /// </summary>
            [FieldOffset(0)]
            public JobobjectCpuRateControlFlags ControlFlags;

            /// <summary>
            /// Specifies the portion of processor cycles that the threads in a job object can use during each scheduling interval, as the number of cycles per 10,000 cycles. If the ControlFlags member specifies JOB_OBJECT_CPU_RATE_WEIGHT_BASED or JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE, this member is not used.
            /// </summary>
            [FieldOffset(4)]
            public uint CpuRate;

            /// <summary>
            /// If the ControlFlags member specifies JOB_OBJECT_CPU_RATE_WEIGHT_BASED, this member specifies the scheduling weight of the job object, which determines the share of processor time given to the job relative to other workloads on the processor.
            /// </summary>
            [FieldOffset(4)]
            public uint Weight;

            /// <summary>
            /// Specifies the minimum portion of the processor cycles that the threads in a job object can reserve during each scheduling interval. Specify this rate as a percentage times 100. For example, to set a minimum rate of 50%, specify 50 times 100, or 5,000.
            /// </summary>
            [FieldOffset(4)]
            public ushort MinRate;

            /// <summary>
            /// Specifies the maximum portion of processor cycles that the threads in a job object can use during each scheduling interval. Specify this rate as a percentage times 100. For example, to set a maximum rate of 50%, specify 50 times 100, or 5,000.
            /// </summary>
            [FieldOffset(6)]
            public ushort MaxRate;
        }

        // ReSharper restore InconsistentNaming
        #endregion
    }
}
