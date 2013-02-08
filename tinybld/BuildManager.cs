namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class BuildManager
    {
        private Queue<Builder> queue = new Queue<Builder>();

        public BuildConfiguration Config { get; set; }

        public BuildData Data { get; set; }

        public BuildStatus Status { get; set; }

        public static BuildManager Create(string repositoryPath, string configPath, BuildData[] data)
        {
            BuildConfiguration config = BuildConfiguration.Load(configPath);
            string relativeConfigPath = configPath.Substring(repositoryPath.Length).TrimStart(new[] { '\\' });

            BuildData datum = null;
            if (data != null)
            {
                datum = data.Where(d => d.Path.Equals(relativeConfigPath, StringComparison.OrdinalIgnoreCase))
                            .SingleOrDefault();
            }

            return new BuildManager()
                {
                    Config = config,
                    Data = datum ?? new BuildData() { Path = relativeConfigPath },
                };
        }

        /// <summary>
        /// Calculates the next build time.
        /// </summary>
        /// <returns>Date time for next build.</returns>
        public DateTime CalculateNextBuild()
        {
            DateTime nextBuild = this.Data.LastBuild;
            if (this.Config.Time.HasValue)
            {
                TimeSpan delta = nextBuild.TimeOfDay - this.Config.Time.Value;
                nextBuild = nextBuild.AddDays(delta.Ticks > 0 ? 1 : 0).Subtract(delta);
            }

            if (this.Config.Day.HasValue)
            {
                nextBuild = nextBuild.AddDays(Math.Abs(nextBuild.DayOfWeek - this.Config.Day.Value));
            }

            return nextBuild;
        }

        public bool BuildOutOfDate(DateTime lastUpdated)
        {
            DateTime nextBuild = CalculateNextBuild();
            return lastUpdated > nextBuild;
        }

        public void Queue(Builder builder)
        {
            if (!queue.Contains(builder))
            {
                queue.Enqueue(builder);
            }
        }
    }
}
