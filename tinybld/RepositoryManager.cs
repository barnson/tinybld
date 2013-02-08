namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    /// <summary>
    /// Manages the collection of repositories.
    /// </summary>
    public class RepositoryManager
    {
        public BuildManagerConfigurationGatherer GatherConfigurations { get; set; }

        public IRepository Repository { get; set; }

        public RepositoryData Data { get; set; }

        public BuildManager[] BuildManagers { get; set; }

        public RepositoryManager()
        {
            this.GatherConfigurations = new BuildManagerConfigurationGatherer("*.tbc");
        }

        /// <summary>
        /// Create a manager from repository configuration and data.
        /// </summary>
        /// <param name="configPath">Path to configuration file that defines the repository.</param>
        /// <param name="data">Optional data that may contain previously persisted state for this repository.</param>
        /// <returns>New repository manager.</returns>
        public static RepositoryManager Create(string configPath, RepositoryData[] data)
        {
            RepositoryConfiguration config = RepositoryConfiguration.Load(configPath);

            RepositoryData datum = data.Where(d => d.RepositoryPath.Equals(config.Path, StringComparison.OrdinalIgnoreCase))
                                       .SingleOrDefault()
                                       ?? new RepositoryData()
                                          {
                                            RepositoryPath = config.Path,
                                          };

            if (String.IsNullOrEmpty(datum.LocalPath))
            {
                datum.LocalPath = Path.Combine("C:\\tinybuild.repos", config.Name);
            }

            IRepository repository;
            switch (config.Type)
            {
                case RepositoryType.Git:
                    repository = new GitRepository()
                    {
                        Branch = config.Branch,
                        LocalRepositoryPath = datum.LocalPath,
                        RemoteRepositoryPath = config.Path,
                    };
                    break;

                case RepositoryType.Hg:
                    throw new NotImplementedException();

                default:
                    throw new InvalidOperationException(String.Format("Repository configuration with path {0} specified an invalid type: {1}. Supported configuration is: git or mercurial", config.Path, config.Type));
            }

            return new RepositoryManager()
            {
                Repository = repository,
                Data = datum,
            };
        }

        /// <summary>
        /// Updates the repository if the interval has elapsed.
        /// </summary>
        /// <param name="interval">Interval between updates.</param>
        public void Update(TimeSpan interval)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(this.Data.LastUpdate) >= interval)
            {
                this.Data.LastChecked = now;

                if (this.Repository.Update())
                {
                    this.Data.LastUpdate = now;
                    this.BuildManagers = null;
                }
            }
        }

        /// <summary>
        /// Get the build manager that is ready to be built.
        /// </summary>
        /// <returns>Build manager to be built or null if no build manager is ready.</returns>
        public BuildManager GetNextReadyBuild()
        {
            if (!Directory.Exists(this.Repository.LocalRepositoryPath))
            {
                this.BuildManagers = null;
            }
            else if (this.BuildManagers == null)
            {
                List<BuildManager> buildManagers = new List<BuildManager>();

                var buildConfigPaths = this.GatherConfigurations.GatherConfigurations(this.Repository.LocalRepositoryPath);
                foreach (string path in buildConfigPaths)
                {
                    buildManagers.Add(BuildManager.Create(this.Repository.LocalRepositoryPath, path, this.Data.BuildData));
                }

                // Point the build data to the data in the build managers so updates
                // to the build data get saved when this repository is persisted.
                this.Data.BuildData = buildManagers.Select(b => b.Data).ToArray();

                this.BuildManagers = buildManagers.ToArray();
            }

            BuildManager ready = null;
            if (this.BuildManagers != null)
            {
                ready = this.BuildManagers.OrderByDescending(b => b.Data.LastBuild)
                                          .Where(b => b.BuildOutOfDate(this.Data.LastUpdate))
                                          .FirstOrDefault();
            }

            return ready;
        }

        private string HashBuildConfig(string path)
        {
            StringBuilder hash = new StringBuilder();

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] readBytes = File.ReadAllBytes(path);
                byte[] hashedBytes = sha1.ComputeHash(readBytes, 0, readBytes.Length);
                for (int i = 0; i < hashedBytes.Length; ++i)
                {
                    hash.AppendFormat("{0:X}", hashedBytes, i);
                }
            }

            return hash.ToString();
        }
    }
}
