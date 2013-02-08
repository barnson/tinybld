namespace RobMensching.TinyBuild.Configuration
{
    using System.IO;
    using ServiceStack.Text;

    public class ServiceConfiguration
    {
        public string BuildRoot { get; set; }

        public int Port { get; set; }

        public RepositoryConfiguration[] Repositories { get; set; }

        public string[] Properties { get; set; }

        public static ServiceConfiguration Load(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var config = JsonSerializer.DeserializeFromReader<ServiceConfiguration>(reader);
                return config;
            }
        }
    }
}
