namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public enum GitReturnCode
    {
        Success = 0,
        NoRepository = 128,
    };

    /// <summary>
    /// Manages interactions with a git based repository.
    /// </summary>
    public class GitRepository : IRepository
    {
        public string Branch { get; set; }

        public string RemoteRepositoryPath { get; set; }

        public string LocalRepositoryPath { get; set; }

        public DateTime LastUpdated { get; set; }

        public RepositoryChange[] Changes(string since = null, string until = null, string filename = null)
        {
            var logArgs = new[]
                        {
                            "log",
                            "--date=rfc",
                            "--reverse",
                            String.IsNullOrEmpty(since) ? String.Empty : since + "..",
                            until ?? String.Empty,
                            filename ?? String.Empty,
                        };
            ProcessManager logCmd = new ProcessManager("git", logArgs, this.LocalRepositoryPath).Run();
            if (logCmd.ExitCode != (int)GitReturnCode.Success)
            {
                throw new ApplicationException(String.Format("git exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", logCmd.ExitCode, logCmd.StandardOutput, logCmd.StandardError));
            }

            List<RepositoryChange> changes = new List<RepositoryChange>();
            RepositoryChange change = null;
            foreach (var line in logCmd.StandardOutput.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("commit "))
                {
                    change = new RepositoryChange()
                    {
                        Id = line.Substring(7),
                    };

                    changes.Add(change);
                }
                else if (line.StartsWith("Author: "))
                {
                    change.Author = line.Substring(8);
                }
                else if (line.StartsWith("Date:   "))
                {
                    change.Date = Convert.ToDateTime(line.Substring(8));
                }
                else if (line.StartsWith("    "))
                {
                    change.Message = change.Message + (String.IsNullOrEmpty(change.Message) ? String.Empty : "\r\n") + line.Substring(4);
                }
            }

            return changes.ToArray();
        }

        /// <summary>
        /// Checks the current status of the local repository with
        /// respect to the remote repository.
        /// </summary>
        /// <returns>Status of the local repository.</returns>
        public RepositoryStatus Check()
        {
            if (!Directory.Exists(this.LocalRepositoryPath))
            {
                return RepositoryStatus.Absent;
            }

            ProcessManager fetchCmd = new ProcessManager("git", "fetch --dry-run", this.LocalRepositoryPath).Run();
            if (fetchCmd.ExitCode == (int)GitReturnCode.Success)
            {
                // If nothing was fetched, check to see if there are still diffs to be
                // merged from the origin. Technically speaking this is not necessary
                // since all of our fetches are done in dry run but this will catch
                // stuff that slips through.
                //
                // TODO: what about local changes that have not been pushed?
                if (String.IsNullOrEmpty(fetchCmd.StandardOutput) && String.IsNullOrEmpty(fetchCmd.StandardError))
                {
                    var logArgs = new[]
                        {
                            "log",
                            "..@{u}",
                            "--abbrev-commit",
                            "--format=oneline",
                        };
                    ProcessManager logCmd = new ProcessManager("git", logArgs, this.LocalRepositoryPath).Run();
                    if (logCmd.ExitCode != (int)GitReturnCode.Success)
                    {
                        throw new ApplicationException(String.Format("git exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", logCmd.ExitCode, logCmd.StandardOutput, logCmd.StandardError));
                    }

                    if (String.IsNullOrEmpty(logCmd.StandardOutput) && String.IsNullOrEmpty(logCmd.StandardError))
                    {
                        return RepositoryStatus.UpToDate;
                    }
                }

                return RepositoryStatus.OutOfDate;
            }
            else if (fetchCmd.ExitCode == (int)GitReturnCode.NoRepository)
            {
                return RepositoryStatus.Absent;
            }

            return RepositoryStatus.Error;
        }

        public void Clean()
        {
            ProcessManager cleanCmd = null;
            if (Directory.Exists(this.LocalRepositoryPath))
            {
                var args = new[]
                    {
                        "clean",
                        "-dxf",
                    };
                cleanCmd = new ProcessManager("git", args, this.LocalRepositoryPath).Run();
            }
        }

        /// <summary>
        /// Pull the changes from origin to make the local repository up to date.
        /// </summary>
        /// <param name="rebase">Rebase the changes instead of merging.</param>
        /// <returns>True if an update was completed, false if no changes were found.</returns>
        public bool Update(bool rebase = false)
        {
            ProcessManager pullCmd = null;
            if (Directory.Exists(this.LocalRepositoryPath))
            {
                var pullArgs = new[]
                    {
                        "pull",
                        rebase ? "--rebase" : null,
                    };
                pullCmd = new ProcessManager("git", pullArgs, this.LocalRepositoryPath).Run();
            }

            // No repository, clone from the remote.
            if (pullCmd == null || pullCmd.ExitCode == (int)GitReturnCode.NoRepository)
            {
                string[] cloneArgs = new[]
                {
                    "clone",
                    "-q",
                    String.IsNullOrEmpty(this.Branch) ? String.Empty : " -b " + this.Branch,
                    this.RemoteRepositoryPath,
                    this.LocalRepositoryPath,
                };

                ProcessManager cloneCmd = new ProcessManager("git", cloneArgs).Run();
                if (cloneCmd.ExitCode != (int)GitReturnCode.Success)
                {
                    throw new ApplicationException(String.Format("git exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", cloneCmd.ExitCode, cloneCmd.StandardOutput, cloneCmd.StandardError));
                }

                this.LastUpdated = DateTime.UtcNow;
                return true;
            }
            else if (pullCmd.ExitCode == (int)GitReturnCode.Success)
            {
                if (String.IsNullOrEmpty(pullCmd.StandardError) &&
                    (String.IsNullOrEmpty(pullCmd.StandardOutput) ||
                     pullCmd.StandardOutput.StartsWith("Already up-to-date.", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                this.LastUpdated = DateTime.UtcNow;
                return true;
            }
            else
            {
                throw new ApplicationException(String.Format("git exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", pullCmd.ExitCode, pullCmd.StandardOutput, pullCmd.StandardError));
            }
        }
    }
}
