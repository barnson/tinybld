namespace RobMensching.TinyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class RepositoryChange
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public DateTime Date { get; set; }

        public string Message { get; set; }
    }
}
