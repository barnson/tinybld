namespace RobMensching.TinyBuild
{
    using System;
    using System.Threading;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class BuildService : IDisposable
    {
        private AppHost appHost;
        private Thread workerThread;
        private AutoResetEvent buildWait;

        public BuildService()
        {
            this.appHost = new AppHost(this);
            this.appHost.Init();

            this.buildWait = new AutoResetEvent(false);
            this.workerThread = new Thread(BuildService.Worker);
        }

        public ServerData ServerData { get; set; }

        public ServiceConfiguration ServerConfig { get; set; }

        public BuildStatus Status { get; set; }

        public static void Worker(object data)
        {
            BuildService buildService = (BuildService)data;

            int timeout = 0;
            while (!buildService.buildWait.WaitOne(timeout))
            {
                if (buildService.Status == BuildStatus.Waiting)
                {
                    buildService.Status = BuildStatus.Building;
                }
                else if (buildService.Status == BuildStatus.Building)
                {
                    buildService.Status = BuildStatus.Waiting;
                }

                timeout = 1000;
            }
        }

        public void Start()
        {
            this.Status = BuildStatus.Starting;

            int port = this.ServerConfig.Port == 0 ? 1337 : this.ServerConfig.Port;
            string listeningOn = String.Format("http://localhost:{0}/", port);
            appHost.Start(listeningOn);

            this.buildWait.Reset();
            this.workerThread.Start(this);

            this.Status = BuildStatus.Waiting;
        }

        public void Stop(int timeout = 5000)
        {
            this.Status = BuildStatus.Stopping;

            this.appHost.Stop();

            this.buildWait.Set();
            this.workerThread.Join(timeout);

            this.Status = BuildStatus.Stopped;
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
                if (this.Status == BuildStatus.Waiting)
                {
                    this.Stop(Timeout.Infinite);
                }

                if (this.appHost != null)
                {
                    this.appHost.Dispose();
                    this.appHost = null;
                }

                if (this.buildWait != null)
                {
                    this.buildWait.Dispose();
                    this.buildWait = null;
                }

                GC.SuppressFinalize(this);
            }
        }
    }
}
