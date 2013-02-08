namespace RobMensching.TinyBuild
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Job to control group of processes.
    /// </summary>
    public class Job : IDisposable
    {
        private IntPtr handle;
        private bool disposed;

        /// <summary>
        /// Creates a job object.
        /// </summary>
        /// <param name="name">Optional name of the job. Default is null.</param>
        public Job(string name = null)
        {
            this.handle = NativeMethods.CreateJobObject(IntPtr.Zero, name);
            if (this.handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // TODO: This just creates a job that kills its children when closed, need to instead set all the timeout stuff.
            NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION { LimitFlags = NativeMethods.JobObjectBasicLimit.KillOnJobClose }
            };

            int lengthExtendedInfo = Marshal.SizeOf(typeof(NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(lengthExtendedInfo);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!NativeMethods.SetInformationJobObject(this.handle, NativeMethods.JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)lengthExtendedInfo))
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Disposable objects should have 
        /// </summary>
        ~Job()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Closes the job.
        /// </summary>
        public void Close()
        {
            NativeMethods.CloseHandle(handle);
            this.handle = IntPtr.Zero;
        }

        /// <summary>
        /// Adds a process to the job.
        /// </summary>
        /// <param name="process">Handle to process to add to the job.</param>
        public void AddProcess(IntPtr processHandle)
        {
            if (!NativeMethods.AssignProcessToJobObject(this.handle, processHandle))
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Adds a process to the job.
        /// </summary>
        /// <param name="process">Process id to add to the job.</param>
        public void AddProcess(int processId)
        {
            this.AddProcess(Process.GetProcessById(processId).Handle);
        }

        /// <summary>
        /// Adds a process to the job.
        /// </summary>
        /// <param name="process">Process object to add to the job.</param>
        public void AddProcess(Process process)
        {
            this.AddProcess(process.Handle);
        }

        /// <summary>
        /// Terminates all of the processes in a job.
        /// </summary>
        /// <param name="exitCode">Exit code all terminated processes will return.</param>
        public void TerminateProcesses(int exitCode)
        {
            if (!NativeMethods.TerminateJobObject(this.handle, (uint)exitCode))
            {
                throw new Win32Exception();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.Close();
                disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }
   }
}