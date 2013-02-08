namespace RobMensching.TinyBuild.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Runtime.CompilerServices;
    using System.Diagnostics;

    public class BaseFixture : IDisposable
    {
        private readonly List<string> cleanup = new List<string>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetFactName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        public void RegisterForCleanup(string path)
        {
            this.cleanup.Add(path);
        }

        public void Dispose()
        {
            foreach (string path in cleanup)
            {
                BaseFixture.DeleteDirectory(path);
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            string[] files = Directory.GetFiles(directoryPath);
            string[] dirs = Directory.GetDirectories(directoryPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                BaseFixture.DeleteDirectory(dir);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
            try
            {
                Directory.Delete(directoryPath, false);
            }
            catch (IOException ex)
            {
                throw new IOException(string.Format("{0}The directory '{1}' could not be deleted!" +
                                                    "{0}Most of the time, this is due to an external process accessing the files in the temporary repositories created during the test runs, and keeping a handle on the directory, thus preventing the deletion of those files." +
                                                    "{0}Known and common causes include:" +
                                                    "{0}- Windows Search Indexer (go to the Indexing Options, in the Windows Control Panel, and exclude the bin folder of LibGit2Sharp.Tests)" +
                                                    "{0}- Antivirus (exclude the bin folder of LibGit2Sharp.Tests from the paths scanned by your real-time antivirus){0}",
                    Environment.NewLine, Path.GetFullPath(directoryPath)), ex);
            }
        }
    }
}
