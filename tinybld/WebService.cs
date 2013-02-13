namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ServiceStack.ServiceInterface;
    using ServiceStack.WebHost.Endpoints;

    public class WebService : Service
    {
        public BuildService BuildService { get; set; }

        public object Any(Status status)
        {
            return new Status() { ServiceStatus = this.BuildService.Status };
            //return this.BuildManager.Status;
        }

        public object Any(TinyBuildStatusList request)
        {
            return new List<TinyBuildStatus>() { new TinyBuildStatus { Id = "test", Description = "Just testing." } };
        }

        public object Any(TinyBuildStatus request)
        {
            return new TinyBuildStatus { Id = request.Id ?? "example", Description = "Requested id: " + request.Id ?? "none" };
        }
    }
}
