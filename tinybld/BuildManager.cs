namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class BuildManager
    {
        public string WorkingFolder { get; set; }

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
                    WorkingFolder = Path.GetFullPath(repositoryPath),
                };
        }

        /// <summary>
        /// Calculates the next build time.
        /// </summary>
        /// <returns>Date time for next build.</returns>
        public DateTime CalculateNextBuildTime()
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

        /// <summary>
        /// Determines if build is out of date based on when the last update was.
        /// </summary>
        /// <param name="lastUpdated">Time when source code was last updated.</param>
        /// <returns>True if build is required.</returns>
        public bool BuildOutOfDate(DateTime lastUpdated)
        {
            DateTime nextBuild = this.CalculateNextBuildTime();
            return lastUpdated >= nextBuild;
        }

        public void Build(BuildService buildService)
        {
            this.Status = BuildStatus.Building;

            this.Data.LastBuild = DateTime.UtcNow;

            try
            {
                foreach (var action in this.Config.Actions)
                {
                    MsbuildProcess msbuild = new MsbuildProcess(action.Project, action.Target, this.WorkingFolder);
                    msbuild.PopulateProperties(action.Properties);

                    msbuild.Build();

                    if (msbuild.ExitCode != 0 && !action.ContinueOnError)
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.Status = BuildStatus.Idle;
            }
        }
    }
}
