namespace RobMensching.TinyBuild.Tests
{
    using System;
    using System.IO;
    using Xunit;

    public class ProcessManagerFixture
    {
        [Fact]
        public void CanGetTestCmdStartOutput()
        {
            ProcessManager pm = new ProcessManager("Resources\\test")
            {
                Arguments = "may the force be with you",
            }.Start();

            Assert.Equal("cmd.exe", pm.Executable);
            Assert.True(pm.Process.WaitForExit(10 * 1000));
            Assert.Equal(String.Empty, pm.StandardError);
            Assert.Equal("test arguments: may the force be with you\r\n", pm.StandardOutput);
        }

        [Fact]
        public void CanGetTestCmdRunOutput()
        {
            ProcessManager pm = new ProcessManager("Resources\\test")
            {
                Arguments = "these are not the droids you are looking for",
            }.Run();

            Assert.Equal(String.Empty, pm.StandardError);
            Assert.Equal("test arguments: these are not the droids you are looking for\r\n", pm.StandardOutput);
        }

        //[Fact]
        //public void CanGetTestCmdOutputInJob()
        //{
        //    using (Job job = new Job())
        //    {
        //        ProcessManager pm = new ProcessManager()
        //        {
        //            Executable = "test",
        //            Arguments = "a b c",
        //            Job = job,
        //        }.Start();

        //        Assert.Equal("cmd.exe", pm.Executable);
        //        Assert.True(pm.Process.WaitForExit(10 + 1000));
        //        Assert.Equal(String.Empty, pm.StandardError);
        //        Assert.Equal("test arguments: a b c\r\n", pm.StandardOutput);
        //    }
        //}
    }
}
