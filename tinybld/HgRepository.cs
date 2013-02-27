namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public enum HgReturnCode
    {
        Success = 0,
        NoRepository = 255,
    };

    public class HgRepository : IRepository
    {
        public string Branch { get; set; }

        public string RemoteRepositoryPath { get; set; }

        public string LocalRepositoryPath { get; set; }

        public DateTime LastUpdated { get; set; }

        public RepositoryChange[] Changes(string since = null, string until = null, string filename = null)
        {
            string revset = null;
            if (!String.IsNullOrEmpty(since) || !String.IsNullOrEmpty(until))
            {
                revset = String.Join(String.Empty,
                                     "--rev (", since, "..", until, ")",
                                     String.IsNullOrEmpty(since) ? null : "-" + since,
                                     String.IsNullOrEmpty(until) ? null : "-" + until);
            }

            var logArgs = new[]
                        {
                            "log --template \"changeset: {node}\nuser: {author}\ndate: {date|rfc3339date}\ndescription: \n{desc}\n\"",
                            revset,
                            filename,
                        };
            ProcessManager logCmd = new ProcessManager("hg", logArgs, this.LocalRepositoryPath).Run();
            if (logCmd.ExitCode != (int)GitReturnCode.Success)
            {
                throw new ApplicationException(String.Format("hg exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", logCmd.ExitCode, logCmd.StandardOutput, logCmd.StandardError));
            }

            List<RepositoryChange> changes = new List<RepositoryChange>();
            RepositoryChange change = null;
            bool description = false;
            foreach (var line in logCmd.StandardOutput.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("changeset:"))
                {
                    description = false;

                    change = new RepositoryChange()
                    {
                        Id = line.Substring(10).Trim(),
                    };

                    changes.Add(change);
                }
                else if (line.StartsWith("user:"))
                {
                    change.Author = line.Substring(6).Trim();
                }
                else if (line.StartsWith("date:"))
                {
                    change.Date = Convert.ToDateTime(line.Substring(6).Trim());
                }
                else if (line.StartsWith("description:"))
                {
                    description = true;
                }
                else if (description)
                {
                    change.Message = change.Message + (String.IsNullOrEmpty(change.Message) ? String.Empty : "\r\n") + line;
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

            ProcessManager incomingCmd = new ProcessManager("hg", "incoming", this.LocalRepositoryPath).Run();
            if (incomingCmd.ExitCode == 0)
            {
                return RepositoryStatus.OutOfDate;
            }
            else if (incomingCmd.ExitCode == 1)
            {
                return RepositoryStatus.UpToDate;
            }
            else if (incomingCmd.ExitCode == (int)HgReturnCode.NoRepository)
            {
                return RepositoryStatus.Absent;
            }

            return RepositoryStatus.Error;
        }

        public void Clean()
        {
            if (Directory.Exists(this.LocalRepositoryPath))
            {
                ProcessManager cleanCmd = new ProcessManager("hg", "clean --all", this.LocalRepositoryPath).Run();
            }
        }

        public bool Update(bool rebase = false)
        {
            ProcessManager pullCmd = null;
            if (Directory.Exists(this.LocalRepositoryPath))
            {
                var pullArgs = new[]
                    {
                        "pull",
                        "-u",
                        rebase ? "--rebase" : null,
                    };
                pullCmd = new ProcessManager("hg", pullArgs, this.LocalRepositoryPath).Run();
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

                ProcessManager cloneCmd = new ProcessManager("hg", cloneArgs).Run();
                if (cloneCmd.ExitCode != (int)GitReturnCode.Success)
                {
                    throw new ApplicationException(String.Format("hg exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", cloneCmd.ExitCode, cloneCmd.StandardOutput, cloneCmd.StandardError));
                }

                this.LastUpdated = DateTime.UtcNow;
                return true;
            }
            else if (pullCmd.ExitCode == (int)GitReturnCode.Success)
            {
                if (String.IsNullOrEmpty(pullCmd.StandardError) &&
                    (String.IsNullOrEmpty(pullCmd.StandardOutput) ||
                     pullCmd.StandardOutput.TrimEnd().EndsWith("no changes found", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                this.LastUpdated = DateTime.UtcNow;
                return true;
            }
            else
            {
                throw new ApplicationException(String.Format("hg exit code: {0}\r\nstdout:{1}\r\nstderr:{2}", pullCmd.ExitCode, pullCmd.StandardOutput, pullCmd.StandardError));
            }
        }
    }
}
