namespace RobMensching.TinyBuild
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Manages a process.
    /// </summary>
    public class ProcessManager
    {
        private StringBuilder errorOutput = new StringBuilder();
        private StringBuilder standardOutput = new StringBuilder();

        public ProcessManager(string executable, string arguments = null, string workingDirectory = null)
        {
            this.Executable = executable;
            this.Arguments = arguments;
            this.WorkingDirectory = workingDirectory;
        }

        public ProcessManager(string executable, string[] arguments, string workingDirectory = null)
        {
            this.Executable = executable;
            this.Arguments = String.Join(" ", arguments);
            this.WorkingDirectory = workingDirectory;
        }

        public Job Job { get; set; }

        public Process Process { get; private set; }

        public string Executable { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public string StandardError { get { return this.errorOutput.ToString(); } }

        public string StandardOutput { get { return this.standardOutput.ToString(); } }

        public int ExitCode { get { return this.Process.ExitCode; } }

        /// <summary>
        /// Starts the process and returns immediately.
        /// </summary>
        /// <remarks>This will replace a previously started process in the manager.</remarks>
        /// <returns>Process manager used to start the proces.</returns>
        public ProcessManager Start()
        {
            string executablePath = this.Executable;
            string extension = Path.GetExtension(executablePath);
            if (extension.Equals(String.Empty) ||
                !executablePath.Equals(Path.GetFullPath(executablePath), StringComparison.OrdinalIgnoreCase))
            {
                executablePath = PathExtension.SearchPathForExecutable(executablePath);
                if (executablePath == null)
                {
                    throw new FileNotFoundException("Could not locate executable on the PATH.", this.Executable);
                }

                extension = Path.GetExtension(executablePath);
            }

            if (extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                string batchFile = executablePath;
                batchFile = PathExtension.QuotePathIfNecessary(batchFile);

                this.Executable = executablePath = "cmd.exe";
                this.Arguments = "/c \"" + batchFile + " " + this.Arguments + "\"";
            }

            bool redirectOutput = Path.GetExtension(this.Executable).Equals(".exe", StringComparison.OrdinalIgnoreCase);

            this.Process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = this.Executable,
                    Arguments = this.Arguments,
                    WorkingDirectory = this.WorkingDirectory,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = redirectOutput,
                    RedirectStandardOutput = redirectOutput,
                    UseShellExecute = !redirectOutput,
                    WindowStyle = ProcessWindowStyle.Hidden,
                },
            };

            if (redirectOutput)
            {
                this.Process.OutputDataReceived += Process_OutputDataReceived;
                this.Process.ErrorDataReceived += Process_ErrorDataReceived;
            }

            this.Process.Start();

            if (redirectOutput)
            {
                this.Process.BeginOutputReadLine();
                this.Process.BeginErrorReadLine();
            }

            if (this.Job != null)
            {
                this.Job.AddProcess(this.Process);
            }

            return this;
        }

        /// <summary>
        /// Starts a process and waits for it to exit.
        /// </summary>
        /// <remarks>This will replace a previously run process in the manager.</remarks>
        /// <returns>Process manager used to run the process.</returns>
        public ProcessManager Run()
        {
            this.Start();
            this.Process.WaitForExit();

            return this;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.standardOutput.AppendLine(e.Data);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.errorOutput.AppendLine(e.Data);
            }
        }
    }
}
