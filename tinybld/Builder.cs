namespace RobMensching.TinyBuild
{
    using RobMensching.TinyBuild.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Builder
    {
        public BuildConfiguration Configuration { get; private set; }

        public string Hash { get; private set; }

        public string Path { get; private set; }

        public IRepository Repository { get; set; }

        public Builder Create(string path, string hash)
        {
            BuildConfiguration config = BuildConfiguration.Load(path);

            return new Builder()
            {
                Configuration = config,
                Hash = hash,
                Path = path.ToLowerInvariant(),
            };
        }
    }
}
