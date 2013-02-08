namespace RobMensching.TinyBuild.Data
{
    using System;

    public class RepositoryData
    {
        public string LocalPath { get; set; }

        public string RepositoryPath { get; set; }

        public DateTime LastChecked { get; set; }

        public DateTime LastUpdate { get; set; }

        public BuildData[] BuildData { get; set; }
    }
}
