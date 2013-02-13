namespace RobMensching.TinyBuild.Tests
{
    using System;
    using RobMensching.TinyBuild.Data;
    using Xunit;

    public class RepositoryManagerFixture
    {
        [Fact]
        public void CanCreateSimpleRepositoryManager()
        {
            var repoMan = RepositoryManager.Create(@"Resources\gittest.repo", new RepositoryData[0]);
            Assert.Equal(@"C:\tinybuild.repos\gittest", repoMan.Data.LocalPath);
            Assert.Equal(@"C:\tinybuild.repos\gittest", repoMan.Repository.LocalRepositoryPath);
            Assert.Equal("https://github.com/robmen/tinybld.git", repoMan.Data.RepositoryPath);
            Assert.Equal("https://github.com/robmen/tinybld.git", repoMan.Repository.RemoteRepositoryPath);
            Assert.Null(repoMan.Repository.Branch);
        }

        [Fact]
        public void CanCreateRepositoryManagerWithData()
        {
            var repoData = new RepositoryData[]
            {
                new RepositoryData() { RepositoryPath = "ignored" },
                new RepositoryData() { RepositoryPath = "https://github.com/robmen/tinybld.git", LocalPath = @"C:\build\git" },
            };

            var repoMan = RepositoryManager.Create(@"Resources\gittest.repo", repoData);
            Assert.Equal(@"C:\build\git", repoMan.Data.LocalPath);
            Assert.Equal(@"C:\build\git", repoMan.Repository.LocalRepositoryPath);
        }

        [Fact]
        public void CanGetNextReadyBuild()
        {
            var repoMan = RepositoryManager.Create(@"Resources\gittest.repo", new []
                {
                    new RepositoryData()
                        {
                            RepositoryPath = "https://github.com/robmen/tinybld.git",
                            LocalPath = ".",
                            LastChecked = new DateTime(2013, 1, 4, 14, 45, 1),
                            LastUpdate = new DateTime(2013, 1, 4, 14, 45, 3),
                            BuildData = new [] 
                            {
                                new BuildData()
                                {
                                    Path = @"Resources\interesting.tbc.test",
                                    LastBuild = new DateTime(2013, 1, 1, 14, 23, 0),
                                },
                            },
                        },
                });
            repoMan.GatherConfigurations = new ConfigurationGatherer("interesting.tbc.test");

            var buildMan = repoMan.GetNextReadyBuild();
            Assert.NotNull(buildMan);
            Assert.Same(repoMan.Data.BuildData[0], buildMan.Data);
            Assert.True(buildMan.BuildOutOfDate(repoMan.Data.LastUpdate));
        }
    }
}
