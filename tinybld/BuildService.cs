namespace RobMensching.TinyBuild
{
    using System;
    using System.Threading;
    using NLog;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class BuildService : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AppHost appHost;
        private Thread workerThread;
        private AutoResetEvent stopEvent;

        public BuildService()
        {
            this.appHost = new AppHost(this);
            this.appHost.Init();

            this.stopEvent = new AutoResetEvent(false);
            this.workerThread = new Thread(BuildService.Worker) { Name = "TinyBuild Worker Thread" };
        }

        public ConfigurationDataManager Configuration { get; set; }

        public ServiceStatus Status { get; set; }

        public static void Worker(object data)
        {
            BuildService buildService = (BuildService)data;

            logger.Info("TinyBuild worker thread started.");

            int timeout = 0;
            while (!buildService.stopEvent.WaitOne(timeout))
            {
                logger.Trace("TinyBuild worker thread checking for repo updates.");

                foreach (RepositoryManager repo in buildService.Configuration.Repositories)
                {
                    if (repo.Update())
                    {
                        logger.Trace("TinyBuild worker thread repo updated: {0}", repo.Data.RepositoryPath);
                    }

                    var buildMan = repo.GetNextReadyBuild();
                    if (buildMan != null)
                    {
                        logger.Trace("TinyBuild worker thread building: {0} in repo: {1}", buildMan.Config.Name, repo.Data.RepositoryPath);

                        ServiceStatus serviceStatus = buildService.Status;
                        try
                        {
                            if (buildService.Status < ServiceStatus.Building)
                            {
                                buildService.Status = ServiceStatus.Building;
                            }

                            if (buildService.Status == ServiceStatus.Building)
                            {
                                repo.Clean();
                            }

                            if (buildService.Status == ServiceStatus.Building)
                            {
                                buildMan.Build(buildService);
                            }
                        }
                        finally
                        {
                            if (buildService.Status == ServiceStatus.Building)
                            {
                                buildService.Status = serviceStatus;
                            }
                        }

                        buildService.Configuration.Save();
                    }
                }

                timeout = 1000;
            }

            logger.Info("TinyBuild service exiting.");
        }

        public void Start()
        {
            logger.Info("TinyBuild service starting...");

            this.Status = ServiceStatus.Starting;

            int port = this.Configuration.ServerConfig.Port;
            string listeningOn = String.Format("http://localhost:{0}/", port);
            appHost.Start(listeningOn);

            this.stopEvent.Reset();
            this.workerThread.Start(this);

            this.Status = ServiceStatus.Idle;
        }

        public void Stop(int timeout = 5000)
        {
            logger.Info("TinyBuild service stopping...");

            this.Status = ServiceStatus.Stopping;

            this.appHost.Stop();

            this.stopEvent.Set();
            this.workerThread.Join(timeout);

            this.Configuration.Save();

            this.Status = ServiceStatus.Stopped;

            logger.Info("TinyBuild service stopped.");
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Status < ServiceStatus.Stopping)
                {
                    this.Stop(Timeout.Infinite);
                }

                if (this.appHost != null)
                {
                    this.appHost.Dispose();
                    this.appHost = null;
                }

                if (this.stopEvent != null)
                {
                    this.stopEvent.Dispose();
                    this.stopEvent = null;
                }

                GC.SuppressFinalize(this);
            }
        }
    }
}
