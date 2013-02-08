namespace RobMensching.TinyBuild.Tests
{
    using RobMensching.TinyBuild.Tests.Helpers;
    using System;
    using System.IO;
    using Xunit;

    public class PathExtensionFixture : BaseFixture
    {
        [Fact]
        public void CanQuotePathsOnlyWhenNecessary()
        {
            Assert.Equal(@"C:\testpath\filename.exe", PathExtension.QuotePathIfNecessary(@"C:\testpath\filename.exe"));
            Assert.Equal(@"""C:\test path\file name.exe""", PathExtension.QuotePathIfNecessary(@"C:\test path\file name.exe"));
            Assert.Equal(@"""C:\test path\file name.exe""", PathExtension.QuotePathIfNecessary(@"""C:\test path\file name.exe"""));
        }

        [Fact]
        public void CanFindCmdExe()
        {
            string cmdPath = PathExtension.SearchPathForExecutable("cmd");
            Assert.Equal(".exe", Path.GetExtension(cmdPath), StringComparer.OrdinalIgnoreCase);
            Assert.True(File.Exists(cmdPath));
        }
    }
}
