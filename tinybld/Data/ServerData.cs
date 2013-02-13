namespace RobMensching.TinyBuild.Data
{
    using System.IO;
    using ServiceStack.Text;

    public class ServerData
    {
        public RepositoryData[] RepositoryData { get; set; }

        public static ServerData Load(string path)
        {
            using (StreamReader reader = File.OpenText(path))
            {
                var data = JsonSerializer.DeserializeFromReader<ServerData>(reader);
                return data;
            }
        }

        public ServerData Save(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directory.Exists)
            {
                directory.Create();
            }

            using (StreamWriter writer = File.CreateText(path))
            {
                JsonSerializer.SerializeToWriter<ServerData>(this, writer);
            }

            return this;
        }
    }
}
