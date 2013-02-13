namespace RobMensching.TinyBuild
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using System.Threading;
    using NLog;
    using RobMensching.TinyBuild.Configuration;
    using RobMensching.TinyBuild.Data;

    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static bool runAsService;
        private static AutoResetEvent consoleWait = new AutoResetEvent(false);

        static int Main(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (String.Equals("-svc", args[i], StringComparison.OrdinalIgnoreCase))
                {
                    Program.runAsService = true;
                }
            }

            logger.Info("TinyBuild loading configuration.");

            ConfigurationDataManager configuration;
            try
            {
                configuration = new ConfigurationDataManager().Load();
            }
            catch (Exception e)
            {
                logger.FatalException("Failed to parse configuration.", e);
                return 1;
            }

            using (BuildService buildService = new BuildService()
                {
                    Configuration = configuration,
                })
            {
                if (Program.runAsService)
                {
                    logger.Trace("TinyBuild running as service.");

                    using (var svc = new WindowsService(buildService))
                    {
                        ServiceBase.Run(svc);
                    }
                }
                else
                {
                    logger.Trace("TinyBuild running as console.");

                    Console.CancelKeyPress += Console_CancelKeyPress;
                    buildService.Start();

                    Console.WriteLine("Press CTRL+C to exit tiny build.");

                    // Wait for user to cancel the build service.
                    Program.consoleWait.WaitOne();

                    buildService.Stop();
                }
            }

            logger.Info("TinyBuild exiting.");
            return 0;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Program.consoleWait.Set();
            e.Cancel = true;
        }
    }
}
