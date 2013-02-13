namespace RobMensching.TinyBuild.Configuration
{
    using System.IO;
    using ServiceStack.Text;

    public class ServiceConfiguration
    {
        public ServiceConfiguration()
        {
            this.Port = 1337;
        }

        public string BuildRoot { get; set; }

        public int Port { get; set; }

        public string[] Properties { get; set; }

        public static ServiceConfiguration Load(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var config = JsonSerializer.DeserializeFromReader<ServiceConfiguration>(reader);
                config.Port = config.Port == 0 ? 1337 : config.Port;

                return config;
            }
        }
    }
}
