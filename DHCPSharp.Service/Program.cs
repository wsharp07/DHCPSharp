using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.Common.Loggers;
using DHCPSharp.DAL;
using DHCPSharp.DAL.Models;
using DHCPSharp.DAL.Repository;
using DHCPSharp.Properties;
using SimpleInjector;
using SQLite;

namespace DHCPSharp
{
    static class Program
    {
        private static Container container;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var config = new SQLiteConfiguration();
            var conn = new SQLiteConnection(config.ConnectionString);
            conn.CreateTable<Lease>();

            Bootstrap();

            var dhcpServer = container.GetInstance<DhcpServer>();
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

        private static void Bootstrap()
        {
            // Create the container as usual.
            container = new Container();

            // Register your types, for instance:
            container.Register<IDbConfiguration, SQLiteConfiguration>();
            container.RegisterSingleton<IDhcpConfiguration>(LoadConfiguration);
            container.Register<ILeaseRepo, LeaseRepo>();
            container.Register<ILogger, ConsoleLogger>();
            container.Register<ILeaseManager, LeaseManager>();
            container.Register<LeaseCleanup>();

            // Optionally verify the container.
            container.Verify();
        }

        private static IDhcpConfiguration LoadConfiguration()
        {
            var gateway = Settings.Default.GatewayIpAddress;
            int leaseTime = Settings.Default.LeaseTimeSeconds;

            var leaseTimeSpan = leaseTime == 0
                ? TimeSpan.FromSeconds(uint.MaxValue)
                : TimeSpan.FromSeconds(leaseTime);
            var subnet = Settings.Default.SubnetIpAddress;
            var bindIp = Settings.Default.BindIpAddress;
            var startIp = Settings.Default.StartIpAddress;
            var endIp = Settings.Default.EndIpAddress;

            var config = new DhcpConfiguration()
            {
                Gateway = IPAddress.Parse(gateway),
                LeaseTime = leaseTimeSpan,
                SubnetMask = IPAddress.Parse(subnet),
                BindIp = IPAddress.Parse(bindIp),
                StartIpAddress = IPAddress.Parse(startIp),
                EndIpAddress = IPAddress.Parse(endIp)
            };

            return config;
        }
    }
}
