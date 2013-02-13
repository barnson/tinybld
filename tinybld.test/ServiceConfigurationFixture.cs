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
            Assert.Null(serviceConfig.BuildRoot);
            Assert.Equal(1337, serviceConfig.Port);
            Assert.Null(serviceConfig.Properties);
        }

        [Fact]
        public void CanLoadInterestingServiceConfig()
        {
            var serviceConfig = ServiceConfiguration.Load(@"Resources\interesting.tbconfig.json");
            Assert.Equal(@"C:\builds", serviceConfig.BuildRoot);
            Assert.Equal(31337, serviceConfig.Port);
            Assert.Equal(2, serviceConfig.Properties.Length);
        }
    }
}
