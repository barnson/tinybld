namespace RobMensching.TinyBuild
{
    using System.Collections.Generic;
    using ServiceStack.ServiceHost;

    public enum ServiceStatus
    {
        Unknown,
        Starting,
        Idle,
        Building,
        Stopping,
        Stopped,
    }

    public enum BuildStatus
    {
        Unknown,
        Idle,
        Queued,
        Building,
    }

    [Route("/status")]
    public class Status
    {
        public ServiceStatus ServiceStatus { get; set; }
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
