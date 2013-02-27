namespace RobMensching.TinyBuild.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using RobMensching.TinyBuild.Tests.Helpers;
    using Xunit;

    public class HgRepositoryFixture : BaseFixture
    {
        private static readonly string RemoteRepository = @"Resources\hgtestrepo";
        private static readonly string LocalTestProxyRepository;
        private static readonly string LocalRepository;

        static HgRepositoryFixture()
        {
            LocalTestProxyRepository = Path.Combine(Path.GetTempPath(), "tinybld_hgtest_proxy");
            LocalRepository = Path.Combine(Path.GetTempPath(), "tinybld_hgtest");
        }

        [Fact]
        public void CanDetectAbsentRepository()
        {
            var repo = CreateTestHgManager();

            Assert.Equal(RepositoryStatus.Absent, repo.Check());
            Assert.Equal(DateTime.MinValue, repo.LastUpdated);
            Assert.False(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanDetectEmptyRepository()
        {
            var repo = CreateTestHgManager();
            Directory.CreateDirectory(repo.LocalRepositoryPath);

            Assert.Equal(RepositoryStatus.Absent, repo.Check());
            Assert.Equal(DateTime.MinValue, repo.LastUpdated);
            Assert.True(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanCreateAbsentRepository()
        {
            var repo = CreateTestHgManager();
            Assert.True(repo.Update());
            Assert.True(DateTime.MinValue < repo.LastUpdated);
            Assert.True(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanDetectRepositoryStable()
        {
            var repo = CreateTestHgManager();
            Assert.True(repo.Update());
            DateTime updatedAt = repo.LastUpdated;

            Assert.Equal(RepositoryStatus.UpToDate, repo.Check());
            Assert.True(DateTime.MinValue < repo.LastUpdated);
            Assert.False(repo.Update());
            Assert.Equal(updatedAt, repo.LastUpdated);
        }

        [Fact]
        public void CanDetectRepositoryChanged()
        {
            var repo = CreateProxiedTestHgManager();
            repo.Update();

            using (var file = File.CreateText(Path.Combine(HgRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("hg", "add added.txt", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("hg", "commit -m added.txt", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check());
            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check()); // ensure the check doesn't make the repo up to date.
        }

        [Fact]
        public void CanUpdateChangedRepository()
        {
            var repo = CreateProxiedTestHgManager();
            repo.Update();
            var updatedAt = repo.LastUpdated;

            using (var file = File.CreateText(Path.Combine(HgRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("hg", "add added.txt", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("hg", "commit -m added.txt", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check());
            Assert.True(repo.Update());
            Assert.Equal(RepositoryStatus.UpToDate, repo.Check());
            Assert.True(updatedAt < repo.LastUpdated);
        }

        [Fact]
        public void CanGetChangesFromUpdatedRepository()
        {
            var repo = CreateProxiedTestHgManager();
            repo.Update();
            var updatedAt = repo.LastUpdated;

            using (var file = File.CreateText(Path.Combine(HgRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("hg", "add added.txt", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("hg", "commit -m \"commit added.txt\"", HgRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            RepositoryChange[] changes = repo.Changes();
            Assert.Equal(1, changes.Length);
            Assert.Equal("Added a.txt in original hgtestrepo.", changes[0].Message);

            Assert.True(repo.Update());

            changes = repo.Changes();
            Assert.Equal(2, changes.Length);
            Assert.False(String.IsNullOrEmpty(changes[0].Id));
            Assert.False(String.IsNullOrEmpty(changes[0].Author));
            Assert.Equal("commit added.txt", changes[0].Message);
            Assert.False(String.IsNullOrEmpty(changes[1].Id));
            Assert.False(String.IsNullOrEmpty(changes[1].Author));
            Assert.Equal("Added a.txt in original hgtestrepo.", changes[1].Message);
            Assert.True(changes[0].Date > changes[1].Date);

            changes = repo.Changes(changes[1].Id);
            Assert.Equal(1, changes.Length);
            Assert.False(String.IsNullOrEmpty(changes[0].Id));
            Assert.False(String.IsNullOrEmpty(changes[0].Author));
            Assert.Equal("commit added.txt", changes[0].Message);

            changes = repo.Changes(changes[0].Id);
            Assert.Equal(0, changes.Length);
        }

        private HgRepository CreateTestHgManager(string branch = null)
        {
            DeleteDirectory(HgRepositoryFixture.LocalRepository);
            RegisterForCleanup(HgRepositoryFixture.LocalRepository);

            return new HgRepository()
            {
                Branch = branch,
                LocalRepositoryPath = HgRepositoryFixture.LocalRepository,
                RemoteRepositoryPath = HgRepositoryFixture.RemoteRepository,
            };
        }

        private HgRepository CreateProxiedTestHgManager(string branch = null)
        {
            DeleteDirectory(HgRepositoryFixture.LocalTestProxyRepository);
            DeleteDirectory(HgRepositoryFixture.LocalRepository);
            RegisterForCleanup(HgRepositoryFixture.LocalTestProxyRepository);
            RegisterForCleanup(HgRepositoryFixture.LocalRepository);

            var testRepo = new HgRepository()
            {
                Branch = branch,
                LocalRepositoryPath = HgRepositoryFixture.LocalTestProxyRepository,
                RemoteRepositoryPath = HgRepositoryFixture.RemoteRepository,
            }.Update();

            return new HgRepository()
            {
                Branch = branch,
                LocalRepositoryPath = HgRepositoryFixture.LocalRepository,
                RemoteRepositoryPath = HgRepositoryFixture.LocalTestProxyRepository,
            };
        }
    }
}
