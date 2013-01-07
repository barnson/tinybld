namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ServiceStack.ServiceInterface;
    using ServiceStack.WebHost.Endpoints;

    public class TinyBuildAppHost : AppHostHttpListenerBase
    {
        public TinyBuildAppHost()
            : base("TinyBuild Web Service", typeof(TinyBuildWebService).Assembly)
        {
        }

        public override void Configure(Funq.Container container)
        {
            this.Routes.Add<IEnumerable<TinyBuildStatus>>("/status")
                       .Add<TinyBuildStatus>("/status/{Id}");
        }

    }
}
