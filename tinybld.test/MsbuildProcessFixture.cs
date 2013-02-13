namespace RobMensching.TinyBuild.Tests
{
    using System.IO;
    using Xunit;

    public class MsbuildProcessFixture
    {
        [Fact]
        public void CanAddProperties()
        {
            var msbuild = new MsbuildProcess().PopulateProperties(new[] { "a=1", " b = 2 ", "c= the letter 'c' ", "  d  =4" });
            Assert.Equal("1", msbuild.Properties["a"]);
            Assert.Equal("2", msbuild.Properties["b"]);
            Assert.Equal("the letter 'c'", msbuild.Properties["c"]);
            Assert.Equal("4", msbuild.Properties["d"]);
            Assert.False(msbuild.Properties.ContainsKey("z"));
            Assert.Equal(4, msbuild.Properties.Count);
        }

        [Fact]
        public void CanOverwriteProperties()
        {
            var msbuild = new MsbuildProcess().PopulateProperties(new[] { "a=1", " b = 2 " });
            Assert.Equal("1", msbuild.Properties["a"]);
            Assert.Equal("2", msbuild.Properties["b"]);
            Assert.Equal(2, msbuild.Properties.Count);

            msbuild.PopulateProperties(new[] { "c=3", "b=4" });
            Assert.Equal("1", msbuild.Properties["a"]);
            Assert.Equal("4", msbuild.Properties["b"]);
            Assert.Equal("3", msbuild.Properties["c"]);
            Assert.Equal(3, msbuild.Properties.Count);
        }

        [Fact]
        public void CanClearProperties()
        {
            var msbuild = new MsbuildProcess().PopulateProperties(new[] { "a=1", " b = 2 ", "=4" });
            Assert.Equal("1", msbuild.Properties["a"]);
            Assert.Equal("2", msbuild.Properties["b"]);
            Assert.Equal(2, msbuild.Properties.Count);

            msbuild.PopulateProperties(new[] { "c=3", "b=", "doesnotexist=" });
            Assert.Equal("1", msbuild.Properties["a"]);
            Assert.Equal("3", msbuild.Properties["c"]);
            Assert.False(msbuild.Properties.ContainsKey("b"));
            Assert.Equal(2, msbuild.Properties.Count);

            msbuild.PopulateProperties(new[] { "a=", "c" });
            Assert.Equal(0, msbuild.Properties.Count);
        }

        [Fact]
        public void CanCreateProperties()
        {
            var msbuild = new MsbuildProcess().PopulateProperties(new[] { "a=1", " b = foo bar " });
            Assert.Equal("/p:a=1 /p:b=\"foo bar\"", msbuild.PropertiesForCommandLine());
        }

        [Fact]
        public void CanBuildDefault()
        {
            var msbuild = new MsbuildProcess(@"Resources\test.proj").Build();
            Assert.Equal(0, msbuild.ExitCode);
            Assert.Contains("TestDefault:\r\n  TestProperty =  default", msbuild.Output);
        }

        [Fact]
        public void CanBuildError()
        {
            var msbuild = new MsbuildProcess(@"Resources\test.proj", "TestError").PopulateProperties(new[] { "TestProperty=foo" });
            msbuild.Build();
            Assert.Equal(1, msbuild.ExitCode);
            Assert.Contains("error : TestProperty = foo failed", msbuild.Output);
        }

        [Fact]
        public void CanBuildWarningWithLog()
        {
            const string logPath = "tinybld.tests.log";

            try
            {
                var msbuild = new MsbuildProcess(@"Resources\test.proj", "TestWarning") { LogPath = logPath };
                msbuild.PopulateProperties(new[] { " TestProperty  =  red blue  " }).Build();

                Assert.True(File.Exists(logPath));
                Assert.Contains(": warning : TestProperty = red blue warning", msbuild.Output);
                Assert.Equal(0, msbuild.ExitCode);
            }
            finally
            {
                File.Delete(logPath);
            }
        }
    }
}
