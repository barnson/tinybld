namespace RobMensching.TinyBuild.Tests
{
    using System;
    using RobMensching.TinyBuild.Configuration;
    using Xunit;

    public class BuildConfigurationFixture
    {
        [Fact]
        public void CanReadEmptyConfig()
        {
            var config = BuildConfiguration.Load(@"Resources\empty.tbc.test");
            Assert.True(String.IsNullOrEmpty(config.Name));
            Assert.Null(config.Day);
            Assert.Null(config.Time);
        }

        [Fact]
        public void CanReadInterestingConfig()
        {
            var config = BuildConfiguration.Load(@"Resources\interesting.tbc.test");
            Assert.Equal("Interesting Test Project", config.Name);
            Assert.Equal(DayOfWeek.Friday, config.Day);
            Assert.Equal(new TimeSpan(14, 30, 0), config.Time);
            Assert.True(config.Force);
            Assert.Equal(2, config.Actions.Length);
            Assert.Equal("Build Project", config.Actions[0].Name);
            Assert.Equal("Release\\interesting.sln", config.Actions[0].Project);
            Assert.Equal("Configuration=Release", config.Actions[0].Properties[0]);
            Assert.Equal("test", config.Actions[1].Target);
            Assert.Equal(1, config.Actions[1].TestResults.Length);
            Assert.Equal(@"interesting.test\bin\Release\results.xml", config.Actions[1].TestResults[0]);
        }
    }
}
