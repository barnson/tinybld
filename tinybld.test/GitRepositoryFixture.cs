namespace RobMensching.TinyBuild.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using RobMensching.TinyBuild.Tests.Helpers;
    using Xunit;

    public class GitRepositoryFixture : BaseFixture
    {
        private static readonly string RemoteRepository = @"Resources\testrepo";
        private static readonly string LocalTestProxyRepository;
        private static readonly string LocalRepository;

        static GitRepositoryFixture()
        {
            LocalTestProxyRepository = Path.Combine(Path.GetTempPath(), "tinybld_test_proxy");
            LocalRepository = Path.Combine(Path.GetTempPath(), "tinybld_test");
        }

        [Fact]
        public void CanDetectAbsentRepository()
        {
            var repo = CreateTestGitManager();

            Assert.Equal(RepositoryStatus.Absent, repo.Check());
            Assert.Equal(DateTime.MinValue, repo.LastUpdated);
            Assert.False(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanDetectEmptyRepository()
        {
            var repo = CreateTestGitManager();
            Directory.CreateDirectory(repo.LocalRepositoryPath);

            Assert.Equal(RepositoryStatus.Absent, repo.Check());
            Assert.Equal(DateTime.MinValue, repo.LastUpdated);
            Assert.True(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanCreateAbsentRepository()
        {
            var repo = CreateTestGitManager();
            Assert.True(repo.Update());
            Assert.True(DateTime.MinValue < repo.LastUpdated);
            Assert.True(Directory.Exists(repo.LocalRepositoryPath));
        }

        [Fact]
        public void CanDetectRepositoryStable()
        {
            var repo = CreateTestGitManager();
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
            var repo = CreateProxiedTestGitManager();
            repo.Update();

            using (var file = File.CreateText(Path.Combine(GitRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("git", "add added.txt", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("git", "commit -m added.txt", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check());
            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check()); // ensure the check doesn't make the repo up to date.
        }

        [Fact]
        public void CanUpdateChangedRepository()
        {
            var repo = CreateProxiedTestGitManager();
            repo.Update();
            var updatedAt = repo.LastUpdated;

            using (var file = File.CreateText(Path.Combine(GitRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("git", "add added.txt", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("git", "commit -m added.txt", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            Assert.Equal(RepositoryStatus.OutOfDate, repo.Check());
            Assert.True(repo.Update());
            Assert.Equal(RepositoryStatus.UpToDate, repo.Check());
            Assert.True(updatedAt < repo.LastUpdated);
        }

        [Fact]
        public void CanGetChangesFromUpdatedRepository()
        {
            var repo = CreateProxiedTestGitManager();
            repo.Update();
            var updatedAt = repo.LastUpdated;

            using (var file = File.CreateText(Path.Combine(GitRepositoryFixture.LocalTestProxyRepository, "added.txt")))
            {
                file.WriteLine("This is added.txt");
            }

            var addCmd = new ProcessManager("git", "add added.txt", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, addCmd.ExitCode);
            var commitCmd = new ProcessManager("git", "commit -m \"commit added.txt\"", GitRepositoryFixture.LocalTestProxyRepository).Run();
            Assert.Equal(0, commitCmd.ExitCode);

            RepositoryChange[] changes = repo.Changes();
            Assert.Equal(1, changes.Length);
            Assert.Equal("Added a.txt in original testrepo.", changes[0].Message);

            Assert.True(repo.Update());

            changes = repo.Changes();
            Assert.Equal(2, changes.Length);
            Assert.False(String.IsNullOrEmpty(changes[0].Id));
            Assert.False(String.IsNullOrEmpty(changes[0].Author));
            Assert.Equal("Added a.txt in original testrepo.", changes[0].Message);
            Assert.False(String.IsNullOrEmpty(changes[1].Id));
            Assert.False(String.IsNullOrEmpty(changes[1].Author));
            Assert.Equal("commit added.txt", changes[1].Message);
            Assert.True(changes[0].Date < changes[1].Date);

            changes = repo.Changes(changes[0].Id);
            Assert.Equal(1, changes.Length);
            Assert.False(String.IsNullOrEmpty(changes[0].Id));
            Assert.False(String.IsNullOrEmpty(changes[0].Author));
            Assert.Equal("commit added.txt", changes[0].Message);

            changes = repo.Changes(changes[0].Id);
            Assert.Equal(0, changes.Length);
        }

        private GitRepository CreateTestGitManager(string branch = null)
        {
            DeleteDirectory(GitRepositoryFixture.LocalRepository);
            RegisterForCleanup(GitRepositoryFixture.LocalRepository);

            return new GitRepository()
            {
                Branch = branch,
                LocalRepositoryPath = GitRepositoryFixture.LocalRepository,
                RemoteRepositoryPath = GitRepositoryFixture.RemoteRepository,
            };
        }

        private GitRepository CreateProxiedTestGitManager(string branch = null)
        {
            DeleteDirectory(GitRepositoryFixture.LocalTestProxyRepository);
            DeleteDirectory(GitRepositoryFixture.LocalRepository);
            RegisterForCleanup(GitRepositoryFixture.LocalTestProxyRepository);
            RegisterForCleanup(GitRepositoryFixture.LocalRepository);

            var testRepo = new GitRepository()
            {
                Branch = branch,
                LocalRepositoryPath = GitRepositoryFixture.LocalTestProxyRepository,
                RemoteRepositoryPath = GitRepositoryFixture.RemoteRepository,
            }.Update();

            return new GitRepository()
            {
                Branch = branch,
                LocalRepositoryPath = GitRepositoryFixture.LocalRepository,
                RemoteRepositoryPath = GitRepositoryFixture.LocalTestProxyRepository,
            };
        }
    }
}
