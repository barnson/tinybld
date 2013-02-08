namespace RobMensching.TinyBuild.Tests
{
    using RobMensching.TinyBuild.Configuration;
    using Xunit;

    public class ServiceConfigurationFixture
    {
        [Fact]
        public void CanLoadEmptyServiceConfig()
        {
            var serviceConfig = ServiceConfiguration.Load(@"Resources\empty.tbconfig.json");
            Assert.Equal(0, serviceConfig.Port);
            Assert.Null(serviceConfig.Repositories);
        }

        [Fact]
        public void CanLoadInterestingServiceConfig()
        {
            var serviceConfig = ServiceConfiguration.Load(@"Resources\interesting.tbconfig.json");
            Assert.Equal(31337, serviceConfig.Port);
            Assert.Equal(2, serviceConfig.Repositories.Length);
            Assert.Equal(RepositoryType.Git, serviceConfig.Repositories[0].Type);
            Assert.Equal("https://github.com/robmen/tinybld.git", serviceConfig.Repositories[0].Path);
            Assert.Equal(RepositoryType.Hg, serviceConfig.Repositories[1].Type);
            Assert.Equal("wix40", serviceConfig.Repositories[1].Branch);
            Assert.Equal("https://hg.codeplex.com/wix", serviceConfig.Repositories[1].Path);
            Assert.Equal(1, serviceConfig.Repositories[1].Properties.Length);
            Assert.Equal("OfficialBuild=true", serviceConfig.Repositories[1].Properties[0]);
        }
    }
}
