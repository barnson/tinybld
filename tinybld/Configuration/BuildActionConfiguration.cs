namespace RobMensching.TinyBuild.Configuration
{
    public class BuildActionConfiguration
    {
        public string Name { get; set; }

        public string Project { get; set; }

        public string[] Properties { get; set; }

        public string Target { get; set; }

        public string[] TestResults { get; set; }

        public bool ContinueOnError { get; set; }
    }
}
