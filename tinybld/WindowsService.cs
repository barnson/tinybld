namespace RobMensching.TinyBuild
{
    using System;
    using System.ServiceProcess;

    public partial class WindowsService : ServiceBase
    {
        BuildService buildManager;

        public WindowsService(BuildService buildManager)
        {
            this.buildManager = buildManager;
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.buildManager.Start();
        }

        protected override void OnStop()
        {
            this.buildManager.Stop();
        }
    }
}
