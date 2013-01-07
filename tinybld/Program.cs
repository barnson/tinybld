using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RobMensching.TinyBuild
{
    public class Program
    {
        private static bool runAsService;
        private static AutoResetEvent consoleWait = new AutoResetEvent(false);
        private TinyBuildAppHost appHost;
        private Thread workerThread;
        private AutoResetEvent serviceWait;

        public Program()
        {
            this.appHost = new TinyBuildAppHost();
            this.appHost.Init();

            this.serviceWait = new AutoResetEvent(false);
            this.workerThread = new Thread(Program.BuildWorker);
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (String.Equals("-svc", args[i], StringComparison.OrdinalIgnoreCase))
                {
                    Program.runAsService = true;
                }
            }

            Program program = new Program();

            if (Program.runAsService)
            {
                ServiceBase.Run(new TinyBuildWindowsService(program));
            }
            else
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                program.Start();

                Console.WriteLine("Press CTRL+C to exit tiny build.");

                // Wait for user to cancel the build service.
                Program.consoleWait.WaitOne();

                program.Stop();
            }
        }

        internal void Start()
        {
            string listeningOn = "http://localhost:1337/";
            appHost.Start(listeningOn);

            this.serviceWait.Reset();
            this.workerThread.Start(this);
        }

        internal void Stop()
        {
            this.appHost.Stop();

            this.serviceWait.Set();
            this.workerThread.Join(5000);
        }

        private static void BuildWorker(object data)
        {
            Program program = (Program)data;

            int timeout = 0;
            while (!program.serviceWait.WaitOne(timeout))
            {
                timeout = 1000;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Program.consoleWait.Set();
            e.Cancel = true;
        }
    }
}
