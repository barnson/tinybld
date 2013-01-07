namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading.Tasks;

    public partial class TinyBuildWindowsService : ServiceBase
    {
        Program program;

        public TinyBuildWindowsService(Program program)
        {
            this.program = program;
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.program.Start();
        }

        protected override void OnStop()
        {
            this.program.Stop();
        }
    }
}
