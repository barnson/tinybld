namespace RobMensching.TinyBuild
{
    using System;

    public class HgRepository : IRepository
    {
        public string Branch { get; set; }

        public string RemoteRepositoryPath { get; set; }

        public string LocalRepositoryPath { get; set; }

        public DateTime LastUpdated { get; set; }

        public RepositoryChange[] Changes(string since = null, string until = null, string filename = null)
        {
            throw new NotImplementedException();
        }

        public RepositoryStatus Check()
        {
            throw new NotImplementedException();
        }

        public void Clean()
        {
            throw new NotImplementedException();
        }

        public bool Update(bool rebase = false)
        {
            throw new NotImplementedException();
        }
    }
}
