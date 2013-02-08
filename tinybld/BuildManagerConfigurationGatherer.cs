namespace RobMensching.TinyBuild
{
    using System.Collections.Generic;
    using System.IO;

    public class BuildManagerConfigurationGatherer
    {
        private string search;

        public BuildManagerConfigurationGatherer(string search)
        {
            this.search = search;
        }

        public virtual IEnumerable<string> GatherConfigurations(string root)
        {
            return Directory.EnumerateFiles(root, this.search, SearchOption.AllDirectories);
        }
    }
}
