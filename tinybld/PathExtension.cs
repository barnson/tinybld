namespace RobMensching.TinyBuild
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Methods that should be on System.IO.Path but are not.
    /// </summary>
    public static class PathExtension
    {
        /// <summary>
        /// Applies quotes around a path when it contains spaces.
        /// </summary>
        /// <param name="path">Path to quote if necessary.</param>
        /// <returns>Resulting path.</returns>
        public static string QuotePathIfNecessary(string path)
        {
            // TODO: This is a bit naive, since a file that starts or ends with
            // a quote would need to have the quote escaped. But this is good
            // enough for now.
            if (!path.StartsWith("\"", StringComparison.Ordinal) &&
                !path.EndsWith("\"", StringComparison.Ordinal) &&
                path.Contains(' '))
            {
                path = "\"" + path + "\"";
            }

            return path;
        }

        /// <summary>
        /// Searches the PATH for an executable.
        /// </summary>
        /// <param name="filename">Name of file to search along path.</param>
        /// <returns>Full path to executable if found, null otherwise.</returns>
        public static string SearchPathForExecutable(string filename)
        {
            string[] pathEnvironment = Environment.GetEnvironmentVariable("PATH").Split(';');
            string[] extensionEnvironment = Environment.GetEnvironmentVariable("PATHEXT").Split(';');

            var paths = new[] { Environment.CurrentDirectory }.Concat(pathEnvironment);
            var extensions = new[] { String.Empty }.Concat(extensionEnvironment.Where(e => e.StartsWith(".")));

            var combinations = paths.SelectMany(x => extensions, (path, extension) => Path.Combine(path, filename + extension));
            return combinations.FirstOrDefault(File.Exists);
        }
    }
}
