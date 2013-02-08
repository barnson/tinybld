namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ServiceStack.ServiceInterface;
    using ServiceStack.WebHost.Endpoints;

    public class AppHost : AppHostHttpListenerBase
    {
        private BuildService buildManager;

        public AppHost(BuildService buildManager)
            : base("TinyBuild Web Service", typeof(WebService).Assembly)
        {
            this.buildManager = buildManager;
        }

        public override void Configure(Funq.Container container)
        {
            container.Register(this.buildManager);

            //this.Routes.Add<IEnumerable<TinyBuildStatus>>("/status")
            //           .Add<TinyBuildStatus>("/status/{Id}");
        }
    }
}
