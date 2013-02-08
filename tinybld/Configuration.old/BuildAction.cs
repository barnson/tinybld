namespace RobMensching.TinyBuild.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public abstract class BuildAction
    {
        public string  Name { get; set; }

        public string Executable { get; private set; }

        public string Arguments { get; private set; }

        public static BuildAction Create(ParsedBuildAction parsed)
        {
            BuildAction action;
            if (!String.IsNullOrEmpty(parsed.project))
            {
                var msbuild = new MsbuildAction();
                msbuild.Name = parsed.name;
                msbuild.Project = parsed.project;
                msbuild.Target = parsed.target;
                msbuild.Properties = parsed.properties;
                action = msbuild;
            }
            else if (!String.IsNullOrEmpty(parsed.xunittests))
            {
                var xunit = new XunitAction();
                xunit.Name = parsed.name;
                xunit.TestAssembly = parsed.xunittests;
                action = xunit;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return action;
        }

        public abstract ParsedBuildAction Deserialize();

        public abstract void Initialize();
    }
}
