namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ServiceStack.ServiceHost;

    public enum BuildStatus
    {
        Unknown,
        Waiting,
        Building,
        Starting,
        Stopped,
        Stopping,
    }

    [Route("/status")]
    public class Status
    {
        public BuildStatus BuildStatus { get; set; }
    }

    [Route("/statuses")]
    public class TinyBuildStatusList : IReturn<List<TinyBuildStatus>>
    {
    }

    [Route("/status/{Id}")]
    public class TinyBuildStatus
    {
        public string Id { get; set; }

        public string Description { get; set; }
    }
}
