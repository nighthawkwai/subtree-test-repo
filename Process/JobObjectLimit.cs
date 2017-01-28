using System;

namespace Microsoft.Services.Core.Process
{
    /// <summary>
    /// Use this class to describe the limits to set for a given object.
    /// </summary>
    public sealed class JobObjectLimit
    {
        private JobObject.JobobjectCpuRateControlInformation _cpuRateControlInformation = new JobObject.JobobjectCpuRateControlInformation();
        private JobObject.JOBOBJECT_EXTENDED_LIMIT_INFORMATION _extendedLimit = new JobObject.JOBOBJECT_EXTENDED_LIMIT_INFORMATION {BasicLimitInformation = new JobObject.JOBOBJECT_BASIC_LIMIT_INFORMATION()};

        /// <summary>
        /// Kills all processes in the job object and their descendent processes.
        /// </summary>
        public bool KillAllProcessOnJobClose
            => (_extendedLimit.BasicLimitInformation.LimitFlags & JobObject.Jobobjectlimit.KillOnJobClose) != 0;

        /// <summary>
        /// The minimum workset memory available to a single process in the job in bytes
        /// Set with <see cref="SetProcessworkingSetMemory"/>
        /// </summary>
        public ulong MinWorkingset => _extendedLimit.BasicLimitInformation.MinimumWorkingSetSize.ToUInt64();

        /// <summary>
        /// The maximum workset memory available to a single process in the job in bytes
        /// Set with <see cref="SetProcessworkingSetMemory"/>
        /// </summary>
        public ulong MaxWorkingSet => _extendedLimit.BasicLimitInformation.MaximumWorkingSetSize.ToUInt64();

        /// <summary>
        /// The maximum virtual memory available to a process in bytes
        /// <see cref="SetProcessCommitMemory"/>
        /// </summary>
        public ulong MaxProcessCommitSize => _extendedLimit.ProcessMemoryLimit.ToUInt64();

        /// <summary>
        /// The maximum number of active processes in the jobobject. Beyond that process creation will fail.
        /// <see cref="SetMaxActiveProcesses"/>
        /// </summary>
        public uint MaxActiveProcesses => _extendedLimit.BasicLimitInformation.ActiveProcessLimit;

        /// <summary>
        /// The percentage of CPU allowed to be consumed by all processes in the job.
        /// The number is percentage * 100. So 10% is 1000 
        /// Set with <see cref="SetCpuLimit(uint)"/>
        /// </summary>
        /// <remarks>
        /// You can use only One of the overloads <see cref="SetCpuLimit(uint)"/> or <see cref="SetCpuLimit(uint, uint)"/>
        /// </remarks>
        public uint MaxCpuPercentageLimit => _cpuRateControlInformation.CpuRate;

        /// <summary>
        /// The max percentage of CPU allowed to be consumed by all pricesses in the job.
        /// The number is percentage * 100. So 10% is 1000 
        /// Set with <see cref="SetCpuLimit(uint,uint)"/>. 
        /// </summary>
        /// <remarks>
        /// You can use only One of the overloads <see cref="SetCpuLimit(uint)"/> or <see cref="SetCpuLimit(uint, uint)"/>
        /// </remarks>
        public uint MaxCpuRangePercentage => _cpuRateControlInformation.MaxRate;

        /// <summary>
        /// The min percentage of CPU reserved for all process in the job in each scheduling cycle.
        /// The number is percentage * 100. So 10% is 1000 
        /// Set with <see cref="SetCpuLimit(uint,uint)"/>
        /// </summary>
        /// <remarks>
        /// You can use only One of the overloads <see cref="SetCpuLimit(uint)"/> or <see cref="SetCpuLimit(uint, uint)"/>
        /// </remarks>
        public uint MinCpuRangePercentage => _cpuRateControlInformation.MinRate;


        public JobObjectLimit KillAllProcessOnJobObjectClose(bool killAll)
        {
            if (killAll)
            {
                SetExtendedFlag(JobObject.Jobobjectlimit.KillOnJobClose);
            }
            else
            {
                ClearExtendedFlag(JobObject.Jobobjectlimit.KillOnJobClose);
            }
            return this;
        }

        /// <summary>
        /// Sets the max CPU allowed to be used in percentage * 100 by all processes in job object combined.
        /// So for 10% you would pass in 1000.
        /// Pass 0 to unset the value
        /// </summary>
        /// <param name="maxPercentage">max percentage * 100, so for 10%, it is 1000</param>
        /// <returns></returns>
        /// <remarks>
        /// Only one of the two overloads can be used <see cref="SetCpuLimit(uint)"/>
        /// or <see cref="SetCpuLimit(uint,uint)"/>
        /// </remarks>
        public JobObjectLimit SetCpuLimit(uint maxPercentage)
        {
            Contract.Requires<ArgumentException>(maxPercentage <= 10000);
            ClearCpuFlag();
            if (maxPercentage == 0)
            {
                _cpuRateControlInformation.CpuRate = 0;
                return this;
            }
            _cpuRateControlInformation.ControlFlags = JobObject.JobobjectCpuRateControlFlags.JobObjectCpuRateControlEnable |
                                   JobObject.JobobjectCpuRateControlFlags.JobObjectCpuRateControlHardCap;
            _cpuRateControlInformation.CpuRate = maxPercentage;
            return this;
        }


