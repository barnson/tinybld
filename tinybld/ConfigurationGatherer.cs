namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigurationGatherer
    {
        private string search;

        public ConfigurationGatherer(string search)
        {
            this.search = search;
        }

        public virtual IEnumerable<string> GatherConfigurations(params string[] root)
        {
            return this.GatherConfigurations((IEnumerable<string>)root);
        }

        public virtual IEnumerable<string> GatherConfigurations(IEnumerable<string> roots)
        {
            foreach (string root in roots)
            {
                foreach (string folder in Directory.EnumerateFiles(root, this.search, SearchOption.AllDirectories))
                {
                    yield return folder;
                }
            }
        }
    }
}
