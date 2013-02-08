namespace RobMensching.TinyBuild.Configuration
{
    public class XunitAction : BuildAction
    {
        public string TestAssembly { get; set; }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override ParsedBuildAction Deserialize()
        {
            return new ParsedBuildAction()
            {
                name = this.Name,
                xunittests = this.TestAssembly,
            };
        }
    }
}
