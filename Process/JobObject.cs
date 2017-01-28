using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Services.Core.Process
{
    /// <summary>
    /// A class to manage windows job objects in C#. This can be used to sandbox processes or a group of processes.
    /// </summary>
    /// <example>
    /// Here is an example that adds the current process to a job object.
    /// When the using block ends the current process will die.
    /// <code>
    /// using (JobObject mgr = new JobObject("Foo"))
    /// {
    ///     mgr.SetJobLimits(new JobObjectLimit()
    ///         .KillAllProcessOnJobObjectClose(true);
    ///     mgr.AddProcess(Process.GetCurrentProcess());
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// An example to limit the memory usage to 40 MB.
    /// <code>
    /// JobObject mgr = new JobObject();
    /// mgr.SetJobLimits(new JobObjectLimit()
    ///     .SetProcessCommitMemory(40*1024*1024));
    /// mgr.AddProcess(Process.GetCurrentProcess());
    /// 
    /// while(true){
    /// //This infinite loop will not peg the CPU
    /// }
    /// </code>
    /// </example>
public sealed partial class JobObject : IDisposable
    {
        private readonly SafeJobHandle _handle;
        private bool _disposed;

        /// <summary>
        /// Creates a new windows job object. Named job objects can be operated cross process.
        /// </summary>
        /// <param name="jobObjectName"></param>
        public JobObject(string jobObjectName = null)
        {
            JobObjectName = jobObjectName;
            _handle = new SafeJobHandle(NativeMethods.CreateJobObject(IntPtr.Zero, jobObjectName));
        }

        /// <summary>
        /// Name of the job object. Null if it doesn't have a name
        /// </summary>
        public string JobObjectName { get; }

        /// <summary>
        /// Sets the limit prescribed by <see cref="JobObjectLimit"/>
        /// </summary>
        /// <param name="limit">limit</param>
        public void SetJobLimits(JobObjectLimit limit)
        {
            Contract.AssertArgNotNull(limit, nameof(limit));
            ValidateDisposed();
            var extendedInfo = limit.ToExtendedLimitInformation();
            var cpu = limit.ToCpuRateControlInformation();
            if (extendedInfo != null)
                SetJobLimit(JobObjectInfoType.ExtendedLimitInformation, extendedInfo.Value);
            if (cpu != null)
                SetJobLimit(JobObjectInfoType.CpuRateControlInformation, cpu.Value);

        }

        /// <summary>
        /// Terminates all processes in the job object with the specified exit code
        /// </summary>
        /// <param name="exitCode">exit code to terminate the processes with</param>
        public void TerminateAllProcessesInJob(int exitCode)
        {
            ValidateDisposed();
            NativeMethods.TerminateJobObject(_handle, (uint)exitCode);
        }

        /// <summary>
        /// Adds the process specified by the process handle to the job object
        /// </summary>
        /// <param name="processHandle"></param>
        public void AddProcess(SafeProcessHandle processHandle)
        {
            ValidateDisposed();
            if (!NativeMethods.AssignProcessToJobObject(_handle, processHandle))
            {
                Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Adds the process to the job object
        /// </summary>
        /// <param name="process"></param>
        public void AddProcess(System.Diagnostics.Process process)
        {
            AddProcess(process.SafeHandle);
        }

        /// <summary>
        /// Adds the process specified by the processId to the job object
        /// </summary>
        /// <param name="processId">Process id</param>
        public void AddProcess(int processId)
        {
            using (var process = System.Diagnostics.Process.GetProcessById(processId))
            {
                AddProcess(process);
            }
        }

        /// <summary>
        /// Gets the limits associated with current job object. This queries the current state of the job object.
        /// </summary>
        /// <returns></returns>
        public JobObjectLimit GetCurrentLimit()
        {
            JobObjectLimit limit = new JobObjectLimit();
            var extended = QueryJobLimit<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            var cpu = QueryJobLimit<JobobjectCpuRateControlInformation>();
            limit.SetExtendendedLimitInfo(extended);
            limit.SetCpuLimitInfo(cpu);
            return limit;
        }

        private T QueryJobLimit<T>() where T : struct
        {
            var len = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(len);
            try
            {
                var returnLenPtr = Marshal.AllocHGlobal(Marshal.SizeOf(new IntPtr()));
                JobObjectInfoType type;
                if (typeof(T) == typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION))
                {
                    type = JobObjectInfoType.ExtendedLimitInformation;
                }
                else if (typeof(T) == typeof(JobobjectCpuRateControlInformation))
                {
                    type = JobObjectInfoType.CpuRateControlInformation;
                }
                else
                {
                    throw new ArgumentException("Invalid type specified " + typeof(T).Name, nameof(T));
                }
                try
                {
                    if (!NativeMethods.QueryInformationJobObject(_handle, type,
                            ptr,
                            (uint)len,
                            returnLenPtr))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                  
                    return Marshal.PtrToStructure<T>(ptr);
                }
                finally
                {
                    Marshal.FreeHGlobal(returnLenPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private void SetJobLimit<T>(JobObjectInfoType type, T limit)
        {
            var length = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(limit, ptr, false);
                if (!NativeMethods.SetInformationJobObject(_handle, type, ptr, (uint)length))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Disposes the job object
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _handle.Dispose();
            _disposed = true;
        }

        private void ValidateDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(JobObject));
        }
    }
}