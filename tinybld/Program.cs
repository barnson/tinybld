namespace RobMensching.TinyBuild
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using System.Threading;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class Program
    {
        private static bool runAsService;
        private static AutoResetEvent consoleWait = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (String.Equals("-svc", args[i], StringComparison.OrdinalIgnoreCase))
                {
                    Program.runAsService = true;
                }
            }


            var serverData = ServerData.Load("tb.json");

            ServiceConfiguration serviceConfig = LoadServiceConfiguration();
            using (BuildService buildService = new BuildService()
                {
                    ServerConfig = serviceConfig,
                    ServerData = serverData,
                })
            {
                if (Program.runAsService)
                {
                    using (var svc = new WindowsService(buildService))
                    {
                        ServiceBase.Run(svc);
                    }
                }
                else
                {
                    Console.CancelKeyPress += Console_CancelKeyPress;
                    buildService.Start();

                    Console.WriteLine("Press CTRL+C to exit tiny build.");

                    // Wait for user to cancel the build service.
                    Program.consoleWait.WaitOne();

                    buildService.Stop();
                }
            }
        }

        private static ServiceConfiguration LoadServiceConfiguration()
        {
            string serviceConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"tinybld\config.json");
            if (!File.Exists(serviceConfigPath))
            {
                return new ServiceConfiguration();
            }

            try
            {
                return ServiceConfiguration.Load(serviceConfigPath);
            }
            catch (ApplicationException) // TODO: catch the correct exception
            {
                // TODO: Display a useful error message.
                throw;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Program.consoleWait.Set();
            e.Cancel = true;
        }
    }
}
