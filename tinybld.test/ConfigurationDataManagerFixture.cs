namespace RobMensching.TinyBuild.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class ConfigurationDataManagerFixture
    {
        [Fact]
        public void CanLoadRepositories()
        {
            var config = new ConfigurationDataManager()
                {
                    RootRepositoryConfigurationFolder = Path.GetFullPath("Resources"),
                    ServiceConfigurationPath = Path.GetFullPath("Resourcesnotfound.json"),
                    ServerDataPath = Path.GetFullPath("Resources/alsonotfound.json"),
                }.Load();
            Assert.Equal(2, config.Repositories.Length);
            Assert.IsType<GitRepository>(config.Repositories[0].Repository);
            Assert.Equal("https://github.com/robmen/tinybld.git", config.Repositories[0].Repository.RemoteRepositoryPath);
            Assert.IsType<HgRepository>(config.Repositories[1].Repository);
            Assert.Equal("wix40", config.Repositories[1].Repository.Branch);
            Assert.Equal("https://hg.codeplex.com/wix", config.Repositories[1].Repository.RemoteRepositoryPath);
            //Assert.Equal(1, config.Repositories[1].Properties.Length);
            //Assert.Equal("OfficialBuild=true", config.Repositories[1].Properties[0]);
        }
    }
}
