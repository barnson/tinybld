namespace RobMensching.TinyBuild.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using RobMensching.TinyBuild.Configuration;
    using ServiceStack.Text;

    public class BuildConfiguration
    {
        public string Name { get; set; }

        public Trigger Trigger { get; set; }

        public BuildAction[] Actions { get; set; }

        public static BuildConfiguration Read(string path)
        {
            ParsedBuildConfig parsed;
            using (StreamReader reader = File.OpenText(path))
            {
               parsed = JsonSerializer.DeserializeFromReader<ParsedBuildConfig>(reader);
            }

            List<BuildAction> actions = new List<BuildAction>();
            foreach (ParsedBuildAction action in parsed.actions)
            {
                actions.Add(BuildAction.Create(action));
            }

            BuildConfiguration config = new BuildConfiguration()
            {
                Name = parsed.name,
                Trigger = new Trigger(parsed.interval, parsed.force),
                Actions = actions.ToArray(),
            };

            return config;
        }

        public BuildConfiguration Write(string path)
        {
            List<ParsedBuildAction> actions = new List<ParsedBuildAction>();
            if (this.Actions != null)
            {
                foreach (BuildAction action in this.Actions)
                {
                    actions.Add(action.Deserialize());
                }
            }

            ParsedBuildConfig parsed = new ParsedBuildConfig()
            {
                name = this.Name,
                actions = actions.ToArray(),
            };

            if (this.Trigger != null)
            {
                parsed.interval = (ParsedTriggerInterval)this.Trigger.Interval;
                parsed.force = this.Trigger.Force;
            }

            using (StreamWriter writer = File.CreateText(path))
            {
                JsonSerializer.SerializeToWriter<ParsedBuildConfig>(parsed, writer);
            }

            return this;
        }
    }
}
