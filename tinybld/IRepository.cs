namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;

    public enum RepositoryStatus
    {
        Error,
        Absent,
        OutOfDate,
        UpToDate,
    }

    /// <summary>
    /// Interface to repository interaction for tinybld.
    /// </summary>
    public interface IRepository
    {
        string Branch { get; set; }

        string RemoteRepositoryPath { get; set; }

        string LocalRepositoryPath { get; set; }

        DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets changes since and until.
        /// </summary>
        /// <param name="since">Optional point to start collecting changes.</param>
        /// <param name="until">Optional point to stop collectiong changes.</param>
        /// <param name="filename">Optional filename to collect changes.</param>
        /// <returns>Array of changes found between since and until.</returns>
        RepositoryChange[] Changes(string since = null, string until = null, string filename = null);

        /// <summary>
        /// Checks the current status of the local repository with
        /// respect to the remote repository.
        /// </summary>
        /// <returns>Status of the local repository.</returns>
        RepositoryStatus Check();

        /// <summary>
        /// Cleans the local repository of any non-committed files.
        /// </summary>
        void Clean();

        /// <summary>
        /// Pull the changes from origin to make the local repository up to date.
        /// </summary>
        /// <param name="rebase">Rebase the changes instead of merging.</param>
        /// <returns>True if an update was completed, false if no changes were found.</returns>
        bool Update(bool rebase = false);
    }
}
