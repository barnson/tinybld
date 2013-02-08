namespace RobMensching.TinyBuild.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum MsbuildVersion
    {
        Best,
        v2,
        v4,
    }

    public class MsbuildAction : BuildAction
    {
        public string Project { get; set; }

        public MsbuildVersion Version { get; set; }

        public string Target { get; set; }

        public string[] Properties { get; set; }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override ParsedBuildAction Deserialize()
        {
            return new ParsedBuildAction()
            {
                name = this.Name,
                project = this.Project,
                properties = this.Properties,
                target = this.Target,
            };
        }
    }
}