        /// <summary>
        /// Sets the min reserved and max CPU allowed to be used in percentage * 100 by all processes in job object combined.
        /// So for 10% you would pass in 1000.
        /// Pass 0,0 to unset the value.
        /// </summary>
        /// <param name="minPercentage">min percentage * 100</param>
        /// <param name="maxPercentage">max percentage * 100</param>
        /// <returns></returns>
        /// <remarks>
        /// Only one of the two overloads can be used <see cref="SetCpuLimit(uint)"/>
        /// or <see cref="SetCpuLimit(uint,uint)"/>
        /// </remarks>
        public JobObjectLimit SetCpuLimit(uint minPercentage, uint maxPercentage)
        {
            Contract.Requires<ArgumentException>(minPercentage <= 10000);
            Contract.Requires<ArgumentException>(maxPercentage <= 10000);
            Contract.Requires<ArgumentException>(minPercentage <= maxPercentage);
            ClearCpuFlag();
            if (maxPercentage == 0)
            {
                _cpuRateControlInformation.MinRate = 0;
                _cpuRateControlInformation.MaxRate = 0;
                return this;
            }
            _cpuRateControlInformation.ControlFlags = JobObject.JobobjectCpuRateControlFlags.JobObjectCpuRateControlEnable |
                                   JobObject.JobobjectCpuRateControlFlags.JobObjectCpuRateControlMinMaxRate;
            _cpuRateControlInformation.MinRate = (ushort) minPercentage;
            _cpuRateControlInformation.MaxRate = (ushort) maxPercentage;
            return this;
        }
        
        /// <summary>
        /// Set the maximum virtual memory in bytes allowed to be used per process in the job object
        /// </summary>
        /// <param name="maxCommitSize"></param>
        /// <returns></returns>
        public JobObjectLimit SetProcessCommitMemory(ulong maxCommitSize)
        {
            if (maxCommitSize == 0)
            {
                ClearExtendedFlag(JobObject.Jobobjectlimit.ProcessMemory);
                InitializeTo0(ref _extendedLimit.ProcessMemoryLimit);
                return this;
            }
            SetExtendedFlag(JobObject.Jobobjectlimit.ProcessMemory);
            _extendedLimit.ProcessMemoryLimit = new UIntPtr(maxCommitSize);
            return this;
        }

        /// <summary>
        /// Sets the maximum number of processes allowed in a job object. Process creation will fail after.
        /// Set 0 to unset the value.
        /// </summary>
        /// <param name="maxProcesses"></param>
        /// <returns></returns>
        public JobObjectLimit SetMaxActiveProcesses(uint maxProcesses)
        {
            if (maxProcesses == 0)
            {
                ClearExtendedFlag(JobObject.Jobobjectlimit.ActiveProcess);
                _extendedLimit.BasicLimitInformation.ActiveProcessLimit = 0;
                return this;
            }
            _extendedLimit.BasicLimitInformation.ActiveProcessLimit = maxProcesses;
            SetExtendedFlag(JobObject.Jobobjectlimit.ActiveProcess);
            return this;
        }

        /// <summary>
        /// Sets the working set memory range allowed per process.
        /// </summary>
        /// <param name="minProcessWorkingSet">min working set in bytes</param>
        /// <param name="maxProcessWorkingSet">max working set in bytes</param>
        /// <returns></returns>
        public JobObjectLimit SetProcessworkingSetMemory(ulong minProcessWorkingSet, ulong maxProcessWorkingSet)
        {
            if (minProcessWorkingSet == maxProcessWorkingSet && minProcessWorkingSet == 0)
            {
                ClearExtendedFlag(JobObject.Jobobjectlimit.Workingset);
                InitializeTo0(ref _extendedLimit.BasicLimitInformation.MinimumWorkingSetSize,ref _extendedLimit.BasicLimitInformation.MaximumWorkingSetSize);
                return this;
            }
            SetExtendedFlag(JobObject.Jobobjectlimit.Workingset);
            _extendedLimit.BasicLimitInformation.MaximumWorkingSetSize = new UIntPtr(maxProcessWorkingSet);
            _extendedLimit.BasicLimitInformation.MinimumWorkingSetSize = new UIntPtr(minProcessWorkingSet);
            return this;
        }

        private void InitializeTo0(ref UIntPtr ptr)
        {
            ptr = UIntPtr.Zero;
        }

        private void InitializeTo0(ref UIntPtr ptr, ref UIntPtr ptr1)
        {
            InitializeTo0(ref ptr);
            InitializeTo0(ref ptr1);
        }

        private void SetExtendedFlag(JobObject.Jobobjectlimit flagToSet)
        {
            _extendedLimit.BasicLimitInformation.LimitFlags = _extendedLimit.BasicLimitInformation.LimitFlags |
                                                              flagToSet;
        }
        private void ClearExtendedFlag(JobObject.Jobobjectlimit flagToClear)
        {
            _extendedLimit.BasicLimitInformation.LimitFlags = _extendedLimit.BasicLimitInformation.LimitFlags & ~(flagToClear);
        }

        private void ClearCpuFlag()
        {
            _cpuRateControlInformation.ControlFlags = 0;
        }

        internal void SetExtendendedLimitInfo(JobObject.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimit)
        {
            _extendedLimit = extendedLimit;
        }

        internal void SetCpuLimitInfo(JobObject.JobobjectCpuRateControlInformation cpuLimit)
        {
            _cpuRateControlInformation = cpuLimit;
        }

        internal JobObject.JOBOBJECT_EXTENDED_LIMIT_INFORMATION? ToExtendedLimitInformation()
        {
            if (_extendedLimit.BasicLimitInformation.LimitFlags != 0)
            {
                 return _extendedLimit;
            }
            return null;
        }

        internal JobObject.JobobjectCpuRateControlInformation? ToCpuRateControlInformation()
        {
            if (_cpuRateControlInformation.ControlFlags != 0)
            {
                return _cpuRateControlInformation;
            }
            return null;
        }
    }
}