namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ServiceStack.ServiceHost;

    [Route("/status")]
    public class TinyBuildStatusList : IReturn<List<TinyBuildStatus>> { }

    [Route("/status/{Id}")]
    public class TinyBuildStatus
    {
        public string Id { get; set; }

        public string Description { get; set; }
    }
}
