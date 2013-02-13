namespace RobMensching.TinyBuild.Configuration
{
    using System;
    using System.IO;
    using ServiceStack.Text;

    public enum RepositoryType
    {
        Unknown,
        Git,
        Hg,
    }

    public class RepositoryConfiguration
    {
        public string Name { get; set; }

        public RepositoryType Type { get; set; }

        public string Path { get; set; }

        public string Branch { get; set; }

        public int Interval { get; set; }

        public string[] Properties { get; set; }

        internal static RepositoryConfiguration Load(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var config = JsonSerializer.DeserializeFromReader<RepositoryConfiguration>(reader);

                if (String.IsNullOrEmpty(config.Name))
                {
                    config.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                }

                return config;
            }
        }
    }
}
