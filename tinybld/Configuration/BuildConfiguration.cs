namespace RobMensching.TinyBuild.Configuration
{
    using System;
    using System.IO;
    using ServiceStack.Text;

    public class BuildConfiguration
    {
        public string Path { get; private set; }

        public string Name { get; set; }

        public DayOfWeek? Day { get; set; }

        public TimeSpan? Time { get; set; }

        public bool Force { get; set; }

        public int PollInterval { get; set; }

        public string[] Properties { get; set; }

        public BuildActionConfiguration[] Actions { get; set; }

        public static BuildConfiguration Load(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var config = JsonSerializer.DeserializeFromReader<BuildConfiguration>(reader);
                config.Path = path;

                return config;
            }
        }
    }
}
