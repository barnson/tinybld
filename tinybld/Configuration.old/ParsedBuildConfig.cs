namespace RobMensching.TinyBuild.Configuration
{
    public enum ParsedTriggerInterval
    {
        immediate,
        daily,
        weekly,
        monthly,
    }

    public class ParsedBuildConfig
    {
        public ParsedTriggerInterval interval { get; set; }

        public bool force { get; set; }

        public string name { get; set; }

        public string[] variables { get; set; }

        public ParsedBuildAction[] actions { get; set; }
    }
}
