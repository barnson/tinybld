namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class ConfigurationDataManager
    {
        public ConfigurationDataManager()
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            this.RootRepositoryConfigurationFolder = Path.Combine(programData, @"tinybld");
            this.ServiceConfigurationPath = Path.Combine(programData, @"tinybld\config.json");
            this.ServerDataPath = Path.Combine(programData, @"tinybld\tb.data");
        }

        public string RootRepositoryConfigurationFolder { get; set; }

        public string ServiceConfigurationPath { get; set; }

        public string ServerDataPath { get; set; }

        public RepositoryManager[] Repositories { get; set; }

        public ServiceConfiguration ServerConfig { get; set; }

        public ServerData ServerData { get; set; }

        public ConfigurationDataManager Load()
        {
            this.ServerConfig = this.LoadServiceConfiguration();

            this.ServerData = this.LoadServerData();

            this.Repositories = this.LoadRepositories(this.ServerData.RepositoryData);

            // Point the server repo data to the data in the repo managers so updates to
            // the repo data get saved when this server configuration data is persisted.
            this.ServerData.RepositoryData = this.Repositories.Select(r => r.Data).ToArray();

            return this;
        }

        public ConfigurationDataManager Save()
        {
            if (this.ServerData != null)
            {
                this.ServerData.Save(this.ServerDataPath);
            }

            return this;
        }

        private RepositoryManager[] LoadRepositories(RepositoryData[] data)
        {
            var repositories = new List<RepositoryManager>();
            var gatherer = new ConfigurationGatherer("*.repo");
            foreach (string path in gatherer.GatherConfigurations(this.RootRepositoryConfigurationFolder))
            {
                repositories.Add(RepositoryManager.Create(path, data));
            }

            return repositories.ToArray();
        }

        private ServiceConfiguration LoadServiceConfiguration()
        {
            string serviceConfigPath = this.ServiceConfigurationPath;
            if (!File.Exists(serviceConfigPath))
            {
                return new ServiceConfiguration();
            }

            try
            {
                return ServiceConfiguration.Load(serviceConfigPath);
            }
            catch (ApplicationException) // TODO: catch the correct exception.
            {
                // TODO: Display a useful error message.
                throw;
            }
        }

        public ServerData LoadServerData()
        {
            string serverDataPath = this.ServerDataPath;
            if (!File.Exists(serverDataPath))
            {
                return new ServerData();
            }

            try
            {
                return ServerData.Load(serverDataPath);
            }
            catch (ApplicationException) // TODO: catch the correct exception.
            {
                // TODO: Display a useful error message.
                throw;
            }
        }
    }
}
