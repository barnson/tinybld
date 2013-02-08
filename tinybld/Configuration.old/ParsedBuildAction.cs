namespace RobMensching.TinyBuild.Configuration
{
    public class ParsedBuildAction
    {
        public string name { get; set; }

        public string project { get; set; }

        public string target { get; set; }

        public string xunittests { get; set; }

        public string[] properties { get; set; }
    }
}
