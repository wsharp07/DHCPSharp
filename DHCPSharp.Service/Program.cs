using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.Common.Loggers;

namespace DHCPSharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var dhcpServer = new DhcpServer(logger);
            var dhcpService = new DhcpHost(dhcpServer);

            if (Environment.UserInteractive)
            {
                dhcpService.ManualStart(args);
                Console.WriteLine("Press [Enter] key to exit...");
                Console.ReadLine();
                dhcpService.ManualStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    dhcpService
                };

                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
