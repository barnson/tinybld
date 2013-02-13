namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class MsbuildProcess
    {
        private static readonly string[] versions = new string[] { "v4.0.30319", "v3.5", "v2.0.50727" };

        public MsbuildProcess(string project = null, string target = null, string workingFolder = null)
        {
            this.Properties = new Dictionary<string, string>();
            this.Project = project;
            this.Target = target;
            this.WorkingFolder = workingFolder;
        }

        public string Project { get; set; }

        public string Target { get; set; }

        public string LogPath { get; set; }

        public IDictionary<string, string> Properties { get; private set; }

        public string WorkingFolder { get; set; }

        public int ExitCode { get; private set; }

        public string Output { get; private set; }

        protected virtual string GetMsbuildPath()
        {
            string frameworkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework");
            foreach (string version in versions)
            {
                string msbuildPath = Path.Combine(frameworkPath, version, "msbuild.exe");
                if (File.Exists(msbuildPath))
                {
                    return msbuildPath;
                }
            }

            throw new FileNotFoundException("msbuild.exe");
        }

        public MsbuildProcess Build()
        {
            string msbuildPath = this.GetMsbuildPath();
            string[] args = new[] 
                {
                    "/nologo",
                    this.Project,
                    String.IsNullOrEmpty(this.Target) ? null : "/t:" + this.Target,
                    String.IsNullOrEmpty(this.LogPath) ? null : "/flp:logfile=" + PathExtension.QuotePathIfNecessary(this.LogPath),
                    this.PropertiesForCommandLine(),
                };
            ProcessManager msbuild = new ProcessManager(msbuildPath, args, this.WorkingFolder).Run();

            this.Output = msbuild.StandardOutput;
            this.ExitCode = msbuild.ExitCode;

            return this;
        }

        public MsbuildProcess PopulateProperties(string[] propertyPairs)
        {
            foreach (string pair in propertyPairs.Where(s => !String.IsNullOrEmpty(s.Trim())))
            {
                string[] split = pair.Split(new[] { '=' });
                string key = split[0].Trim();

                if (!String.IsNullOrEmpty(key))
                {
                    string value = split.Count() > 1 ? split[1].Trim() : null;
                    if (String.IsNullOrEmpty(value))
                    {
                        this.Properties.Remove(key);
                    }
                    else
                    {
                        this.Properties[key] = value;
                    }
                }
            }

            return this;
        }

        public string PropertiesForCommandLine()
        {
            var props = this.Properties.Select(kvp => "/p:" + kvp.Key + "=" + PathExtension.QuotePathIfNecessary(kvp.Value));
            return String.Join(" ", props);
        }
    }
}
