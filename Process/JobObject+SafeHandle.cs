using System;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Services.Core.Process
{
    public partial class JobObject
    {
        /// <summary>
        /// A class that encapsulates IntPtrs in a safe handle <see cref="SafeHandleZeroOrMinusOneIsInvalid"/>
        /// </summary>
        private sealed class SafeJobHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeJobHandle(IntPtr handle) : base(true)
            {
                SetHandle(handle);
            }

            protected override bool ReleaseHandle()
            {
                //Notify host of critical code that can compromise the integrity of the AppDomain
                Thread.BeginCriticalRegion();
                try
                {
                    return NativeMethods.CloseHandle(handle);
                }
                finally
                {
                    Thread.EndCriticalRegion();
                }
            }
        }
    }
}
