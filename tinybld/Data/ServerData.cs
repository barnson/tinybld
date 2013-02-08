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
    }
}
